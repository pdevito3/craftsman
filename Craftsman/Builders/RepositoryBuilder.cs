namespace Craftsman.Builders
{
    using Craftsman.Builders.Dtos;
    using Craftsman.Enums;
    using Craftsman.Exceptions;
    using Craftsman.Helpers;
    using Craftsman.Models;
    using System;
    using System.IO;
    using System.Linq;
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
                    WriteError($"An unhandled exception occurred when running the API command.\nThe error details are: \n{e.Message}");
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

            GlobalSingleton.AddCreatedFile(classPath.FullClassPath.Replace($"{solutionDirectory}{Path.DirectorySeparatorChar}", ""));
        }

        public static string GetIRepositoryFileText(string classNamespace, Entity entity)
        {
            var getListMethodName = Utilities.GetRepositoryListMethodName(entity.Plural);
            var pkPropertyType = entity.PrimaryKeyProperty.Type;

            return @$"namespace {classNamespace}
{{
    using System;
    using Application.Dtos.{entity.Name};
    using Application.Wrappers;
    using System.Threading.Tasks;
    using Domain.Entities;

    public interface {Utilities.GetRepositoryName(entity.Name, true)}
    {{
        Task<PagedList<{entity.Name}>> {getListMethodName}({Utilities.GetDtoName(entity.Name, Dto.ReadParamaters)} {entity.Name}Parameters);
        Task<{entity.Name}> Get{entity.Name}Async({pkPropertyType} {entity.Name}Id);
        {entity.Name} Get{entity.Name}({pkPropertyType} {entity.Name}Id);
        Task Add{entity.Name}({entity.Name} {entity.Name.LowercaseFirstLetter()});
        void Delete{entity.Name}({entity.Name} {entity.Name.LowercaseFirstLetter()});
        void Update{entity.Name}({entity.Name} {entity.Name.LowercaseFirstLetter()});
        bool Save();
        Task<bool> SaveAsync();
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

            GlobalSingleton.AddCreatedFile(classPath.FullClassPath.Replace($"{solutionDirectory}{Path.DirectorySeparatorChar}", ""));
        }

        public static string GetRepositoryFileText(string classNamespace, Entity entity, TemplateDbContext dbContext)
        {
            var getListMethodName = Utilities.GetRepositoryListMethodName(entity.Plural);
            var paramBase = entity.Name.LowercaseFirstLetter();
            var pkPropertyType = entity.PrimaryKeyProperty.Type;
            var pkPropertyName = entity.PrimaryKeyProperty.Name;
            var fkIncludes = "";
            foreach(var fk in entity.Properties.Where(p => p.IsForeignKey))
            {
                fkIncludes += $@"{Environment.NewLine}                .Include({fk.Name.ToLower().Substring(0, 1)} => {fk.Name.ToLower().Substring(0, 1)}.{fk.Name})";
            }

            return @$"namespace {classNamespace}
{{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Application.Dtos.{entity.Name};
    using Application.Interfaces.{entity.Name};
    using Application.Wrappers;
    using Domain.Entities;
    using Infrastructure.Persistence.Contexts;
    using Microsoft.EntityFrameworkCore;
    using Sieve.Models;
    using Sieve.Services;

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

        public async Task<PagedList<{entity.Name}>> {getListMethodName}({entity.Name}ParametersDto {paramBase}Parameters)
        {{
            if ({paramBase}Parameters == null)
            {{
                throw new ArgumentNullException(nameof({paramBase}Parameters));
            }}

            var collection = _context.{entity.Plural}{fkIncludes}
                as IQueryable<{entity.Name}>; // TODO: AsNoTracking() should increase performance, but will break the sort tests. need to investigate

            var sieveModel = new SieveModel
            {{
                Sorts = {paramBase}Parameters.SortOrder ?? ""{pkPropertyName}"",
                Filters = {paramBase}Parameters.Filters
            }};

            collection = _sieveProcessor.Apply(sieveModel, collection);

            return await PagedList<{entity.Name}>.CreateAsync(collection,
                {paramBase}Parameters.PageNumber,
                {paramBase}Parameters.PageSize);
        }}

        public async Task<{entity.Name}> Get{entity.Name}Async({pkPropertyType} {paramBase}Id)
        {{
            // include marker -- requires return _context.{entity.Plural} as it's own line with no extra text -- do not delete this comment
            return await _context.{entity.Plural}{fkIncludes}
                .FirstOrDefaultAsync({entity.Lambda} => {entity.Lambda}.{entity.PrimaryKeyProperty.Name} == {paramBase}Id);
        }}

        public {entity.Name} Get{entity.Name}({pkPropertyType} {paramBase}Id)
        {{
            // include marker -- requires return _context.{entity.Plural} as it's own line with no extra text -- do not delete this comment
            return _context.{entity.Plural}{fkIncludes}
                .FirstOrDefault({entity.Lambda} => {entity.Lambda}.{entity.PrimaryKeyProperty.Name} == {paramBase}Id);
        }}

        public async Task Add{entity.Name}({entity.Name} {paramBase})
        {{
            if ({paramBase} == null)
            {{
                throw new ArgumentNullException(nameof({entity.Name}));
            }}

            await _context.{entity.Plural}.AddAsync({paramBase});
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

        public async Task<bool> SaveAsync()
        {{
            return await _context.SaveChangesAsync() > 0;
        }}
    }}
}}";
        }

        public static void RegisterRepository(string solutionDirectory, Entity entity)
        {
            var classPath = ClassPathHelper.InfraPersistenceServiceProviderClassPath(solutionDirectory, "ServiceRegistration.cs");

            if (!Directory.Exists(classPath.ClassDirectory))
                throw new DirectoryNotFoundException($"The `{classPath.ClassDirectory}` directory could not be found.");

            if (!File.Exists(classPath.FullClassPath))
                throw new FileNotFoundException($"The `{classPath.FullClassPath}` file could not be found.");

            var tempPath = $"{classPath.FullClassPath}temp";
            var interfaceNamespaceAdded = false;
            using (var input = File.OpenText(classPath.FullClassPath))
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

            // delete the old file and set the name of the new one to the original name
            File.Delete(classPath.FullClassPath);
            File.Move(tempPath, classPath.FullClassPath);

            GlobalSingleton.AddUpdatedFile(classPath.FullClassPath.Replace($"{solutionDirectory}{Path.DirectorySeparatorChar}", ""));
        }
    }
}
