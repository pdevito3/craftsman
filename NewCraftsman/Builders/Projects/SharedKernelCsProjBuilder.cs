namespace NewCraftsman.Builders.Projects
{
    using System.IO.Abstractions;
    using Helpers;
    using Services;

    public class SharedKernelCsProjBuilder
    {
        private readonly ICraftsmanUtilities _utilities;

        public SharedKernelCsProjBuilder(ICraftsmanUtilities utilities)
        {
            _utilities = utilities;
        }
        
        public void CreateSharedKernelCsProj(string solutionDirectory)
        {
            var classPath = ClassPathHelper.SharedKernelProjectClassPath(solutionDirectory);
            var fileText = GetMessagesCsProjFileText();
            _utilities.CreateFile(classPath, fileText);
        }

        public static string GetMessagesCsProjFileText()
        {
            return @$"<Project Sdk=""Microsoft.NET.Sdk"">

  <PropertyGroup>
    <TargetFramework>netstandard2.1</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include=""FluentValidation"" Version=""10.4.0"" />
  </ItemGroup>

</Project>";
        }
    }
}