namespace Craftsman.Builders.Docker;

using System;
using System.Collections.Generic;
using Craftsman.Helpers;
using System.IO.Abstractions;
using System.Linq;
using Enums;
using Models;

public class DockerBuilders
{
    public static void CreateDockerfile(string srcDirectory, string projectBaseName, IFileSystem fileSystem)
    {
        var classPath = ClassPathHelper.WebApiProjectRootClassPath(srcDirectory, $"Dockerfile", projectBaseName);
        var fileText = GetDockerfileText(projectBaseName);
        Utilities.CreateFile(classPath, fileText, fileSystem);
    }
    
    public static void CreateDockerCompose(string solutionDirectory, IEnumerable<DockerConfig> dockerConfigList, IFileSystem fileSystem)
    {
        var classPath = ClassPathHelper.SolutionClassPath(solutionDirectory, $"docker-compose.yaml");
        var fileText = GetDockerComposeText(dockerConfigList);
        Utilities.CreateFile(classPath, fileText, fileSystem);
    }
    
    public static void CreateDockerIgnore(string srcDirectory, string projectBaseName, IFileSystem fileSystem)
    {
        var classPath = ClassPathHelper.WebApiProjectRootClassPath(srcDirectory, $".dockerignore", projectBaseName);
        var fileText = GetDockerIgnoreText();
        Utilities.CreateFile(classPath, fileText, fileSystem);
    }

    private static string GetDockerfileText(string projectBaseName)
    {
        return @$"FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY [""{projectBaseName}.csproj"", ""./""]
RUN dotnet restore ""./{projectBaseName}.csproj""

# Copy everything else and build
COPY . ./
RUN dotnet build ""{projectBaseName}.csproj"" -c Release -o /app/build

FROM build-env AS publish
RUN dotnet publish ""{projectBaseName}.csproj"" -c Release -o /app/out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0
COPY --from=publish /app/out .

ENV ASPNETCORE_URLS=https://+:8080
EXPOSE 8080

ENTRYPOINT [""dotnet"", ""{projectBaseName}.dll""]
";
        
        /* if I want to do both 80 and 443, do
```            
ENV ASPNETCORE_URLS=http://+:80;https://+:443
EXPOSE 80
EXPOSE 443
```

also add an `80` port in `ports` in docker-compose.yaml like 
```
    ports:
      #- ""{dockerConfig.ApiPort-1}:80""
      - ""{dockerConfig.ApiPort}:443""
```

Then take off httpsredirect in startup


         */
    }

    private static string GetDockerComposeText(IEnumerable<DockerConfig> dockerConfigList)
    {
        var services = "";
        var volumes = "";
        foreach (var dockerConfig in dockerConfigList)
        {
            var postgresDbUser = dockerConfig.DbUser ?? "postgres";
            var postgresDbPassword = dockerConfig.DbPassword ?? "postgres";
            var dbService = $@"
    image: postgres
    restart: always
    ports:
      - '{dockerConfig.DbPort}:5432'
    environment:
      - POSTGRES_USER={postgresDbUser}
      - POSTGRES_PASSWORD={postgresDbPassword}
      - POSTGRES_DB={dockerConfig.DbName}
    volumes:
      - {dockerConfig.VolumeName}:/var/lib/postgresql/data";
            
            var connectionString = $"Host={dockerConfig.DbHostName};Port=5432;Database={dockerConfig.DbName};Username={postgresDbUser};Password={postgresDbPassword}";
            
            if (dockerConfig.DbProviderEnum == DbProvider.SqlServer)
            {
                var dbUser = "SA";
                var dbPassword = dockerConfig.DbPassword ?? "#localDockerPassword#";
                connectionString = @$"Data Source={dockerConfig.DbHostName},1433;Integrated Security=False;User ID={dbUser};Password={dbPassword}";
                dbService = @$"
    image: mcr.microsoft.com/mssql/server
    restart: always
    ports:
      - '{dockerConfig.DbPort}:1433'
    environment:
      - DB_USER={dbUser}
      - SA_PASSWORD={dbPassword}
      - DB_CONTAINER_NAME={dockerConfig.DbName}
      - ACCEPT_EULA=Y
    volumes:
      - {dockerConfig.VolumeName}:/var/lib/sqlserver/data";
            }
            
            volumes += $"{Environment.NewLine}  {dockerConfig.VolumeName}:";
            
            services += $@"
  {dockerConfig.DbHostName}:{dbService}

  {dockerConfig.ApiServiceName}:
    build:
      context: ""./{dockerConfig.ProjectName}/src/{dockerConfig.ProjectName}""
      dockerfile: ""Dockerfile""
    ports:
      - ""{dockerConfig.ApiPort}:8080""
    environment:
      ASPNETCORE_ENVIRONMENT: ""DockerLocal""
      DB_CONNECTION_STRING: ""{connectionString}""
      #WELL_KNOWN_ENDPOINT: ""https://host.docker.internal:{dockerConfig.AuthServerPort}/.well-known/jwks.json""
      ASPNETCORE_Kestrel__Certificates__Default__Path: ""/https/aspnetappcert.pfx""
      ASPNETCORE_Kestrel__Certificates__Default__Password: ""password""
    volumes:
      - ~/.aspnet/https:/https:ro";
        }

        return @$"version: '3.7'

services:{services}
        
volumes:{volumes}
";
    }

    private static string GetDockerIgnoreText()
    {
        return @$"**/.dockerignore
**/.env
**/.git
**/.gitignore
**/.project
**/.settings
**/.toolstarget
**/.vs
**/.vscode
**/.idea
**/*.*proj.user
**/*.dbmdl
**/*.jfm
**/azds.yaml
**/bin
**/charts
**/docker-compose*
**/Dockerfile*
**/node_modules
**/npm-debug.log
**/obj
**/secrets.dev.yaml
**/values.dev.yaml
LICENSE
README.md";
    }
}