namespace Craftsman.Builders.Tests.Utilities
{
    using Craftsman.Enums;
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using System;
    using System.IO.Abstractions;
    using System.Text;

    public class DockerUtilitiesBuilder
    {
        public static void CreateGeneralUtilityClass(string solutionDirectory, string projectBaseName, string provider, IFileSystem fileSystem)
        {
            var classPath = ClassPathHelper.IntegrationTestUtilitiesClassPath(solutionDirectory, projectBaseName, "DockerUtilities.cs");
            var data = GetBaseUtilityText(classPath.ClassNamespace, provider);
            Utilities.CreateFile(classPath, data, fileSystem);
        }

        public static void CreateDockerDatabaseUtilityClass(string solutionDirectory, string projectBaseName, string provider, IFileSystem fileSystem)
        {
            var classPath = ClassPathHelper.IntegrationTestUtilitiesClassPath(solutionDirectory, projectBaseName, "DockerDatabaseUtilities.cs");
            var data = GetDbUtilityText(classPath.ClassNamespace, provider, projectBaseName);
            Utilities.CreateFile(classPath, data, fileSystem);
        }

        private static string GetDbUtilityText(string classNamespace, string provider, string projectBaseName)
        {
            var providerPort = Enum.GetName(typeof(DbProvider), DbProvider.Postgres) == provider ? "5432" : "1433";
            var envList = Enum.GetName(typeof(DbProvider), DbProvider.Postgres) == provider
                ? $@".WithEnvironment(
                    $""POSTGRES_DB={{DB_NAME}}"",
                    $""POSTGRES_PASSWORD={{DB_PASSWORD}}"")"
                : @$".WithEnvironment(
                    ""ACCEPT_EULA=Y"",
                    $""SA_PASSWORD={{DB_PASSWORD}}"")";

            var constants = Enum.GetName(typeof(DbProvider), DbProvider.Postgres) == provider
                ? @$"public const string DB_PASSWORD = ""#testingDockerPassword#"";
    private const string DB_USER = ""postgres"";
    private const string DB_NAME = ""{projectBaseName}"";
    private static readonly ImageTag ImageTagForOs = new ImageTag(""postgres"", ""latest"");
    private const string DB_CONTAINER_NAME = ""IntegrationTesting_{projectBaseName}"";
    private const string DB_VOLUME_NAME = ""IntegrationTesting_{projectBaseName}"";"
                : @$"private const string DB_PASSWORD = ""#testingDockerPassword#"";
    private const string DB_USER = ""SA"";
    private static readonly ImageTag ImageTagForOs = GetImageTagForOs();
    private const string DB_CONTAINER_NAME = ""IntegrationTesting_{projectBaseName}"";
    private const string DB_VOLUME_NAME = ""IntegrationTesting_{projectBaseName}"";";

            var mountedVol = Enum.GetName(typeof(DbProvider), DbProvider.Postgres) == provider
                ? @$"""/var/lib/postgresql/data"""
                : @$"""/var/lib/sqlserver/data""";

            var dbConnection = Enum.GetName(typeof(DbProvider), DbProvider.Postgres) == provider
                ? $@"DockerUtilities.GetSqlConnectionString(port, DB_PASSWORD, DB_USER, DB_NAME);"
                : $@"DockerUtilities.GetSqlConnectionString(port, DB_PASSWORD, DB_USER);";

