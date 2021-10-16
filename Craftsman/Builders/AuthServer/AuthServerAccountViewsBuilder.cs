namespace Craftsman.Builders.AuthServer
{
    using System;
    using System.IO.Abstractions;
    using System.Linq;
    using Enums;
    using Helpers;
    using Models;
    using static Helpers.ConstMessages;

    public class AuthServerAccountViewsBuilder
    {
        public static void CreateLoginView(string projectDirectory, string authServerProjectName, IFileSystem fileSystem)
        {
            var classPath = ClassPathHelper.AuthServerViewsSubDirClassPath(projectDirectory,
                "Login.cshtml",
                authServerProjectName,
                ClassPathHelper.AuthServerViewSubDir.Account);
            var fileText = GetLoginViewText(projectDirectory, authServerProjectName);
            Utilities.CreateFile(classPath, fileText, fileSystem);
        }
        
        public static void CreateLogoutView(string projectDirectory, string authServerProjectName, IFileSystem fileSystem)
        {
            var classPath = ClassPathHelper.AuthServerViewsSubDirClassPath(projectDirectory,
                "Logout.cshtml",
                authServerProjectName,
                ClassPathHelper.AuthServerViewSubDir.Account);
            var fileText = GetLogoutViewText(projectDirectory, authServerProjectName);
            Utilities.CreateFile(classPath, fileText, fileSystem);
        }
        
        public static void CreateAccessDeniedView(string projectDirectory, string authServerProjectName, IFileSystem fileSystem)
        {
            var classPath = ClassPathHelper.AuthServerViewsSubDirClassPath(projectDirectory,
                "AccessDenied.cshtml",
                authServerProjectName,
                ClassPathHelper.AuthServerViewSubDir.Account);
            var fileText = GetAccessDeniedViewText(projectDirectory, authServerProjectName);
            Utilities.CreateFile(classPath, fileText, fileSystem);
        }
        
        public static string GetLoginViewText(string projectDirectory, string authServerProjectName)
        {
            var viewModelsClassPath = ClassPathHelper.AuthServerViewModelsClassPath(projectDirectory, "", authServerProjectName);
            
            return @$"@* {DuendeDisclosure}// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information. 

This file also uses a free template from Tailwind UI as a base.*@


@model {viewModelsClassPath.ClassNamespace}.LoginViewModel


@if (Model.EnableLocalLogin)
{{
  <div class=""min-h-screen flex sm:items-center justify-center bg-white sm:bg-gray-50 py-12 sm:px-6 lg:px-8"">
    <div class=""w-full px-4 space-y-8 bg-white rounded sm:shadow-md sm:p-12 sm:max-w-md "">
      <div>
        <img class=""mx-auto h-12 w-auto"" src=""https://tailwindui.com/img/logos/workflow-mark-blue-600.svg"" alt=""Workflow"" />
        <h2 class=""mt-6 text-center text-3xl font-extrabold text-gray-900"">Sign in to your account</h2>
      </div>
      <form class=""mt-8 space-y-6"" action=""#"" method=""POST"">
        <input type=""hidden"" name=""remember"" value=""true"" />
        <div class=""rounded-md shadow-sm -space-y-px"">
          <div>
            <label asp-for=""Username"" class=""sr-only"">Username</label>
            <input asp-for=""Username"" id=""Username"" name=""username"" required class=""appearance-none rounded-none relative block w-full px-3 py-2 border border-gray-300 placeholder-gray-500 text-gray-900 rounded-t-md focus:outline-none focus:ring-blue-500 focus:border-blue-500 focus:z-10 sm:text-sm"" placeholder=""Username"" />
          </div>
          <div>
            <label asp-for=""Password"" class=""sr-only"">Password</label>
            <input asp-for=""Password"" id=""Password"" name=""Password"" type=""password"" autocomplete=""current-password"" required class=""appearance-none rounded-none relative block w-full px-3 py-2 border border-gray-300 placeholder-gray-500 text-gray-900 rounded-b-md focus:outline-none focus:ring-blue-500 focus:border-blue-500 focus:z-10 sm:text-sm"" placeholder=""Password"" />
          </div>
        </div>

        <div class=""flex items-center justify-between"">
          @if (Model.AllowRememberLogin) {{
            <div class=""flex items-center"">
              <input asp-for=""RememberLogin"" id=""RememberLogin"" name=""RememberLogin"" type=""checkbox"" class=""h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"" />
              <label asp-for=""RememberLogin"" class=""ml-2 block text-sm text-gray-900""> Remember me </label>
            </div>
          }}

          <!-- <div class=""text-sm"">
            <a href=""#"" class=""font-medium text-blue-600 hover:text-blue-500"">
              Forgot your password?
            </a>
          </div> -->
        </div>

        <div>
          <button type=""submit"" value=""login"" class=""group relative w-full flex justify-center py-2 px-4 border border-transparent text-sm font-medium rounded-md text-white bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"">Sign in</button>
        </div>
      </form>
    </div>
  </div>
}}

@if (!Model.EnableLocalLogin && !Model.VisibleExternalProviders.Any())
{{
    <div class=""alert alert-warning"">
        <strong>Invalid login request</strong>
        There are no login schemes configured for this request.
    </div>
}}";
        }
        
        public static string GetLogoutViewText(string projectDirectory, string authServerProjectName)
        {
            var viewModelsClassPath = ClassPathHelper.AuthServerViewModelsClassPath(projectDirectory, "", authServerProjectName);
            return @$"@* {DuendeDisclosure}// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information. *@


@model {viewModelsClassPath.ClassNamespace}.LogoutViewModel


<div class=""flex-1 h-full w-full max-h-full max-w-full text-xs md:text-sm relative mt-auto"">
    <div class=""h-full flex-1 flex-wrap w-full max-h-full max-w-full items-center flex-no-wrap"">
        <div class=""flex p-3 md:p-5 overflow-hidden h-full"">
             <div class=""flex flex-col relative space-y-5 justify-center items-center w-full max-w-screen-lg mx-auto my-auto"">

                <div class=""w-full justify-center h-80 relative mx-auto flex md:w-1/2 bg-cover rounded-l-lg my-auto"">
                    @* <img src=""~/images/LoginLogo.png"" alt=""Sample Photo"" /> *@
                    <div class=""flex logout-svg w-8/12 whitespace-pre""></div>
                </div>

                <form class=""absolute bottom-0 mr-2"" asp-action=""Logout"">
                    <input type=""hidden"" name=""logoutId"" value=""@Model.LogoutId""  />
                    <div>
                        <button name=""button"" value=""login"" class="" px-1 h-10 w-24 md:w-32 text-sm rounded-lg md:text-base focus:outline-none font-semibold border border-white outline-none transition duration-200 focus:delay-75 bg-blue-500 text-white focus:ring-blue-500 focus:ring-2 hover:bg-blue-600"">
                            <div class=""flex flex-row flex-nowrap space-x-2 leading-none justify-center items-center"">
                                <div class=""text-center items-center p-1"">
                                    Logout
                                </div>
                            </div>
                        </button>
                    </div>
                </form>
            </div>
        </div>
    </div>
</div>";
        }
        
        public static string GetAccessDeniedViewText(string projectDirectory, string authServerProjectName)
        {
            var viewModelsClassPath = ClassPathHelper.AuthServerViewModelsClassPath(projectDirectory, "", authServerProjectName);
            return @$"@* {DuendeDisclosure}// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information. *@


<div class=""flex-1 h-full w-full max-h-full max-w-full text-xs md:text-sm relative mt-auto"">
    <div class=""h-full flex-1 flex-wrap w-full max-h-full max-w-full items-center flex-no-wrap"">
        <div class=""flex p-3 md:p-5 overflow-hidden h-full"">
             <div class=""flex flex-col space-y-5 justify-center items-center w-full max-w-screen-lg mx-auto my-auto"">
                <h1>
                    <small class=""flex text-2xl md:text-3xl font-semibold text-gray-700"">Access Denied</small>
                </h1>                
                <p>You do not have access to that resource.</p>
            </div>
        </div>
    </div>
</div>";
        }
    }
}