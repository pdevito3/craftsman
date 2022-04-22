namespace NewCraftsman.Builders.Bff;

using System.IO.Abstractions;
using Domain;
using Helpers;
using Services;

public class BffProgramBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public BffProgramBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateProgram(string projectDirectory, string solutionDirectory, string projectName, BffTemplate template)
    {
        var classPath = ClassPathHelper.BffProjectRootClassPath(projectDirectory, $"Program.cs");
        var fileText = GetProgramText(template, solutionDirectory, projectName);
        _utilities.CreateFile(classPath, fileText);
    }
    
    public static string GetProgramText(BffTemplate template, string solutionDirectory, string projectName)
    {
        var loggerClassPath = ClassPathHelper.BffHostExtensionsClassPath(solutionDirectory, "LoggingConfiguration.cs", projectName);
        var boundaryScopes = "";
        foreach(var scope in template.BoundaryScopes)
            boundaryScopes +=
                $@"{Environment.NewLine}        options.Scope.Add(""{scope}"");";
        
        var remoteEndpoints = "";
        foreach(var endpoint in template.RemoteEndpoints)
            remoteEndpoints +=
                $@"{Environment.NewLine}    endpoints.MapRemoteBffApiEndpoint(""{endpoint.LocalPath}"", ""{endpoint.ApiAddress}"")
        .RequireAccessToken();";
        
        
        return @$"using {loggerClassPath.ClassNamespace};
using Duende.Bff;
using Duende.Bff.Yarp;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
builder.Host.AddLoggingConfiguration(builder.Environment);

builder.Services.AddControllers();
builder.Services.AddBff()
    .AddRemoteApis();

builder.Services.AddAuthentication(options =>
    {{
        options.DefaultScheme = ""cookie"";
        options.DefaultChallengeScheme = ""oidc"";
        options.DefaultSignOutScheme = ""oidc"";
    }})
    .AddCookie(""cookie"", options =>
    {{
        options.Cookie.Name = ""{template.CookieName}"";
        options.Cookie.SameSite = SameSiteMode.Strict;
    }})
    .AddOpenIdConnect(""oidc"", options =>
    {{
        options.Authority = Environment.GetEnvironmentVariable(""AUTH_AUTHORITY"");
        options.ClientId = Environment.GetEnvironmentVariable(""AUTH_CLIENT_ID"");
        options.ClientSecret = Environment.GetEnvironmentVariable(""AUTH_CLIENT_SECRET"");
        options.ResponseType = ""code"";
        options.ResponseMode = ""query"";

        options.GetClaimsFromUserInfoEndpoint = true;
        options.MapInboundClaims = false;
        options.SaveTokens = true;

        options.Scope.Clear();
        options.Scope.Add(""openid"");
        options.Scope.Add(""profile"");
        options.Scope.Add(""offline_access"");
        
        // boundary scopes{boundaryScopes}

        options.TokenValidationParameters = new()
        {{
            NameClaimType = ""name"",
            RoleClaimType = ""role""
        }};
    }});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}}

// app.UseHttpsRedirection();
app.UseStaticFiles();

// adds route matching to the middleware pipeline. This middleware looks at the set of endpoints defined in the app, and selects the best match based on the request.
app.UseRouting();

app.UseAuthentication();
app.UseBff();
app.UseAuthorization();

// adds endpoint execution to the middleware pipeline. It runs the delegate associated with the selected endpoint.
app.MapBffManagementEndpoints();

app.MapControllers()
    .RequireAuthorization()
    .AsBffApiEndpoint();

app.UseEndpoints(endpoints =>
{{{remoteEndpoints}
}});

app.MapFallbackToFile(""index.html"");

try
{{
    Log.Information(""Starting application"");
    await app.RunAsync();
}}
catch (Exception e)
{{
    Log.Error(e, ""The application failed to start correctly"");
    throw;
}}
finally
{{
    Log.Information(""Shutting down application"");
    Log.CloseAndFlush();
}}";
    }
}
