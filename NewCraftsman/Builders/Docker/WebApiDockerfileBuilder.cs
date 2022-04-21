namespace NewCraftsman.Builders.Docker;

using Helpers;
using Services;

public class WebApiDockerfileBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public WebApiDockerfileBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateStandardDotNetDockerfile(string srcDirectory, string projectBaseName)
    {
        var classPath = ClassPathHelper.WebApiProjectRootClassPath(srcDirectory, $"Dockerfile", projectBaseName);
        var fileText = GetDockerfileText(projectBaseName);
        _utilities.CreateFile(classPath, fileText);
    }

    private static string GetDockerfileText(string projectBaseName)
    {
        return @$"FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY [""{projectBaseName}/src/{projectBaseName}/{projectBaseName}.csproj"", ""./{projectBaseName}/src/{projectBaseName}/""]
COPY [""SharedKernel/SharedKernel.csproj"", ""./SharedKernel/""]
RUN dotnet restore ""./{projectBaseName}/src/{projectBaseName}/{projectBaseName}.csproj""

# Copy everything else and build
COPY . ./
RUN dotnet build ""{projectBaseName}/src/{projectBaseName}/{projectBaseName}.csproj"" -c Release -o /app/build

FROM build-env AS publish
RUN dotnet publish ""{projectBaseName}/src/{projectBaseName}/{projectBaseName}.csproj"" -c Release -o /app/out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=publish /app/out .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT [""dotnet"", ""/app/{projectBaseName}.dll""]
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
}