            return @$"namespace {classNamespace};

using System.Runtime.InteropServices;
using Ductus.FluentDocker.Builders;
using Ductus.FluentDocker.Model.Builders;
using Ductus.FluentDocker.Services;
using Ductus.FluentDocker.Services.Extensions;

public static class DockerDatabaseUtilities
{{
    private record ImageTag(string Image, string Tag);
    {constants}

    public static async Task<int> EnsureDockerStartedAndGetPortPortAsync()
    {{
        await DockerUtilities.CleanupRunningContainers(DB_CONTAINER_NAME);
        await DockerUtilities.CleanupRunningVolumes(DB_CONTAINER_NAME);
        var freePort = DockerUtilities.GetFreePort();

        var hosts = new Hosts().Discover();
        var docker = hosts.FirstOrDefault(x => x.IsNative) ?? hosts.FirstOrDefault(x => x.Name == ""default"");     

        // create a volume, if one doesn't already exist
        var volume = docker?.GetVolumes().FirstOrDefault(v => v.Name == DB_VOLUME_NAME) ?? new Builder()
            .UseVolume()
            .WithName(DB_VOLUME_NAME)
            .Build();

        // create container, if one doesn't already exist
        var existingContainer = docker?.GetContainers().FirstOrDefault(c => c.Name == DB_CONTAINER_NAME);

        if (existingContainer == null)
        {{
            var container = new Builder().UseContainer()
                .WithName(DB_CONTAINER_NAME)
                .UseImage($""{{ImageTagForOs.Image}}:{{ImageTagForOs.Tag}}"")
                .ExposePort(freePort, {providerPort})
                {envList}
                .WaitForPort(""{providerPort}/tcp"", 30000 /*30s*/)
                .MountVolume(volume, {mountedVol}, MountType.ReadWrite)
                .Build();
    
            container.Start();

            await DockerUtilities.WaitUntilDatabaseAvailableAsync(GetSqlConnectionString(freePort.ToString()));
            return freePort;
        }}

        return existingContainer.ToHostExposedEndpoint(""{providerPort}/tcp"").Port;
    }}

    // SQL Server 2019 does not work on macOS + M1 chip. So we use SQL Edge as a workaround until SQL Server 2022 is GA.
    // See https://github.com/pdevito3/craftsman/issues/53 for details.
    private static ImageTag GetImageTagForOs() 
    {{
        var sqlServerImageTag = new ImageTag(""mcr.microsoft.com/mssql/server"", ""2019-latest"");
        var sqlEdgeImageTag = new ImageTag(""mcr.microsoft.com/azure-sql-edge"", ""latest"");
        return IsRunningOnMacOsArm64() ? sqlEdgeImageTag : sqlServerImageTag;
    }}

    private static bool IsRunningOnMacOsArm64() 
    {{
        var isMacOs = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
        var cpuArch = RuntimeInformation.ProcessArchitecture;
        return isMacOs && cpuArch == Architecture.Arm64;
    }}

    public static string GetSqlConnectionString(string port)
    {{
        return {dbConnection}
    }}
}}";
        }

        private static string GetBaseUtilityText(string classNamespace, string provider)
        {
            var dbConnection = Enum.GetName(typeof(DbProvider), DbProvider.Postgres) == provider
                ? $@"new NpgsqlConnection(dbConnection);"
                : $@"new SqlConnection(dbConnection);";

            var dbConnectionStringMethod = Enum.GetName(typeof(DbProvider), DbProvider.Postgres) == provider
                ? $@"public static string GetSqlConnectionString(string port, string password, string user, string dbName)
    {{
        return new NpgsqlConnectionStringBuilder()
        {{
            Host = ""localhost"",
            Password = password,
            Username = user,
            Database = dbName,
            Port = int.Parse(port)
        }}.ToString();
    }}"
            : $@"public static string GetSqlConnectionString(string port, string password, string user)
    {{
        return $""Data Source=localhost,{{port}};"" +
            ""Integrated Security=False;"" +
            $""User ID={{user}};"" +
            $""Password={{password}}"";
    }}";

            var usingStatement = Enum.GetName(typeof(DbProvider), DbProvider.Postgres) == provider
                ? $@"
using Npgsql;"
                : @$"
using Microsoft.Data.SqlClient;";

            return @$"// based on https://blog.dangl.me/archive/running-sql-server-integration-tests-in-net-core-projects-via-docker/

namespace {classNamespace};

using Docker.DotNet;
using Docker.DotNet.Models;{usingStatement}
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

public static class DockerUtilities
{{
    private static bool IsRunningOnWindows()
    {{
        return Environment.OSVersion.Platform == PlatformID.Win32NT;
    }}

    public static DockerClient GetDockerClient()
    {{
        var dockerUri = IsRunningOnWindows()
            ? ""npipe://./pipe/docker_engine""
            : ""unix:///var/run/docker.sock"";
        return new DockerClientConfiguration(new Uri(dockerUri))
            .CreateClient();
    }}

    public static async Task CleanupRunningContainers( string containerName, int hoursTillExpiration = -24)
    {{
        var dockerClient = GetDockerClient();

        var runningContainers = await dockerClient.Containers
            .ListContainersAsync(new ContainersListParameters());

        foreach (var runningContainer in runningContainers.Where(cont => cont.Names.Any(n => n.Contains(containerName))))
        {{
            // Stopping all test containers that are older than 24 hours
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

    public static async Task CleanupRunningVolumes(string volumeName, int hoursTillExpiration = -24)
    {{
        var dockerClient = GetDockerClient();

        var runningVolumes = await dockerClient.Volumes.ListAsync();

        foreach (var runningVolume in runningVolumes.Volumes.Where(v => v.Name == volumeName))
        {{
            // Stopping all test volumes that are older than 24 hours
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

    public static async Task WaitUntilDatabaseAvailableAsync(string dbConnection)
    {{
        var start = DateTime.UtcNow;
        const int maxWaitTimeSeconds = 60;
        var connectionEstablished = false;
        while (!connectionEstablished && start.AddSeconds(maxWaitTimeSeconds) > DateTime.UtcNow)
        {{
            try
            {{
                using var sqlConnection = {dbConnection}
                await sqlConnection.OpenAsync();
                connectionEstablished = true;
            }}
            catch
            {{
                // If opening the SQL connection fails, SQL Server is not ready yet
                await Task.Delay(500);
            }}
        }}

        if (!connectionEstablished)
        {{
            throw new Exception($""Connection to the SQL docker database could not be established within {{maxWaitTimeSeconds}} seconds."");
        }}

        return;
    }}

    public static int GetFreePort()
    {{
        // From https://stackoverflow.com/a/150974/4190785
        var tcpListener = new TcpListener(IPAddress.Loopback, 0);
        tcpListener.Start();
        var port = ((IPEndPoint)tcpListener.LocalEndpoint).Port;
        tcpListener.Stop();
        return port;
    }}

    {dbConnectionStringMethod}
}}";
        }
    }
}
