namespace Craftsman.Builders
{
    using Craftsman.Builders.Dtos;
    using Craftsman.Enums;
    using Craftsman.Exceptions;
    using Craftsman.Models;
    using System;
    using System.IO;
    using System.Text;
    using static Helpers.ConsoleWriter;

    public static class RepositoryBuilder
    {
        public static void AddRepository(string solutionDirectory, Entity entity)
        {
            try
            {
                CreateIRepositoryClass(solutionDirectory, entity);
                CreateRepositoryClass(solutionDirectory, entity);
                RegisterRepository(solutionDirectory, entity);
            }
            catch (Exception e)
            {
                if (e is DirectoryNotFoundException
                   || e is FileNotFoundException
                   || e is FileAlreadyExistsException)
                {
                    WriteError(e.Message);
                }
                else
                {
                    WriteError($"An unhandled exception occured when running the API command.\nThe error details are: \n{e.Message}");
                }
                 
            }
        }

        private static void CreateIRepositoryClass(string solutionDirectory, Entity entity)
        {
            //TODO move these to a dictionary to lookup and overwrite if I want
            var repoTopPath = $"Application\\Interfaces\\{entity.Name}";
            var repoNamespace = repoTopPath.Replace("\\", ".");

            var entityDir = Path.Combine(solutionDirectory, repoTopPath);
            if (!Directory.Exists(entityDir))
                Directory.CreateDirectory(entityDir);

            var pathString = Path.Combine(entityDir, $"{GetRepositoryName(entity, true)}.cs");
            if (File.Exists(pathString))
                throw new FileAlreadyExistsException(pathString);

            using (FileStream fs = File.Create(pathString))
            {
                var data = GetIRepositoryFileText(repoNamespace, entity);
                fs.Write(Encoding.UTF8.GetBytes(data));
            }
            WriteInfo($"A new '{entity.Name}' repository interface file was added here: {pathString}.");
        }

        public static string GetIRepositoryFileText(string classNamespace, Entity entity)
        {
            return @$"namespace {classNamespace}
{{
    using Application.Dtos.{entity.Name};
    using Application.Wrappers;
    using System.Threading.Tasks;
    using Domain.Entities;

    public interface {GetRepositoryName(entity, true)}
    {{
        PagedList <{entity.Name}> Get{entity.Plural}({DtoBuilder.DtoNameGenerator(entity.Name, Dto.ReadParamaters)} {entity.Name}Parameters);
        Task<{entity.Name}> Get{entity.Name}Async(int {entity.Name}Id);
        {entity.Name} Get{entity.Name}(int {entity.Name}Id);
        void Add{entity.Name}({entity.Name} {entity.Name.LowercaseFirstLetter()});
        void Delete{entity.Name}({entity.Name} {entity.Name.LowercaseFirstLetter()});
        void Update{entity.Name}({entity.Name} {entity.Name.LowercaseFirstLetter()});
        bool Save();
    }}
}}";
        }

        private static void CreateRepositoryClass(string solutionDirectory, Entity entity)
        {
            //TODO move these to a dictionary to lookup and overwrite if I want
            var repoTopPath = "Infrastructure.Persistence\\Repositories";
            var repoNamespace = repoTopPath.Replace("\\", ".");

            var entityDir = Path.Combine(solutionDirectory, repoTopPath);
            if (!Directory.Exists(entityDir))
                Directory.CreateDirectory(entityDir);

            var pathString = Path.Combine(entityDir, $"{GetRepositoryName(entity, false)}.cs");
            if (File.Exists(pathString))
                throw new FileAlreadyExistsException(pathString);

            using (FileStream fs = File.Create(pathString))
            {
                var data = GetRepositoryFileText(repoNamespace, entity);
                fs.Write(Encoding.UTF8.GetBytes(data));
            }
            WriteInfo($"A new '{entity.Name}' repository file was added here: {pathString}.");
        }

