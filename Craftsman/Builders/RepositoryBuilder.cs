namespace Craftsman.Builders
{
    using Craftsman.Builders.Dtos;
    using Craftsman.Enums;
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System;
    using System.IO;
    using System.Text;
    using static Helpers.ConsoleWriter;

    public static class RepositoryBuilder
    {
        public static void AddRepository(string solutionDirectory, Entity entity, TemplateDbContext dbContext)
        {
            try
            {
                CreateIRepositoryClass(solutionDirectory, entity);
                CreateRepositoryClass(solutionDirectory, entity, dbContext);
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
            var classPath = ClassPathHelper.IRepositoryClassPath(solutionDirectory, $"{Utilities.GetRepositoryName(entity.Name, true)}.cs", entity.Name);

            if (!Directory.Exists(classPath.ClassDirectory))
                Directory.CreateDirectory(classPath.ClassDirectory);

            if (File.Exists(classPath.FullClassPath))
                throw new FileAlreadyExistsException(classPath.FullClassPath);

            using (FileStream fs = File.Create(classPath.FullClassPath))
            {
                var data = GetIRepositoryFileText(classPath.ClassNamespace, entity);
                fs.Write(Encoding.UTF8.GetBytes(data));
            }

            GlobalSingleton.AddCreatedFile(classPath.FullClassPath.Replace($"{solutionDirectory}\\", ""));
        }

        public static string GetIRepositoryFileText(string classNamespace, Entity entity)
        {
            return @$"namespace {classNamespace}
{{
    using Application.Dtos.{entity.Name};
    using Application.Wrappers;
    using System.Threading.Tasks;
    using Domain.Entities;

    public interface {Utilities.GetRepositoryName(entity.Name, true)}
    {{
        PagedList <{entity.Name}> Get{entity.Plural}({Utilities.DtoNameGenerator(entity.Name, Dto.ReadParamaters)} {entity.Name}Parameters);
        Task<{entity.Name}> Get{entity.Name}Async(int {entity.Name}Id);
        {entity.Name} Get{entity.Name}(int {entity.Name}Id);
        void Add{entity.Name}({entity.Name} {entity.Name.LowercaseFirstLetter()});
        void Delete{entity.Name}({entity.Name} {entity.Name.LowercaseFirstLetter()});
        void Update{entity.Name}({entity.Name} {entity.Name.LowercaseFirstLetter()});
        bool Save();
    }}
}}";
        }

        private static void CreateRepositoryClass(string solutionDirectory, Entity entity, TemplateDbContext dbContext)
        {
            var classPath = ClassPathHelper.RepositoryClassPath(solutionDirectory, $"{Utilities.GetRepositoryName(entity.Name, false)}.cs");

            if (!Directory.Exists(classPath.ClassDirectory))
                Directory.CreateDirectory(classPath.ClassDirectory);

            if (File.Exists(classPath.FullClassPath))
                throw new FileAlreadyExistsException(classPath.FullClassPath);

            using (FileStream fs = File.Create(classPath.FullClassPath))
            {
                var data = GetRepositoryFileText(classPath.ClassNamespace, entity, dbContext);
                fs.Write(Encoding.UTF8.GetBytes(data));
            }

            GlobalSingleton.AddCreatedFile(classPath.FullClassPath.Replace($"{solutionDirectory}\\", ""));
        }

        public static string GetRepositoryFileText(string classNamespace, Entity entity, TemplateDbContext dbContext)
        {
            var paramBase = entity.Name.LowercaseFirstLetter();
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

    public class {Utilities.GetRepositoryName(entity.Name, false)} : {Utilities.GetRepositoryName(entity.Name, true)}
    {{
        private {dbContext.ContextName} _context;
        private readonly SieveProcessor _sieveProcessor;

        public {entity.Name}Repository({dbContext.ContextName} context,
            SieveProcessor sieveProcessor)
        {{
            _context = context
                ?? throw new ArgumentNullException(nameof(context));
            _sieveProcessor = sieveProcessor ??
                throw new ArgumentNullException(nameof(sieveProcessor));
        }}

        public PagedList<{entity.Name}> Get{entity.Plural}({entity.Name}ParametersDto {paramBase}Parameters)
        {{
            if ({paramBase}Parameters == null)
            {{
                throw new ArgumentNullException(nameof({paramBase}Parameters));
            }}

            var collection = _context.{entity.Plural} as IQueryable<{entity.Name}>; // TODO: AsNoTracking() should increase performance, but will break the sort tests. need to investigate

            var sieveModel = new SieveModel
            {{
                Sorts = {paramBase}Parameters.SortOrder,
                Filters = {paramBase}Parameters.Filters
            }};

            collection = _sieveProcessor.Apply(sieveModel, collection);

            return PagedList<{entity.Name}>.Create(collection,
                {paramBase}Parameters.PageNumber,
                {paramBase}Parameters.PageSize);
        }}

        public async Task<{entity.Name}> Get{entity.Name}Async(int {paramBase}Id)
        {{
            return await _context.{entity.Plural}.FirstOrDefaultAsync({entity.Lambda} => {entity.Lambda}.{entity.PrimaryKeyProperties[0].Name} == {paramBase}Id);
        }}

        public {entity.Name} Get{entity.Name}(int {paramBase}Id)
        {{
            return _context.{entity.Plural}.FirstOrDefault({entity.Lambda} => {entity.Lambda}.{entity.PrimaryKeyProperties[0].Name} == {paramBase}Id);
        }}

        public void Add{entity.Name}({entity.Name} {paramBase})
        {{
            if ({paramBase} == null)
            {{
                throw new ArgumentNullException(nameof({entity.Name}));
            }}

            _context.{entity.Plural}.Add({paramBase});
        }}

        public void Delete{entity.Name}({entity.Name} {paramBase})
        {{
            if ({paramBase} == null)
            {{
                throw new ArgumentNullException(nameof({entity.Name}));
            }}

            _context.{entity.Plural}.Remove({paramBase});
        }}

        public void Update{entity.Name}({entity.Name} {paramBase})
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
                            newText += @$"{Environment.NewLine}            services.AddScoped<{Utilities.GetRepositoryName(entity.Name, true)}, {Utilities.GetRepositoryName(entity.Name, false)}>();";
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

            GlobalSingleton.AddUpdatedFile(pathString.Replace($"{solutionDirectory}\\", ""));
            //WriteWarning($"TODO Need a message for the update of Service Registration.");
        }
    }
}
