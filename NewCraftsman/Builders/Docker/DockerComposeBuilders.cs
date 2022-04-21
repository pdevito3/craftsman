namespace NewCraftsman.Builders.Docker;

using System.IO;
using System.IO.Abstractions;
using Domain;
using Helpers;
using Services;

public class DockerComposeBuilders
{
    private readonly IFileSystem _fileSystem;
    private readonly ICraftsmanUtilities _utilities;

    public DockerComposeBuilders(ICraftsmanUtilities utilities, IFileSystem fileSystem)
    {
        _utilities = utilities;
        _fileSystem = fileSystem;
    }
    
    public void CreateDockerComposeSkeleton(string solutionDirectory)
    {
        var classPath = ClassPathHelper.SolutionClassPath(solutionDirectory, $"docker-compose.yaml");
        var fileText = GetDockerComposeSkeletonText();
        _utilities.CreateFile(classPath, fileText);
    }
    
    public void CreateDockerComposeDbSkeleton(string solutionDirectory)
    {
        var classPath = ClassPathHelper.SolutionClassPath(solutionDirectory, $"docker-compose.data.yaml");
        var fileText = GetDockerComposeSkeletonText();
        _utilities.CreateFile(classPath, fileText);
    }

    private static string GetDockerComposeSkeletonText()
    {
        return @$"version: '3.7'

services:
        
volumes:";
    }
    
    public void AddVolumeToDockerComposeDb(string solutionDirectory, DockerConfig dockerConfig)
    {
        var services = "";
        var volumes = GetDbVolumeAndServiceTextForCompose(dockerConfig, out var dbService);
  
        services += $@"
  {dockerConfig.DbHostName}:{dbService}";
        
        // TODO change back to `.data` and use a regular compose that can do apis, bffs, auth server as well
        // var classPath = ClassPathHelper.SolutionClassPath(solutionDirectory, $"docker-compose.data.yaml");
        var classPath = ClassPathHelper.SolutionClassPath(solutionDirectory, $"docker-compose.yaml");
  
        if (!_fileSystem.Directory.Exists(classPath.ClassDirectory))
            _fileSystem.Directory.CreateDirectory(classPath.ClassDirectory);

        if (!_fileSystem.File.Exists(classPath.FullClassPath))
            return; //don't want to require this
  
        var tempPath = $"{classPath.FullClassPath}temp";
        using (var input = _fileSystem.File.OpenText(classPath.FullClassPath))
        {
            using var output = _fileSystem.File.CreateText(tempPath);
            {
                string line;
                while (null != (line = input.ReadLine()))
                {
                    var newText = $"{line}";
                    if (line.Contains($"services:"))
                    {
                        newText += @$"{services}";
                    }
                    if (line.Contains($"volumes:"))
                    {
                        newText += @$"{volumes}";
                    }
  
                    output.WriteLine(newText);
                }
            }
        }
  
        // delete the old file and set the name of the new one to the original name
        _fileSystem.File.Delete(classPath.FullClassPath);
        _fileSystem.File.Move(tempPath, classPath.FullClassPath);
    }
    
    public void AddJaegerToDockerCompose(string solutionDirectory)
    {
        var services = $@"

  jaeger:
    image: jaegertracing/all-in-one:latest
#    port mappings: https://www.jaegertracing.io/docs/1.32/getting-started/
    ports:
      - ""5775:5775/udp""
      - ""6831:6831/udp""
      - ""6832:6832/udp""
      - ""5778:5778""
      - ""16686:16686""
      - ""14250:14250""
      - ""14268:14268""
      - ""14269:14269""
      - ""9411:9411""
";
        
        var classPath = ClassPathHelper.SolutionClassPath(solutionDirectory, $"docker-compose.yaml");

        if (!_fileSystem.Directory.Exists(classPath.ClassDirectory))
            _fileSystem.Directory.CreateDirectory(classPath.ClassDirectory);

        if (!_fileSystem.File.Exists(classPath.FullClassPath))
            return; //don't want to require this

        // don't add it again if it's already there
        var fileText = File.ReadAllText(classPath.FullClassPath);
        if (fileText.Contains($"jaegertracing"))
            return;
        
        var tempPath = $"{classPath.FullClassPath}temp";
        using (var input = _fileSystem.File.OpenText(classPath.FullClassPath))
        {
            using var output = _fileSystem.File.CreateText(tempPath);
            {
                string line;
                while (null != (line = input.ReadLine()))
                {
                    var newText = $"{line}";
                    if (line.Contains($"services:"))
                    {
                        newText += @$"{services}";
                    }

                    output.WriteLine(newText);
                }
            }
        }

        // delete the old file and set the name of the new one to the original name
        _fileSystem.File.Delete(classPath.FullClassPath);
        _fileSystem.File.Move(tempPath, classPath.FullClassPath);
    }
    
