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
// See LICENSE in the project root for license information. *@


@model {viewModelsClassPath.ClassNamespace}.LoginViewModel

<div class=""flex-1 h-full w-full max-h-full max-w-full text-xs md:text-sm relative mt-auto"">

    @if (Model.EnableLocalLogin)
    {{
    <div class=""h-full flex-1 flex-wrap w-full max-h-full max-w-full items-center flex-no-wrap"">
        <div class=""flex p-3 md:p-5 overflow-hidden h-full"">
          <div class=""flex flex-row w-full max-w-screen-lg mx-auto my-auto shadow-lg rounded-xl border border-gray-300 bg-white overflow-hidden"">
            <div class=""w-full justify-center h-72 lg:h-80 relative mx-auto hidden md:flex md:w-1/2 bg-cover rounded-l-lg my-auto"">
                @* <img src=""~/images/LoginLogo.png"" alt=""Sample Photo"" /> *@
                <div class=""flex login-svg w-8/12 whitespace-pre mx-aut""></div>
            </div>

            @if (!Model.EnableLocalLogin)
            {{   
                <div class=""w-full md:w-1/2 bg-white px-1 my-auto h-full sm:px-2 md:px-5 pt-5 rounded-lg md:rounded-l-none p-2"">
                    <div class=""alert alert-warning"">
                        <strong>Invalid login request</strong>
                        There are no login schemes configured for this request.
                    </div>
                </div>
            }}else{{

                <div class=""w-full md:w-1/2 bg-white px-1 sm:px-2 md:px-5 pt-5 rounded-lg md:rounded-l-none"">
                    <h3 class=""pt-4 text-2xl text-center font-bold"">
                    Login
                    </h3>

                    <h6 class=""pt-2 my-2 text-center font-bold"">
                    👋 Welcome to identity server
                    </h6>

                    <form class=""px-8 pt-2 pb-8 mb-4 space-y-3 bg-white rounded"" asp-route=""Login"">

                        <partial name=""_ValidationSummary"" />

                        <input type=""hidden"" asp-for=""ReturnUrl"" />

                        <div class=""flex flex-col space-y-2"">
                            <label class=""text-sm sm:text-base font-semibold align-items-center pb-2 flex-no-wrap flex-grow-0 break-normal"" asp-for=""Username""></label>
                            
                            <div class=""flex flex-row my-auto justify-start align-middle outline content-center p-1 border-2 rounded-md transition duration-200 focus:bg-white focus-within:bg-white border-gray-200 focus-within:border-blue-500 hover:border-blue-500"">
                                <input class=""mx-2 my-0.5 w-full text-gray-500 focus:text-gray-700 placeholder-gray-500 outline-none border-transparent bg-transparent text-base"" placeholder=""Username"" asp-for=""Username"" autofocus>
                            </div> 
                        </div>
                        <div class=""flex flex-col space-y-2"">
                            <label class=""text-sm sm:text-base font-semibold align-items-center pb-2 flex-no-wrap flex-grow-0 break-normal"" asp-for=""Password""></label>
                        
                            <div type=""password"" class=""flex flex-row my-auto justify-start align-middle outline content-center p-1 border-2 rounded-md transition duration-200 focus:bg-white focus-within:bg-white border-gray-200 focus-within:border-blue-500 hover:border-blue-500"">
                                <input  type=""password"" class=""mx-2 my-0.5 w-full text-gray-500 focus:text-gray-700 placeholder-gray-500 outline-none border-transparent bg-transparent text-base"" placeholder=""Password"" asp-for=""Password"" autocomplete=""off"">
                            </div>
                        </div>
                        @if (Model.AllowRememberLogin)
                        {{

                        <div class=""flex mb-5 flex-row space-x-2 items-center"">
                            <input class=""my-auto "" asp-for=""RememberLogin"">
                            <label class=""text-sm flex my-auto leading-none justify-center items-center align-items-center flex-no-wrap flex-grow-0 break-normal"" class=""form-check-label"" asp-for=""RememberLogin"">
                                Remember My Login
                            </label>
                        </div>
                   
                        }}

                        <div class=""flex flex-row space-x-2"">
                            <button name=""button"" value=""login"" class="" px-1 h-10 w-16 md:w-20 text-sm rounded-lg md:text-base focus:outline-none font-semibold border border-white outline-none transition duration-200 focus:delay-75 bg-blue-500 text-white focus:ring-blue-500 focus:ring-2 hover:bg-blue-600"">
                                <div class=""flex flex-row flex-nowrap space-x-2 leading-none justify-center items-center"">
                                    <div class=""text-center items-center p-1"">
                                        Login
                                    </div>
                                </div>
                            </button>
                            <button name=""button"" value=""cancel"" class="" px-1 h-10 md:w-20 text-sm rounded-lg md:text-base focus:outline-none font-semibold border border-white outline-none transition duration-200 focus:delay-75 bg-gray-500 text-white focus:ring-gray-500 focus:ring-2 hover:bg-gray-600"">
                                <div class=""flex flex-row flex-nowrap space-x-2 leading-none justify-center items-center"">
                                    <div class=""text-center items-center p-1"">
                                        Cancel
                                    </div>
                                </div>
                            </button>
                        </div>
                    </form>
                </div>
            }}
          </div>
        </div>
      </div>
        }}
</div>";
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