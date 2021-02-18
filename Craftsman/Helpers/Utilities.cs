namespace Craftsman.Helpers
{
    using Craftsman.Enums;
    using Craftsman.Exceptions;
    using Craftsman.Models;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class Utilities
    {
        public static string PropTypeCleanup(string prop)
        {
            var lowercaseProps = new string[] { "string", "int", "decimal", "double", "float", "object", "bool", "byte", "char", "byte", "ushort", "uint", "ulong" };
            if (lowercaseProps.Contains(prop.ToLower()))
                return prop.ToLower();
            else if (prop.ToLower() == "datetime")
                return "DateTime";
            else if (prop.ToLower() == "datetime?")
                return "DateTime?";
            else if (prop.ToLower() == "datetimeoffset")
                return "DateTimeOffset";
            else if (prop.ToLower() == "datetimeoffset?")
                return "DateTimeOffset?";
            else if (prop.ToLower() == "guid")
                return "Guid";
            else
                return prop;
        }

        public static string GetRepositoryName(string entityName, bool isInterface)
        {
            return isInterface ? $"I{entityName}Repository" : $"{entityName}Repository";
        }

        public static string GetControllerName(string entityName)
        {
            return $"{entityName}Controller";
        }

        public static string GetSeederName(Entity entity)
        {
            return $"{entity.Name}Seeder";
        }

        public static string GetRepositoryListMethodName(string pluralEntity)
        {
            return $"Get{pluralEntity}Async";
        }

        public static string GetAppSettingsName(string envName, bool asJson = true)
        {
            if(envName == "Startup")
                return asJson ? $"appsettings.json" : $"appsettings";

            return asJson ? $"appsettings.{envName}.json" : $"appsettings.{envName}";
        }

        public static string GetStartupName(string envName)
        {
            return envName == "Startup" ? "Startup" : $"Startup{envName}";
        }

        public static string GetProfileName(string entityName)
        {
            return $"{entityName}Profile";
        }

        public static string GetDtoName(string entityName, Dto dto)
        {
            switch (dto)
            {
                case Dto.Manipulation:
                    return $"{entityName}ForManipulationDto";
                case Dto.Creation:
                    return $"{entityName}ForCreationDto";
                case Dto.Update:
                    return $"{entityName}ForUpdateDto";
                case Dto.Read:
                    return $"{entityName}Dto";
                case Dto.ReadParamaters:
                    return $"{entityName}ParametersDto";
                default:
                    throw new Exception($"Name generator not configured for {Enum.GetName(typeof(Dto), dto)}");
            }
        }

        public static string ValidatorNameGenerator(string entityName, Validator validator)
        {
            switch (validator)
            {
                case Validator.Manipulation:
                    return $"{entityName}ForManipulationDtoValidator";
                case Validator.Creation:
                    return $"{entityName}ForCreationDtoValidator";
                case Validator.Update:
                    return $"{entityName}ForUpdateDtoValidator";
                default:
                    throw new Exception($"Name generator not configured for {Enum.GetName(typeof(Validator), validator)}");
            }
        }

        public static void ExecuteProcess(string command, string args, string directory)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = command,
                    Arguments = args,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = false,
                    WorkingDirectory = directory
                }
            };

            process.Start();
            process.WaitForExit();
        }

        public static string EndpointBaseGenerator(string entityNamePlural)
        {
            return $@"api/{entityNamePlural}";
        }

        public static string PolicyStringBuilder(Policy policy)
        {
            if (policy.PolicyType == Enum.GetName(typeof(PolicyType), PolicyType.Scope))
            {
                // ex: options.AddPolicy("CanRead", policy => policy.RequireClaim("scope", "detailedrecipes.read"));
                return $@"                options.AddPolicy(""{policy.Name}"", 
                    policy => policy.RequireClaim(""scope"", ""{policy.PolicyValue}""));";
            }
            //else if (policy.PolicyType == Enum.GetName(typeof(PolicyType), PolicyType.Role))
            //{
            //    // ex: options.AddPolicy("RequireAdministratorRole", policy => policy.RequireRole("Administrator"));
            //    return $@"                options.AddPolicy(""{policy.Name}"", 
            //        policy => policy.RequireRole(""{policy.PolicyValue}""));";
            //}

            // claim ex: options.AddPolicy("EmployeeOnly", policy => policy.RequireClaim("EmployeeNumber"));
            return $@"                options.AddPolicy(""{policy.Name}"", 
                    policy => policy.RequireClaim(""{policy.PolicyValue}""));";
        }

        public static string BuildTestAuthorizationString(List<Policy> policies, List<Endpoint> endpoints, string entityName, PolicyType policyType)
        {
            var endpointStrings = new List<string>();
            foreach(var endpoint in endpoints)
            {
                endpointStrings.Add(Enum.GetName(typeof(Endpoint), endpoint));
            }

            var results = policies
                .Where(p => p.EndpointEntities.Any(ee => ee.EntityName == entityName)
                    && p.EndpointEntities.Any(ee => ee.RestrictedEndpoints.Intersect(endpointStrings).Any()));
            
            return "{\"" + string.Join("\", \"", results.Select(r => r.PolicyValue)) + "\"}";
        }
    }
}
