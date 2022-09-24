namespace Craftsman.Builders.NextJs.Domain.Features;

using Craftsman.Domain;
using Craftsman.Domain.Enums;
using Craftsman.Helpers;
using Craftsman.Services;

public class NextJsEntityFormBuilder
{
    private readonly ICraftsmanUtilities _utilities;

    public NextJsEntityFormBuilder(ICraftsmanUtilities utilities)
    {
        _utilities = utilities;
    }

    public void CreateFile(string nextSrc, string entityName, string entityPlural, List<NextJsEntityProperty> properties)
    {
        var routesIndexClassPath = ClassPathHelper.NextJsSpaFeatureClassPath(nextSrc,
            entityPlural,
            NextJsDomainCategory.Features,
            $"{FileNames.NextJsEntityFeatureFormName(entityName)}.tsx");
        var routesIndexFileText = GetFileText(entityName, entityPlural, properties);
        _utilities.CreateFile(routesIndexClassPath, routesIndexFileText);
    }

    public static string GetFileText(string entityName, string entityPlural, List<NextJsEntityProperty> properties)
    {
        var dtoForCreationName = FileNames.GetDtoName(entityName, Dto.Creation);
        var dtoForUpdateName = FileNames.GetDtoName(entityName, Dto.Update);
        var readDtoName = FileNames.GetDtoName(entityName, Dto.Read);
        var entityPluralLowercase = entityPlural.ToLower();
        var entityPluralLowercaseFirst = entityPlural.LowercaseFirstLetter();
        var entityUpperFirst = entityName.UppercaseFirstLetter();
        var entityLowerFirst = entityName.LowercaseFirstLetter();
        var validationSchema = FileNames.NextJsEntityValidationName(entityName);

        return @$"import {{
  Button,
  Checkbox,
  ComboBox,
  DatePicker,
  NumberInput,
  TextArea,
  TextInput,
}} from ""@/components/forms"";
import {{ Notifications }} from ""@/components/notifications"";
import {{
  {readDtoName},
  {dtoForCreationName},
  {dtoForUpdateName},
  {validationSchema},
  useAdd{entityUpperFirst},
  useUpdate{entityUpperFirst},
}} from ""@/domain/{entityPluralLowercaseFirst}"";
import {{ FormMode }} from ""@/types"";
import {{ getSimpleDirtyFields, useAutosave }} from ""@/utils"";
import {{ DevTool }} from ""@hookform/devtools"";
import {{ yupResolver }} from ""@hookform/resolvers/yup"";
import {{ useEffect }} from ""react"";
import {{ Controller, SubmitHandler, useForm }} from ""react-hook-form"";

interface {entityUpperFirst}FormProps {{
  {entityLowerFirst}Id?: string | undefined;
  {entityLowerFirst}Data?: {readDtoName};
}}

function {entityUpperFirst}Form({{ {entityLowerFirst}Id, {entityLowerFirst}Data }}: {entityUpperFirst}FormProps) {{
  const formMode = ({entityLowerFirst}Id ? ""Edit"" : ""Add"") as typeof FormMode[number];

  const focusField = ""{properties.FirstOrDefault()?.Name.LowercaseFirstLetter()}"";
  const {{
    handleSubmit,
    reset,
    control,
    setFocus,
    setValue,
    watch,
    formState: {{ dirtyFields, isValid }},
  }} = useForm<{entityUpperFirst}ForCreationDto | {entityUpperFirst}ForUpdateDto>({{
    mode: ""onBlur"",
    resolver: yupResolver({validationSchema}),
    defaultValues: {{{GetDefaultValues(properties)}
    }},
  }});

  useEffect(() => {{
    setFocus(focusField);
  }}, [setFocus]);

  const onSubmit: SubmitHandler<{entityUpperFirst}ForCreationDto | {entityUpperFirst}ForUpdateDto> = (
    data
  ) => {{
    formMode === ""Add"" ? create{entityUpperFirst}(data) : update{entityUpperFirst}(data);
    if (formMode === ""Add"") setFocus(focusField);
  }};

  const create{entityUpperFirst}Api = useAdd{entityUpperFirst}();
  function create{entityUpperFirst}(data: {entityUpperFirst}ForCreationDto) {{
    create{entityUpperFirst}Api
      .mutateAsync(data)
      .then(() => {{
        Notifications.success(""{entityUpperFirst} created successfully"");
      }})
      .then(() => {{
        reset();
      }})
      .catch((e) => {{
        Notifications.error(""There was an error creating the {entityLowerFirst}"");
        console.error(e);
      }});
  }}

  const update{entityUpperFirst}Api = useUpdate{entityUpperFirst}();
  function update{entityUpperFirst}(data: {entityUpperFirst}ForUpdateDto) {{
    const id = {entityLowerFirst}Id;
    if (id === null || id === undefined) return;

    update{entityUpperFirst}Api
      .mutateAsync({{ id, data }})
      .then(() => {{
        Notifications.success(""{entityUpperFirst} updated successfully"");
      }})
      .then(() => {{
        reset(
          {{ ...data }},
          {{
            keepValues: true,
          }}
        );
      }})
      .catch((e) => {{
        Notifications.error(""There was an error updating the {entityUpperFirst}"");
        console.error(e);
      }});
  }}

  useEffect(() => {{
    if (formMode === ""Edit"") {{
      {GetSetValues(properties, entityName)}
      reset(
        {{}},
        {{
          keepValues: true,
        }}
      );
    }}
  }}, [formMode, {entityLowerFirst}Data, reset, setValue]);

  const watchAllFields = watch();
  useAutosave({{
    handleSubmission: handleSubmit(onSubmit),
    isDirty: getSimpleDirtyFields(dirtyFields),
    isValid,
    formFields: watchAllFields,
  }});

  return (
    <>
      {{/* Need `noValidate` to allow RHF validation to trump browser validation when field is required */}}
      <form className=""space-y-4"" onSubmit={{handleSubmit(onSubmit)}} noValidate>
        {GetFormControls(properties, validationSchema)}

        {{formMode === ""Add"" && (
          <Button buttonStyle=""primary"" type=""submit"">
            Submit
          </Button>
        )}}
      </form>
      <DevTool control={{control}} placement={{""bottom-right""}} />
    </>
  );
}}

export {{ {entityUpperFirst}Form }};";
    }

    private static string GetDefaultValues(List<NextJsEntityProperty> properties)
    {
        var defaultValues = "";
        foreach (var property in properties)
        {
            defaultValues += property.TypeEnum.FormDefaultValue(property.Name);
        }

        return defaultValues;
    }

    private static string GetFormControls(List<NextJsEntityProperty> properties, string validationSchema)
    {
        var controls = "";
        foreach (var property in properties)
        {
            controls += property.TypeEnum.FormControl(property.Name, property.Label, validationSchema, property.FormControlEnum);
        }

        return controls;
    }

    private static string GetSetValues(List<NextJsEntityProperty> properties, string entityName)
    {
        var setValues = "";
        foreach (var property in properties)
        {
            setValues += property.TypeEnum.FormSetValue(property.Name, entityName);
        }

        return setValues;
    }
}