    // TODO add props to make this configurable
    public void AddRmqToDockerCompose(string solutionDirectory)
    {
        var services = $@"

  rmq-message-broker:
    image: masstransit/rabbitmq
    restart: always
    ports:
      - '15672:15672'
      - '5672:5672'
    environment:
      RABBITMQ_DEFAULT_USER: guest
      RABBITMQ_DEFAULT_PASS: guest
";
        
        var classPath = ClassPathHelper.SolutionClassPath(solutionDirectory, $"docker-compose.yaml");

        if (!_fileSystem.Directory.Exists(classPath.ClassDirectory))
            _fileSystem.Directory.CreateDirectory(classPath.ClassDirectory);

        if (!_fileSystem.File.Exists(classPath.FullClassPath))
            return; //don't want to require this

        // don't add it again if it's already there
        var fileText = File.ReadAllText(classPath.FullClassPath);
        if (fileText.Contains($"masstransit/rabbitmq"))
            return;
        
        var tempPath = $"{classPath.FullClassPath}temp";
        using (var input = _fileSystem.File.OpenText(classPath.FullClassPath))
        {
            using var output = _fileSystem.File.CreateText(tempPath);
            {
                string line;
                while (null != (line = input.ReadLine()))
                {
                    var newText = $"{line}";
                    if (line.Contains($"services:"))
                    {
                        newText += @$"{services}";
                    }

                    output.WriteLine(newText);
                }
            }
        }

        // delete the old file and set the name of the new one to the original name
        _fileSystem.File.Delete(classPath.FullClassPath);
        _fileSystem.File.Move(tempPath, classPath.FullClassPath);
        
        // now do docker compose data
        // AddRmqToComposeData(solutionDirectory, services);
    }

    private void AddRmqToComposeData(string solutionDirectory, string rmqServicesText)
    {
        var classPath = ClassPathHelper.SolutionClassPath(solutionDirectory, $"docker-compose.data.yaml");

        if (!_fileSystem.Directory.Exists(classPath.ClassDirectory))
            _fileSystem.Directory.CreateDirectory(classPath.ClassDirectory);

        if (!_fileSystem.File.Exists(classPath.FullClassPath))
            return;

        // don't add it again if it's already there
        var fileText = File.ReadAllText(classPath.FullClassPath);
        if (fileText.Contains($"masstransit/rabbitmq"))
            return;

        var tempPath = $"{classPath.FullClassPath}temp";
        using (var input = _fileSystem.File.OpenText(classPath.FullClassPath))
        {
            using var output = _fileSystem.File.CreateText(tempPath);
            {
                string line;
                while (null != (line = input.ReadLine()))
                {
                    var newText = $"{line}";
                    if (line.Contains($"services:"))
                    {
                        newText += @$"{rmqServicesText}";
                    }

                    output.WriteLine(newText);
                }
            }
        }

        // delete the old file and set the name of the new one to the original name
        _fileSystem.File.Delete(classPath.FullClassPath);
        _fileSystem.File.Move(tempPath, classPath.FullClassPath);
    }

