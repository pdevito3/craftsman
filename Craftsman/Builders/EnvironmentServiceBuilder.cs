namespace Craftsman.Builders;

using Domain;
using Helpers;
using MediatR;
using Services;

public static class EnvironmentServiceBuilder
{
    public class Command : IRequest<bool>
    {
    }

    public class Handler : IRequestHandler<Command, bool>
    {
        private readonly ICraftsmanUtilities _utilities;
        private readonly IScaffoldingDirectoryStore _scaffoldingDirectoryStore;

        public Handler(ICraftsmanUtilities utilities,
            IScaffoldingDirectoryStore scaffoldingDirectoryStore)
        {
            _utilities = utilities;
            _scaffoldingDirectoryStore = scaffoldingDirectoryStore;
        }

        public Task<bool> Handle(Command request, CancellationToken cancellationToken)
        {
            var classPath = ClassPathHelper.WebApiServicesClassPath(_scaffoldingDirectoryStore.SrcDirectory, 
                $"{FileNames.GetEnvironmentServiceFileName()}.cs",
                _scaffoldingDirectoryStore.ProjectBaseName);
            var fileText = GetFileText(classPath.ClassNamespace, _scaffoldingDirectoryStore.ProjectBaseName);
            _utilities.CreateFile(classPath, fileText);
            return Task.FromResult(true);
        }
        private string GetFileText(string classNamespace, string projectBaseName)
        {
            var serviceName = FileNames.GetEnvironmentServiceFileName();
            var interfaceName = FileNames.GetEnvironmentServiceInterfaceFileName();
            return @$"namespace {classNamespace};

public interface {interfaceName} : {FileNames.BoundaryServiceInterface(projectBaseName)}
{{
    public string GetEnv();
    public string GetAudience();
    public string GetAuthority();
    public string GetAuthUrl();
    public string GetTokenUrl();
    public string GetClientId();
    public string GetClientSecret();
    public string GetDbConnectionString();
    public string GetRmqHost();
    public string GetRmqVirtualHost();
    public string GetRmqUsername();
    public string GetRmqPassword();
    public string GetRmqPort();
    public string GetJaegerHost();
}}

public sealed class {serviceName} : {interfaceName}
{{
    public const string EnvKey = ""ASPNETCORE_ENVIRONMENT"";
    public const string AudienceKey = ""AUTH_AUDIENCE"";
    public const string AuthorityKey = ""AUTH_AUTHORITY"";
    public const string AuthUrlKey = ""AUTH_AUTHORIZATION_URL"";
    public const string TokenUrlKey = ""AUTH_TOKEN_URL"";
    public const string ClientIdKey = ""AUTH_CLIENT_ID"";
    public const string ClientSecretKey = ""AUTH_CLIENT_SECRET"";
    public const string DbConnectionStringKey = ""DB_CONNECTION_STRING"";
    public const string RmqHostKey = ""RMQ_HOST"";
    public const string RmqVirtualHostKey = ""RMQ_VIRTUAL_HOST"";
    public const string RmqUsernameKey = ""RMQ_USERNAME"";
    public const string RmqPasswordKey = ""RMQ_PASSWORD"";
    public const string RmqPortKey = ""RMQ_PORT"";
    public const string JaegerHostKey = ""JAEGER_HOST"";

    public static string Env => GetEnvVar(EnvKey, false);
    public static string Audience => GetEnvVar(AudienceKey, false);
    public static string Authority => GetEnvVar(AuthorityKey, false);
    public static string AuthUrl => GetEnvVar(AuthUrlKey, false);
    public static string TokenUrl => GetEnvVar(TokenUrlKey, false);
    public static string ClientId => GetEnvVar(ClientIdKey, false);
    public static string ClientSecret => GetEnvVar(ClientSecretKey, false);
    public static string DbConnectionString => GetEnvVar(DbConnectionStringKey, false);
    public static string RmqHost => GetEnvVar(RmqHostKey, false);
    public static string RmqVirtualHost => GetEnvVar(RmqVirtualHostKey, false);
    public static string RmqUsername => GetEnvVar(RmqUsernameKey, false);
    public static string RmqPassword => GetEnvVar(RmqPasswordKey, false);
    public static string RmqPort => GetEnvVar(RmqPortKey, false);
    public static string JaegerHost => GetEnvVar(JaegerHostKey, false);

    public string GetEnv() => GetEnvVar(EnvKey, false);
    public string GetAudience() => GetEnvVar(AudienceKey, false);
    public string GetAuthority() => GetEnvVar(AuthorityKey, false);
    public string GetAuthUrl() => GetEnvVar(AuthUrlKey, false);
    public string GetTokenUrl() => GetEnvVar(TokenUrlKey, false);
    public string GetClientId() => GetEnvVar(ClientIdKey, false);
    public string GetClientSecret() => GetEnvVar(ClientSecretKey, false);
    public string GetDbConnectionString() => GetEnvVar(DbConnectionStringKey, false);
    public string GetRmqHost() => GetEnvVar(RmqHostKey, false);
    public string GetRmqVirtualHost() => GetEnvVar(RmqVirtualHostKey, false);
    public string GetRmqUsername() => GetEnvVar(RmqUsernameKey, false);
    public string GetRmqPassword() => GetEnvVar(RmqPasswordKey, false);
    public string GetRmqPort() => GetEnvVar(RmqPortKey, false);
    public string GetJaegerHost() => GetEnvVar(JaegerHostKey, false);
    
    private static string GetEnvVar(string envVarName, bool throwIfMissing = true)
    {{
        var envVar = Environment.GetEnvironmentVariable(envVarName);
        if (envVar == null && throwIfMissing)
            throw new Exception($""Invalid environment variable: {{envVarName}}"");
        
        return envVar;
    }}
}}";
        }
    }
    
}