        public static string GetRepositoryFileText(string classNamespace, Entity entity)
        {
            return @$"namespace {classNamespace}
{{
    using Application.Dtos.{entity.Name};
    using Application.Interfaces.{entity.Name};
    using Application.Wrappers;
    using Domain.Entities;
    using Infrastructure.Persistence.Contexts;
    using Microsoft.EntityFrameworkCore;
    using Sieve.Models;
    using Sieve.Services;
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    public class {GetRepositoryName(entity, false)} : {GetRepositoryName(entity, true)}
    {{
        private {entity.Name}DbContext _context;
        private readonly SieveProcessor _sieveProcessor;

        public {entity.Name}Repository({entity.Name}DbContext context,
            SieveProcessor sieveProcessor)
        {{
            _context = context
                ?? throw new ArgumentNullException(nameof(context));
            _sieveProcessor = sieveProcessor ??
                throw new ArgumentNullException(nameof(sieveProcessor));
        }}

        public PagedList<{entity.Name}> Get{entity.Plural}({entity.Name}ParametersDto {entity.Name}Parameters)
        {{
            if ({entity.Name}Parameters == null)
            {{
                throw new ArgumentNullException(nameof({entity.Name}Parameters));
            }}

            var collection = _context.{entity.Plural} as IQueryable<{entity.Name}>; // TODO: AsNoTracking() should increase performance, but will break the sort tests. need to investigate

            if (!string.IsNullOrWhiteSpace({entity.Name}Parameters.QueryString))
            {{
                var QueryString = {entity.Name}Parameters.QueryString.Trim();
                collection = collection.Where({entity.Lambda} => {entity.Lambda}.{entity.Name}TextField1.Contains(QueryString)
                    || {entity.Lambda}.{entity.Name}TextField2.Contains(QueryString));
            }}

            var sieveModel = new SieveModel
            {{
                Sorts = {entity.Name}Parameters.SortOrder,
                Filters = {entity.Name}Parameters.Filters
            }};

            collection = _sieveProcessor.Apply(sieveModel, collection);

            return PagedList<{entity.Name}>.Create(collection,
                {entity.Name}Parameters.PageNumber,
                {entity.Name}Parameters.PageSize);
        }}

        public async Task<{entity.Name}> Get{entity.Name}Async(int {entity.Name}Id)
        {{
            return await _context.{entity.Plural}.FirstOrDefaultAsync({entity.Lambda} => {entity.Lambda}.{entity.PrimaryKeyProperties[0].Name} == {entity.Name}Id);
        }}

        public {entity.Name} Get{entity.Name}(int {entity.Name}Id)
        {{
            return _context.{entity.Plural}.FirstOrDefault({entity.Lambda} => {entity.Lambda}.{entity.PrimaryKeyProperties[0].Name} == {entity.Name}Id);
        }}

        public void Add{entity.Name}({entity.Name} {entity.Name.LowercaseFirstLetter()})
        {{
            if ({entity.Name} == null)
            {{
                throw new ArgumentNullException(nameof({entity.Name}));
            }}

            _context.{entity.Plural}.Add({entity.Name});
        }}

        public void Delete{entity.Name}({entity.Name} {entity.Name.LowercaseFirstLetter()})
        {{
            if ({entity.Name} == null)
            {{
                throw new ArgumentNullException(nameof({entity.Name}));
            }}

            _context.{entity.Plural}.Remove({entity.Name});
        }}

        public void Update{entity.Name}({entity.Name} {entity.Name.LowercaseFirstLetter()})
        {{
            // no implementation for now
        }}

        public bool Save()
        {{
            return _context.SaveChanges() > 0;
        }}
    }}
}}";
        }

        public static string GetRepositoryName(Entity entity, bool isInterface)
        {
            return isInterface ? $"I{entity.Name}Repository" : $"{entity.Name}Repository";
        }

        public static void RegisterRepository(string solutionDirectory, Entity entity)
        {
            //TODO move these to a dictionary to lookup and overwrite if I want
            var repoTopPath = "Infrastructure.Persistence";

            var entityDir = Path.Combine(solutionDirectory, repoTopPath);
            if (!Directory.Exists(entityDir))
                throw new DirectoryNotFoundException($"The `{entityDir}` directory could not be found.");

            var pathString = Path.Combine(entityDir, $"ServiceRegistration.cs");
            if (!File.Exists(pathString))
                throw new FileNotFoundException($"The `{pathString}` file could not be found.");

            var tempPath = $"{pathString}temp";
            var interfaceNamespaceAdded = false;
            using (var input = File.OpenText(pathString))
            {
                using (var output = new StreamWriter(tempPath))
                {
                    string line;
                    while (null != (line = input.ReadLine()))
                    {
                        var newText = $"{line}";
                        if(line.Contains("#region Repositories"))
                        {
                            newText += @$"{Environment.NewLine}            services.AddScoped<{GetRepositoryName(entity, true)}, {GetRepositoryName(entity, false)}>();";
                        }
                        else if (line.Contains("using") & !interfaceNamespaceAdded)
                        {
                            newText += @$"{Environment.NewLine}    using Application.Interfaces.{entity.Name};";
                            interfaceNamespaceAdded = true;
                        }

                        output.WriteLine(newText);
                    }
                }
            }

            // delete the old file and set the name of the new one to the original nape
            File.Delete(pathString); 
            File.Move(tempPath, pathString);
        }
    }
}
