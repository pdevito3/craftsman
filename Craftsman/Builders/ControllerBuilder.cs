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

    public static class ControllerBuilder
    {
        public static void CreateController(string solutionDirectory, Entity entity)
        {
            try
            {
                var classPath = ClassPathHelper.ControllerClassPath(solutionDirectory, $"{Utilities.GetControllerName(entity.Plural)}.cs");

                if (!Directory.Exists(classPath.ClassDirectory))
                    Directory.CreateDirectory(classPath.ClassDirectory);

                if (File.Exists(classPath.FullClassPath))
                    throw new FileAlreadyExistsException(classPath.FullClassPath);

                using (FileStream fs = File.Create(classPath.FullClassPath))
                {
                    var data = GetControllerFileText(classPath.ClassNamespace, entity);
                    fs.Write(Encoding.UTF8.GetBytes(data));
                }

                GlobalSingleton.AddCreatedFile(classPath.FullClassPath.Replace($"{solutionDirectory}{Path.DirectorySeparatorChar}", ""));
            }
            catch (FileAlreadyExistsException e)
            {
                WriteError(e.Message);
                throw;
            }
            catch (Exception e)
            {
                WriteError($"An unhandled exception occurred when running the API command.\nThe error details are: \n{e.Message}");
                throw;
            }
        }

        public static string GetControllerFileText(string classNamespace, Entity entity)
        {
            var lowercaseEntityVariable = entity.Name.LowercaseFirstLetter();
            var entityName = entity.Name;
            var entityNamePlural = entity.Plural;
            var readDto = Utilities.GetDtoName(entityName, Dto.Read);
            var readParamDto = Utilities.GetDtoName(entityName, Dto.ReadParamaters);
            var creationDto = Utilities.GetDtoName(entityName, Dto.Creation);
            var updateDto = Utilities.GetDtoName(entityName, Dto.Update);
            var primaryKeyProp = entity.PrimaryKeyProperty;
            var auditable = entity.Auditable ? @$"{Environment.NewLine}    [Authorize]" : "";
            var getListMethodName = Utilities.GetRepositoryListMethodName(entity.Plural);
            var pkPropertyType = primaryKeyProp.Type;

            return @$"namespace {classNamespace}
{{
    using System;
    using System.Collections.Generic;
    using System.Text.Json;
    using AutoMapper;
    using FluentValidation.AspNetCore;
    using Application.Dtos.{entityName};
    using Application.Enums;
    using Application.Interfaces.{entityName};
    using Application.Validation.{entityName};
    using Domain.Entities;
    using Microsoft.AspNetCore.JsonPatch;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Authorization;
    using System.Threading.Tasks;

    [ApiController]
    [Route(""api/{entityNamePlural}"")]
    [ApiVersion(""1.0"")]{auditable}
    public class {entityNamePlural}Controller: Controller
    {{
        private readonly {Utilities.GetRepositoryName(entity.Name, true)} _{lowercaseEntityVariable}Repository;
        private readonly IMapper _mapper;

        public {entityNamePlural}Controller({Utilities.GetRepositoryName(entity.Name, true)} {lowercaseEntityVariable}Repository
            , IMapper mapper)
        {{
            _{lowercaseEntityVariable}Repository = {lowercaseEntityVariable}Repository ??
                throw new ArgumentNullException(nameof({lowercaseEntityVariable}Repository));
            _mapper = mapper ??
                throw new ArgumentNullException(nameof(mapper));
        }}


        [HttpGet(Name = ""Get{entityNamePlural}"")]
        public async Task<ActionResult<IEnumerable<{readDto}>>> Get{entityNamePlural}([FromQuery] {readParamDto} {lowercaseEntityVariable}ParametersDto)
        {{
            var {lowercaseEntityVariable}sFromRepo = await _{lowercaseEntityVariable}Repository.{getListMethodName}({lowercaseEntityVariable}ParametersDto);

            var paginationMetadata = new
            {{
                totalCount = {lowercaseEntityVariable}sFromRepo.TotalCount,
                pageSize = {lowercaseEntityVariable}sFromRepo.PageSize,
                currentPageSize = {lowercaseEntityVariable}sFromRepo.CurrentPageSize,
                currentStartIndex = {lowercaseEntityVariable}sFromRepo.CurrentStartIndex,
                currentEndIndex = {lowercaseEntityVariable}sFromRepo.CurrentEndIndex,
                pageNumber = {lowercaseEntityVariable}sFromRepo.PageNumber,
                totalPages = {lowercaseEntityVariable}sFromRepo.TotalPages,
                hasPrevious = {lowercaseEntityVariable}sFromRepo.HasPrevious,
                hasNext = {lowercaseEntityVariable}sFromRepo.HasNext
            }};

            Response.Headers.Add(""X-Pagination"",
                JsonSerializer.Serialize(paginationMetadata));

            var {lowercaseEntityVariable}sDto = _mapper.Map<IEnumerable<{entityName}Dto>>({lowercaseEntityVariable}sFromRepo);
            return Ok({lowercaseEntityVariable}sDto);
        }}


        [HttpGet(""{{{lowercaseEntityVariable}Id}}"", Name = ""Get{entityName}"")]
        public async Task<ActionResult<{readDto}>> Get{entityName}({pkPropertyType} {lowercaseEntityVariable}Id)
        {{
            var {lowercaseEntityVariable}FromRepo = await _{lowercaseEntityVariable}Repository.Get{entityName}Async({lowercaseEntityVariable}Id);

            if ({lowercaseEntityVariable}FromRepo == null)
            {{
                return NotFound();
            }}

            var {lowercaseEntityVariable}Dto = _mapper.Map<{readDto}>({lowercaseEntityVariable}FromRepo);

            return Ok({lowercaseEntityVariable}Dto);
        }}

        [HttpPost]
        public async Task<ActionResult<{readDto}>> Add{entityName}({creationDto} {lowercaseEntityVariable}ForCreation)
        {{
            var validationResults = new {entityName}ForCreationDtoValidator().Validate({lowercaseEntityVariable}ForCreation);
            validationResults.AddToModelState(ModelState, null);

            if (!ModelState.IsValid)
            {{
                return BadRequest(new ValidationProblemDetails(ModelState));
                //return ValidationProblem();
            }}

            var {lowercaseEntityVariable} = _mapper.Map<{entityName}>({lowercaseEntityVariable}ForCreation);
            await _{lowercaseEntityVariable}Repository.Add{entityName}({lowercaseEntityVariable});
            var saveSuccessful = await _{lowercaseEntityVariable}Repository.SaveAsync();

            if(saveSuccessful)
            {{
                var {lowercaseEntityVariable}Dto = await _{lowercaseEntityVariable}Repository.Get{entityName}Async({lowercaseEntityVariable}.{entity.PrimaryKeyProperty.Name}); //get from repo for fk object, if needed
                return CreatedAtRoute(""Get{entityName}"",
                    new {{ {lowercaseEntityVariable}Dto.{primaryKeyProp.Name} }},
                    {lowercaseEntityVariable}Dto);
            }}

            return StatusCode(500);
        }}

        [HttpDelete(""{{{lowercaseEntityVariable}Id}}"")]
        public async Task<ActionResult> Delete{entityName}({pkPropertyType} {lowercaseEntityVariable}Id)
        {{
            var {lowercaseEntityVariable}FromRepo = await _{lowercaseEntityVariable}Repository.Get{entityName}Async({lowercaseEntityVariable}Id);

            if ({lowercaseEntityVariable}FromRepo == null)
            {{
                return NotFound();
            }}

            _{lowercaseEntityVariable}Repository.Delete{entityName}({lowercaseEntityVariable}FromRepo);
            await _{lowercaseEntityVariable}Repository.SaveAsync();

            return NoContent();
        }}

        [HttpPut(""{{{lowercaseEntityVariable}Id}}"")]
        public async Task<IActionResult> Update{entityName}({pkPropertyType} {lowercaseEntityVariable}Id, {updateDto} {lowercaseEntityVariable})
        {{
            var {lowercaseEntityVariable}FromRepo = await _{lowercaseEntityVariable}Repository.Get{entityName}Async({lowercaseEntityVariable}Id);

            if ({lowercaseEntityVariable}FromRepo == null)
            {{
                return NotFound();
            }}

            var validationResults = new {entityName}ForUpdateDtoValidator().Validate({lowercaseEntityVariable});
            validationResults.AddToModelState(ModelState, null);

            if (!ModelState.IsValid)
            {{
                return BadRequest(new ValidationProblemDetails(ModelState));
                //return ValidationProblem();
            }}

            _mapper.Map({lowercaseEntityVariable}, {lowercaseEntityVariable}FromRepo);
            _{lowercaseEntityVariable}Repository.Update{entityName}({lowercaseEntityVariable}FromRepo);

            await _{lowercaseEntityVariable}Repository.SaveAsync();

            return NoContent();
        }}

        [HttpPatch(""{{{lowercaseEntityVariable}Id}}"")]
        public async Task<IActionResult> PartiallyUpdate{entityName}({pkPropertyType} {lowercaseEntityVariable}Id, JsonPatchDocument<{updateDto}> patchDoc)
        {{
            if (patchDoc == null)
            {{
                return BadRequest();
            }}

            var existing{entityName} = await _{lowercaseEntityVariable}Repository.Get{entityName}Async({lowercaseEntityVariable}Id);

            if (existing{entityName} == null)
            {{
                return NotFound();
            }}

            var {lowercaseEntityVariable}ToPatch = _mapper.Map<{updateDto}>(existing{entityName}); // map the {lowercaseEntityVariable} we got from the database to an updatable {lowercaseEntityVariable} model
            patchDoc.ApplyTo({lowercaseEntityVariable}ToPatch, ModelState); // apply patchdoc updates to the updatable {lowercaseEntityVariable}

            if (!TryValidateModel({lowercaseEntityVariable}ToPatch))
            {{
                return ValidationProblem(ModelState);
            }}

            _mapper.Map({lowercaseEntityVariable}ToPatch, existing{entityName}); // apply updates from the updatable {lowercaseEntityVariable} to the db entity so we can apply the updates to the database
            _{lowercaseEntityVariable}Repository.Update{entityName}(existing{entityName}); // apply business updates to data if needed

            await _{lowercaseEntityVariable}Repository.SaveAsync(); // save changes in the database

            return NoContent();
        }}
    }}
}}";
        }
    }
}
