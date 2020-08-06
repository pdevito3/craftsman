namespace Craftsman.Helpers
{
    using Craftsman.Enums;
    using Craftsman.Models;
    using System;
    using System.Collections.Generic;
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
            else
                return prop;
        }

        public static string GetRepositoryName(Entity entity, bool isInterface)
        {
            return isInterface ? $"I{entity.Name}Repository" : $"{entity.Name}Repository";
        }

        public static string GetSeederName(Entity entity)
        {
            return $"{entity.Name}Seeder";
        }

        public static string GetProfileName(Entity entity)
        {
            return $"{entity.Name}Profile";
        }

        public static string DtoNameGenerator(string entityName, Dto dto)
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
                case Dto.PaginationParamaters:
                    return $"{entityName}PaginationParameters";
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
    }
}
