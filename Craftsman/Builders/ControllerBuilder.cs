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

                GlobalSingleton.AddCreatedFile(classPath.FullClassPath.Replace($"{solutionDirectory}\\", ""));
            }
            catch (FileAlreadyExistsException e)
            {
                WriteError(e.Message);
                throw;
            }
            catch (Exception e)
            {
                WriteError($"An unhandled exception occured when running the API command.\nThe error details are: \n{e.Message}");
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
            var primaryKeyProp = entity.PrimaryKeyProperties[0];

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

    [ApiController]
    [Route(""api/{entityNamePlural}"")]
    [ApiVersion(""1.0"")]
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
        public ActionResult<IEnumerable<{readDto}>> Get{entityNamePlural}([FromQuery] {readParamDto} {lowercaseEntityVariable}ParametersDto)
        {{
            var {lowercaseEntityVariable}sFromRepo = _{lowercaseEntityVariable}Repository.Get{entityNamePlural}({lowercaseEntityVariable}ParametersDto);
            
            var previousPageLink = {lowercaseEntityVariable}sFromRepo.HasPrevious
                    ? Create{entityNamePlural}ResourceUri({lowercaseEntityVariable}ParametersDto,
                        ResourceUriType.PreviousPage)
                    : null;

            var nextPageLink = {lowercaseEntityVariable}sFromRepo.HasNext
                ? Create{entityNamePlural}ResourceUri({lowercaseEntityVariable}ParametersDto,
                    ResourceUriType.NextPage)
                : null;

            var paginationMetadata = new
            {{
                totalCount = {lowercaseEntityVariable}sFromRepo.TotalCount,
                pageSize = {lowercaseEntityVariable}sFromRepo.PageSize,
                pageNumber = {lowercaseEntityVariable}sFromRepo.PageNumber,
                totalPages = {lowercaseEntityVariable}sFromRepo.TotalPages,
                hasPrevious = {lowercaseEntityVariable}sFromRepo.HasPrevious,
                hasNext = {lowercaseEntityVariable}sFromRepo.HasNext,
                previousPageLink,
                nextPageLink
            }};

            Response.Headers.Add(""X-Pagination"",
                JsonSerializer.Serialize(paginationMetadata));

            var {lowercaseEntityVariable}sDto = _mapper.Map<IEnumerable<{entityName}Dto>>({lowercaseEntityVariable}sFromRepo);
            return Ok({lowercaseEntityVariable}sDto);
        }}


        [HttpGet(""{{{lowercaseEntityVariable}Id}}"", Name = ""Get{entityName}"")]
        public ActionResult<{readDto}> Get{entityName}(int {lowercaseEntityVariable}Id)
        {{
            var {lowercaseEntityVariable}FromRepo = _{lowercaseEntityVariable}Repository.Get{entityName}({lowercaseEntityVariable}Id);

            if ({lowercaseEntityVariable}FromRepo == null)
            {{
                return NotFound();
            }}

            var {lowercaseEntityVariable}Dto = _mapper.Map<{readDto}>({lowercaseEntityVariable}FromRepo);

            return Ok({lowercaseEntityVariable}Dto);
        }}

        [HttpPost]
        public ActionResult<{readDto}> Add{entityName}({creationDto} {lowercaseEntityVariable}ForCreation)
        {{
            var validationResults = new {entityName}ForCreationDtoValidator().Validate({lowercaseEntityVariable}ForCreation);
            validationResults.AddToModelState(ModelState, null);

            if (!ModelState.IsValid)
            {{
                return BadRequest(new ValidationProblemDetails(ModelState));
                //return ValidationProblem();
            }}

            var {lowercaseEntityVariable} = _mapper.Map<{entityName}>({lowercaseEntityVariable}ForCreation);
            _{lowercaseEntityVariable}Repository.Add{entityName}({lowercaseEntityVariable});
            var saveSuccessful = _{lowercaseEntityVariable}Repository.Save();

            if(saveSuccessful)
            {{
                var {lowercaseEntityVariable}Dto = _mapper.Map<{readDto}>({lowercaseEntityVariable});
                return CreatedAtRoute(""Get{entityName}"",
                    new {{ {lowercaseEntityVariable}Dto.{primaryKeyProp.Name} }},
                    {lowercaseEntityVariable}Dto);
            }}

            return StatusCode(500);
        }}

        [HttpDelete(""{{{lowercaseEntityVariable}Id}}"")]
        public ActionResult Delete{entityName}(int {lowercaseEntityVariable}Id)
        {{
            var {lowercaseEntityVariable}FromRepo = _{lowercaseEntityVariable}Repository.Get{entityName}({lowercaseEntityVariable}Id);

            if ({lowercaseEntityVariable}FromRepo == null)
            {{
                return NotFound();
            }}

            _{lowercaseEntityVariable}Repository.Delete{entityName}({lowercaseEntityVariable}FromRepo);
            _{lowercaseEntityVariable}Repository.Save();

            return NoContent();
        }}

        [HttpPut(""{{{lowercaseEntityVariable}Id}}"")]
        public IActionResult Update{entityName}(int {lowercaseEntityVariable}Id, {updateDto} {lowercaseEntityVariable})
        {{
            var {lowercaseEntityVariable}FromRepo = _{lowercaseEntityVariable}Repository.Get{entityName}({lowercaseEntityVariable}Id);

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

            _{lowercaseEntityVariable}Repository.Save();

            return NoContent();
        }}

        [HttpPatch(""{{{lowercaseEntityVariable}Id}}"")]
        public IActionResult PartiallyUpdate{entityName}(int {lowercaseEntityVariable}Id, JsonPatchDocument<{updateDto}> patchDoc)
        {{
            if (patchDoc == null)
            {{
                return BadRequest();
            }}

            var existing{entityName} = _{lowercaseEntityVariable}Repository.Get{entityName}({lowercaseEntityVariable}Id);

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

            _{lowercaseEntityVariable}Repository.Save(); // save changes in the database

            return NoContent();
        }}

        private string Create{entityNamePlural}ResourceUri(
            {entityName}ParametersDto {lowercaseEntityVariable}ParametersDto,
            ResourceUriType type)
        {{
            switch (type)
            {{
                case ResourceUriType.PreviousPage:
                    return Url.Link(""Get{entityNamePlural}"",
                        new
                        {{
                            filters = {lowercaseEntityVariable}ParametersDto.Filters,
                            orderBy = {lowercaseEntityVariable}ParametersDto.SortOrder,
                            pageNumber = {lowercaseEntityVariable}ParametersDto.PageNumber - 1,
                            pageSize = {lowercaseEntityVariable}ParametersDto.PageSize
                        }});
                case ResourceUriType.NextPage:
                    return Url.Link(""Get{entityNamePlural}"",
                        new
                        {{
                            filters = {lowercaseEntityVariable}ParametersDto.Filters,
                            orderBy = {lowercaseEntityVariable}ParametersDto.SortOrder,
                            pageNumber = {lowercaseEntityVariable}ParametersDto.PageNumber + 1,
                            pageSize = {lowercaseEntityVariable}ParametersDto.PageSize
                        }});

                default:
                    return Url.Link(""Get{entityNamePlural}"",
                        new
                        {{
                            filters = {lowercaseEntityVariable}ParametersDto.Filters,
                            orderBy = {lowercaseEntityVariable}ParametersDto.SortOrder,
                            pageNumber = {lowercaseEntityVariable}ParametersDto.PageNumber,
                            pageSize = {lowercaseEntityVariable}ParametersDto.PageSize
                        }});
            }}
        }}
    }}
}}";
        }
    }
}