     public void AddBoundaryToDockerCompose(string solutionDirectory, DockerConfig dockerConfig, string clientId, string clientSecret, string audience)
     {
         var services = "";
         var volumes = GetDbVolumeAndServiceTextForCompose(dockerConfig, out var dbService);

         // just add all env vars potentially needed. can be ignored or deleted if not needed. updated if vals change?
         services += $@"
   {dockerConfig.DbHostName}:{dbService}

   {dockerConfig.ApiServiceName}:
     build:
       context: .
       dockerfile: {dockerConfig.ProjectName}/src/{dockerConfig.ProjectName}/Dockerfile
     ports:
       - ""{dockerConfig.ApiPort}:8080""
     environment:
       ASPNETCORE_ENVIRONMENT: ""Development""
       DB_CONNECTION_STRING: ""{dockerConfig.DbConnectionStringCompose}""
       ASPNETCORE_URLS: ""https://+:8080;""
       ASPNETCORE_Kestrel__Certificates__Default__Path: ""/https/aspnetappcert.pfx""
       ASPNETCORE_Kestrel__Certificates__Default__Password: ""password""
       AUTH_AUDIENCE: {audience}
       AUTH_AUTHORITY: https://auth-server:{dockerConfig.AuthServerPort}
       AUTH_AUTHORIZATION_URL: https://auth-server:{dockerConfig.AuthServerPort}/connect/authorize
       AUTH_TOKEN_URL: https://auth-server:{dockerConfig.AuthServerPort}/connect/token
       AUTH_CLIENT_ID: {clientId}
       AUTH_CLIENT_SECRET: {clientSecret}
       RMQ_HOST: rabbitmq
       RMQ_VIRTUAL_HOST: /
       RMQ_USERNAME: guest
       RMQ_PASSWORD: guest

     volumes:
       - ~/.aspnet/https:/https:ro";
         
         var classPath = ClassPathHelper.SolutionClassPath(solutionDirectory, $"docker-compose.yaml");

         if (!_fileSystem.Directory.Exists(classPath.ClassDirectory))
             _fileSystem.Directory.CreateDirectory(classPath.ClassDirectory);

         if (!_fileSystem.File.Exists(classPath.FullClassPath))
             return; //don't want to require this

         var tempPath = $"{classPath.FullClassPath}temp";
         using (var input = File.OpenText(classPath.FullClassPath))
         {
             using (var output = new StreamWriter(tempPath))
             {
                 string line;
                 while (null != (line = input.ReadLine()))
                 {
                     var newText = $"{line}";
                     if (line.Contains($"services:"))
                     {
                         newText += @$"{services}";
                     }
                     if (line.Equals($"volumes:"))
                     {
                         newText += @$"{volumes}";
                     }

                     output.WriteLine(newText);
                 }
             }
         }

         // delete the old file and set the name of the new one to the original name
         _fileSystem.File.Delete(classPath.FullClassPath);
         _fileSystem.File.Move(tempPath, classPath.FullClassPath);
     }

    public void AddAuthServerToDockerCompose(string solutionDirectory, string projectName, int port)
    {
        var services = "";

        services += $@"
  auth-server:
    build:
      context: .
      dockerfile: {projectName}/Dockerfile
    ports:
      - ""{port}:8080""
    environment:
      ASPNETCORE_ENVIRONMENT: ""Development""
      ASPNETCORE_URLS: ""https://+:8080;""
      ASPNETCORE_Kestrel__Certificates__Default__Path: ""/https/aspnetappcert.pfx""
      ASPNETCORE_Kestrel__Certificates__Default__Password: ""password""

    volumes:
      - ~/.aspnet/https:/https:ro";
        
        var classPath = ClassPathHelper.SolutionClassPath(solutionDirectory, $"docker-compose.yaml");

        if (!_fileSystem.Directory.Exists(classPath.ClassDirectory))
            _fileSystem.Directory.CreateDirectory(classPath.ClassDirectory);

        if (!_fileSystem.File.Exists(classPath.FullClassPath))
            return; //don't want to require this

        var tempPath = $"{classPath.FullClassPath}temp";
        using (var input = _fileSystem.File.OpenText(classPath.FullClassPath))
        {
            using var output = _fileSystem.File.CreateText(tempPath);
            {
                string line;
                while (null != (line = input.ReadLine()))
                {
                    var newText = $"{line}";
                    if (line.Contains($"services:"))
                    {
                        newText += @$"{services}";
                    }

                    output.WriteLine(newText);
                }
            }
        }

        // delete the old file and set the name of the new one to the original name
        _fileSystem.File.Delete(classPath.FullClassPath);
        _fileSystem.File.Move(tempPath, classPath.FullClassPath);
    }
    
    private string GetDbVolumeAndServiceTextForCompose(DockerConfig dockerConfig, out string dbService)
    {
        var volumes = "";
        dbService = $@"
    image: postgres
    restart: always
    ports:
      - '{dockerConfig.DbPort}:5432'
    environment:
      POSTGRES_USER: {dockerConfig.DbUser}
      POSTGRES_PASSWORD: {dockerConfig.DbPassword}
      POSTGRES_DB: {dockerConfig.DbName}
    volumes:
      - {dockerConfig.VolumeName}:/var/lib/postgresql/data";
    
        if (dockerConfig.ProviderEnum == DbProvider.SqlServer)
        {
            dbService = @$"
    image: mcr.microsoft.com/mssql/server
    restart: always
    ports:
      - '{dockerConfig.DbPort}:1433'
    environment:
      - DB_USER={dockerConfig.DbUser}
      - SA_PASSWORD={dockerConfig.DbPassword}
      - DB_CONTAINER_NAME={dockerConfig.DbName}
      - ACCEPT_EULA=Y
    volumes:
      - {dockerConfig.VolumeName}:/var/lib/sqlserver/data";
        }
    
        volumes += $"{Environment.NewLine}  {dockerConfig.VolumeName}:";
        return volumes;
    }
}