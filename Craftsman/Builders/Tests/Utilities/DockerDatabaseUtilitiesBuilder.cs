namespace Craftsman.Builders
{
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using System;
    using System.IO.Abstractions;
    using System.Text;
    using static Helpers.ConsoleWriter;

    public class DockerDatabaseUtilitiesBuilder
    {
        public static void CreateClass(string solutionDirectory, string projectBaseName, IFileSystem fileSystem)
        {
            try
            {
                var classPath = ClassPathHelper.IntegrationTestUtilitiesClassPath(solutionDirectory, projectBaseName, "DockerDatabaseUtilities.cs");

                if (!fileSystem.Directory.Exists(classPath.ClassDirectory))
                    fileSystem.Directory.CreateDirectory(classPath.ClassDirectory);

                if (fileSystem.File.Exists(classPath.FullClassPath))
                    throw new FileAlreadyExistsException(classPath.FullClassPath);

                using (var fs = fileSystem.File.Create(classPath.FullClassPath))
                {
                    var data = "";
                    data = GetBaseText(classPath.ClassNamespace, projectBaseName);
                    fs.Write(Encoding.UTF8.GetBytes(data));
                }

                GlobalSingleton.AddCreatedFile(classPath.FullClassPath.Replace($"{solutionDirectory}{fileSystem.Path.DirectorySeparatorChar}", ""));
            }
            catch (FileAlreadyExistsException e)
            {
                WriteError(e.Message);
                throw;
            }
            catch (Exception e)
            {
                WriteError($"An unhandled exception occurred when running the API command.\nThe error details are: \n{e.Message}");
                throw;
            }
        }

        public static string GetBaseText(string classNamespace, string projectBaseName)
        {
            return @$"// based on https://blog.dangl.me/archive/running-sql-server-integration-tests-in-net-core-projects-via-docker/

namespace {classNamespace}
{{
    using Docker.DotNet;
    using Docker.DotNet.Models;
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading.Tasks;

    public static class DockerSqlDatabaseUtilities
    {{
        public const string SQLSERVER_SA_PASSWORD = ""#testingDockerPassword#"";
        public const string SQLSERVER_IMAGE = ""mcr.microsoft.com/mssql/server"";
        public const string SQLSERVER_IMAGE_TAG = ""2017-latest"";
        public const string SQLSERVER_CONTAINER_NAME = ""IntegrationTestingContainer_{projectBaseName}"";
        public const string SQLSERVER_VOLUME_NAME = ""IntegrationTestingVolume_{projectBaseName}"";

        public static async Task<(string containerId, string port)> EnsureDockerStartedAndGetContainerIdAndPortAsync()
        {{
            await CleanupRunningContainers();
            await CleanupRunningVolumes();
            var dockerClient = GetDockerClient();
            var freePort = GetFreePort();

            // This call ensures that the latest SQL Server Docker image is pulled
            await dockerClient.Images.CreateImageAsync(new ImagesCreateParameters
            {{
                FromImage = $""{{SQLSERVER_IMAGE}}:{{SQLSERVER_IMAGE_TAG}}""
            }}, null, new Progress<JSONMessage>());

            // create a volume, if one doesn't already exist
            var volumeList = await dockerClient.Volumes.ListAsync();
            var volumeCount = volumeList.Volumes.Where(v => v.Name == SQLSERVER_VOLUME_NAME).Count();
            if(volumeCount <= 0)
            {{
                await dockerClient.Volumes.CreateAsync(new VolumesCreateParameters
                {{
                    Name = SQLSERVER_VOLUME_NAME,
                }});
            }}

            // create container, if one doesn't already exist
            var contList = await dockerClient
                .Containers.ListContainersAsync(new ContainersListParameters() {{ All = true }});
            var existingCont = contList
                .Where(c => c.Names.Any(n => n.Contains(SQLSERVER_CONTAINER_NAME))).FirstOrDefault();

            if (existingCont == null)
            {{
                var sqlContainer = await dockerClient
                    .Containers
                    .CreateContainerAsync(new CreateContainerParameters
                    {{
                        Name = SQLSERVER_CONTAINER_NAME,
                        Image = $""{{SQLSERVER_IMAGE}}:{{SQLSERVER_IMAGE_TAG}}"",
                        Env = new List<string>
                        {{
                        ""ACCEPT_EULA=Y"",
                        $""SA_PASSWORD={{SQLSERVER_SA_PASSWORD}}""
                        }},
                        HostConfig = new HostConfig
                        {{
                            PortBindings = new Dictionary<string, IList<PortBinding>>
                            {{
                            {{
                                ""1433/tcp"",
                                new PortBinding[]
                                {{
                                    new PortBinding
                                    {{
                                        HostPort = freePort
                                    }}
                                }}
                            }}
                            }},
                            Binds = new List<string>
                            {{
                                $""{{SQLSERVER_VOLUME_NAME}}:/mytestdata""
                            }}
                        }},
                    }});

                await dockerClient
                    .Containers
                    .StartContainerAsync(sqlContainer.ID, new ContainerStartParameters());

                await WaitUntilDatabaseAvailableAsync(freePort);
                return (sqlContainer.ID, freePort);
            }}

            return (existingCont.ID, existingCont.Ports.FirstOrDefault().PublicPort.ToString());
        }}

        public static async Task EnsureDockerContainersStoppedAndRemovedAsync(string dockerContainerId)
        {{
            var dockerClient = GetDockerClient();
            await dockerClient.Containers
                .StopContainerAsync(dockerContainerId, new ContainerStopParameters());
            await dockerClient.Containers
                .RemoveContainerAsync(dockerContainerId, new ContainerRemoveParameters());
        }}

        public static async Task EnsureDockerVolumesRemovedAsync(string volumeName)
        {{
            var dockerClient = GetDockerClient();
            await dockerClient.Volumes.RemoveAsync(volumeName);
        }}

        private static DockerClient GetDockerClient()
        {{
            var dockerUri = IsRunningOnWindows()
                ? ""npipe://./pipe/docker_engine""
                : ""unix:///var/run/docker.sock"";
            return new DockerClientConfiguration(new Uri(dockerUri))
                .CreateClient();
        }}

        private static async Task CleanupRunningContainers(int hoursTillExpiration = -24)
        {{
            var dockerClient = GetDockerClient();

            var runningContainers = await dockerClient.Containers
                .ListContainersAsync(new ContainersListParameters());

            foreach (var runningContainer in runningContainers.Where(cont => cont.Names.Any(n => n.Contains(SQLSERVER_CONTAINER_NAME))))
            {{
                // Stopping all test containers that are expired -- defaulted to a day
                var expiration = hoursTillExpiration > 0 
                    ? hoursTillExpiration * -1 
                    : hoursTillExpiration;
                if (runningContainer.Created < DateTime.UtcNow.AddHours(expiration))
                {{
                    try
                    {{
                        await EnsureDockerContainersStoppedAndRemovedAsync(runningContainer.ID);
                    }}
                    catch
                    {{
                        // Ignoring failures to stop running containers
                    }}
                }}
            }}
        }}

        private static async Task CleanupRunningVolumes(int hoursTillExpiration = -24)
        {{
            var dockerClient = GetDockerClient();

            var runningVolumes = await dockerClient.Volumes.ListAsync();

            foreach (var runningVolume in runningVolumes.Volumes.Where(v => v.Name == SQLSERVER_VOLUME_NAME))
            {{
                // Stopping all test containers that are older than one hour, they likely failed to cleanup
                var expiration = hoursTillExpiration > 0
                    ? hoursTillExpiration * -1
                    : hoursTillExpiration;
                if (DateTime.Parse(runningVolume.CreatedAt) < DateTime.UtcNow.AddHours(expiration))
                {{
                    try
                    {{
                        await EnsureDockerVolumesRemovedAsync(runningVolume.Name);
                    }}
                    catch
                    {{
                        // Ignoring failures to stop running containers
                    }}
                }}
            }}
        }}

        private static async Task WaitUntilDatabaseAvailableAsync(string databasePort)
        {{
            var start = DateTime.UtcNow;
            const int maxWaitTimeSeconds = 60;
            var connectionEstablised = false;
            while (!connectionEstablised && start.AddSeconds(maxWaitTimeSeconds) > DateTime.UtcNow)
            {{
                try
                {{
                    var sqlConnectionString = $""Data Source=localhost,{{databasePort}};Integrated Security=False;User ID=SA;Password={{SQLSERVER_SA_PASSWORD}}"";
                    using var sqlConnection = new SqlConnection(sqlConnectionString);
                    await sqlConnection.OpenAsync();
                    connectionEstablised = true;
                }}
                catch
                {{
                    // If opening the SQL connection fails, SQL Server is not ready yet
                    await Task.Delay(500);
                }}
            }}

            if (!connectionEstablised)
            {{
                throw new Exception(""Connection to the SQL docker database could not be established within 60 seconds."");
            }}

            return;
        }}

        private static string GetFreePort()
        {{
            // From https://stackoverflow.com/a/150974/4190785
            var tcpListener = new TcpListener(IPAddress.Loopback, 0);
            tcpListener.Start();
            var port = ((IPEndPoint)tcpListener.LocalEndpoint).Port;
            tcpListener.Stop();
            return port.ToString();
        }}

        private static bool IsRunningOnWindows()
        {{
            return Environment.OSVersion.Platform == PlatformID.Win32NT;
        }}
    }}
}}";
        }
    }
}