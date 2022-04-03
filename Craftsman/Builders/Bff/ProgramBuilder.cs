namespace Craftsman.Builders.Bff;

using System.IO.Abstractions;
using Helpers;
using Models;

public class ProgramBuilder
{
    public static void CreateProgram(string spaDirectory, string projectName, BffTemplate template, IFileSystem fileSystem)
    {
        var classPath = ClassPathHelper.BffProjectRootClassPath(spaDirectory, $"Program.cs", projectName);
        var fileText = GetProgramText(template);
        Utilities.CreateFile(classPath, fileText, fileSystem);
    }
    
    public static string GetProgramText(BffTemplate template)
    {
        var boundaryScopes = "";
        foreach(var scope in template.BoundaryScopes)
            boundaryScopes +=
                $@"{Environment.NewLine}        options.Scope.Add(""{scope}"");";
        
        var remoteEndpoints = "";
        foreach(var endpoint in template.RemoteEndpoints)
            remoteEndpoints +=
                $@"{Environment.NewLine}    endpoints.MapRemoteBffApiEndpoint(""{endpoint.LocalPath}"", ""{endpoint.ApiAddress}"")
        .RequireAccessToken();";
        
        
        return @$"using Duende.Bff;
using Duende.Bff.Yarp;

var builder = WebApplication.CreateBuilder(args);

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

app.Run();
";
    }
}
