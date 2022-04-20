namespace NewCraftsman.Builders.Bff
{
    using System.IO.Abstractions;
    using Helpers;
    using Services;

    public class BffReadmeBuilder
    {
        private readonly ICraftsmanUtilities _utilities;

        public BffReadmeBuilder(ICraftsmanUtilities utilities)
        {
            _utilities = utilities;
        }

        public void CreateReadme(string projectDirectory, string projectName)
        {
            var classPath = ClassPathHelper.BffProjectRootClassPath(projectDirectory, $"README.md");
            var fileText = GetReadmeFileText(projectName);
            _utilities.CreateFile(classPath, fileText);
        }

        public static string GetReadmeFileText(string projectName)
        {
            return @$"# {projectName}

This project was created with [Craftsman](https://github.com/pdevito3/craftsman).

## Getting Started
1. Start your Auth Server and associated APIs
2. Run your dotnet project with `dotnet run` or your IDE of choice.

## Project Structure
This project structure is comparable to a [bulletproof react](https://github.com/alan2207/bulletproof-react) project.

## Configuration
This project uses .NET as a backend for frontend (BFF) and leverages [Duende BFF](https://github.com/DuendeSoftware/BFF) for the security layer.

### Calling Apis
Since we are using a BFF, we want to proxy our api calls through the BFF. There are two parts of our project that need to be configured 
to have this work:

1. In `vite.config.ts` you have have a config definition. Part of that config definition is the `proxy` property under the `server` section.
In order to proxy api calls to to your .NET project, you you need to add the url base that you want to proxy to the `proxy` section. 
For example, `'/api': baseProxy,` in the below will proxy all network calls starting with `/api` to your .NET project.

```ts
// condensed for README

const baseProxy = {{
    target,
    secure: false,
}};

export default defineConfig({{
    server: {{
        // these are the proxy routes that will be forwarded to your **BFF**
        proxy: {{
            '/bff': baseProxy,
            '/signin-oidc': baseProxy,
            '/signout-callback-oidc': baseProxy,
            '/api': baseProxy,
        }},
    }},
}});
```

> ⭐️ This should already be configured for you given the Craftsman configuration and work for all of your scaffolded boundaries, 
but if you want to add more routes, you can do so.

2. Now that you are sending the routes to your BFF, they still need to be routed *out* of your BFF. To do that, you need to set up 
[Remote Endpoints](https://docs.duendesoftware.com/identityserver/v5/bff/apis/remote/) in your BFF. For example, the 
below would route an incoming call to anything starting with `/api/recipes` to `https://localhost:5375/api/recipes`.

```csharp
app.UseEndpoints(endpoints =>
{{
    endpoints.MapRemoteBffApiEndpoint(""/api/recipes"", ""https://localhost:5375/api/recipes"")
        .RequireAccessToken();
    endpoints.MapRemoteBffApiEndpoint(""/api/ingredients"", ""https://localhost:5375/api/ingredients"")
        .RequireAccessToken();
}});
```

```ts

";
        }
    }
}