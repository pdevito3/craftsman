namespace Craftsman.Builders.AuthServer
{
  using Helpers;
  using Services;
  using static Helpers.ConstMessages;

    public class AuthServerAccountViewsBuilder
    {
      private readonly ICraftsmanUtilities _utilities;

      public AuthServerAccountViewsBuilder(ICraftsmanUtilities utilities)
      {
        _utilities = utilities;
      }

        public void CreateLoginView(string projectDirectory, string authServerProjectName)
        {
            var classPath = ClassPathHelper.AuthServerViewsSubDirClassPath(projectDirectory,
                "Login.cshtml",
                authServerProjectName,
                ClassPathHelper.AuthServerViewSubDir.Account);
            var fileText = GetLoginViewText(projectDirectory, authServerProjectName);
            _utilities.CreateFile(classPath, fileText);
        }
        
        public void CreateLogoutView(string projectDirectory, string authServerProjectName)
        {
            var classPath = ClassPathHelper.AuthServerViewsSubDirClassPath(projectDirectory,
                "Logout.cshtml",
                authServerProjectName,
                ClassPathHelper.AuthServerViewSubDir.Account);
            var fileText = GetLogoutViewText(projectDirectory, authServerProjectName);
            _utilities.CreateFile(classPath, fileText);
        }
        
        public void CreateAccessDeniedView(string projectDirectory, string authServerProjectName)
        {
            var classPath = ClassPathHelper.AuthServerViewsSubDirClassPath(projectDirectory,
                "AccessDenied.cshtml",
                authServerProjectName,
                ClassPathHelper.AuthServerViewSubDir.Account);
            var fileText = GetAccessDeniedViewText(projectDirectory, authServerProjectName);
            _utilities.CreateFile(classPath, fileText);
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
  <div class=""min-h-screen flex flex-col space-y-3 sm:items-center justify-center bg-white sm:bg-gray-50 py-12 sm:px-6 lg:px-8"">
    <div class=""w-full px-4 space-y-8 bg-white rounded-lg sm:shadow-md sm:p-12 sm:max-w-md "">
      <div class=""flex items-center justify-center"">
        <svg class=""h-12 w-12"" xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 100 111.812"">
          <path d=""M98.53 83.86c-3.106 5.341-10.064 7.205-15.406 3.975-1.615-.994-2.981-2.36-3.851-3.852-1.864-3.106-4.224-5.839-7.081-8.2-1.864-1.242-3.727-2.484-5.715-3.726-1.988-1.118-3.976-2.112-6.088-2.982-1.118-.497-2.236-.745-3.354-1.118-.249 1.118-.497 2.236-.746 3.479-.372 2.236-.497 4.472-.497 6.832s.125 4.473.497 6.71c.497 3.602 1.74 7.08 3.479 10.187.87 1.615 1.367 3.354 1.367 5.342.124 6.211-4.97 11.305-11.182 11.305s-11.305-5.094-11.18-11.305c0-1.864.496-3.727 1.366-5.342 1.74-3.106 2.981-6.585 3.478-10.188.373-2.236.497-4.472.497-6.709 0-2.236-.124-4.472-.497-6.832a17.408 17.408 0 00-.745-3.48 17.718 17.718 0 00-3.354 1.119c-2.112.87-4.1 1.864-6.088 2.982-1.988 1.118-3.851 2.36-5.59 3.851-2.858 2.236-5.218 5.094-7.082 8.2-.87 1.49-2.236 2.857-3.851 3.85-5.342 3.107-12.3 1.367-15.405-3.975-3.106-5.342-1.243-12.299 4.224-15.28 1.615-.995 3.478-1.367 5.217-1.492 3.603 0 7.206-.745 10.56-2.112 2.112-.87 4.1-1.863 6.088-2.981 1.988-1.118 3.851-2.485 5.59-3.851 2.61-1.988 4.722-4.473 6.46-7.206l.374-.746c.124-.124.124-.248.248-.372 1.615-2.982 2.733-6.212 3.23-9.566.373-2.237.497-4.473.497-6.71 0-2.235-.124-4.472-.497-6.708-.497-3.603-1.74-7.081-3.478-10.187-.622-1.864-1.119-3.603-1.119-5.467C38.772 5.094 43.866 0 50.078 0s11.305 5.094 11.18 11.305a11.38 11.38 0 01-1.366 5.343c-1.739 3.105-2.981 6.584-3.478 10.187-.373 2.236-.497 4.472-.497 6.709 0 2.236.124 4.472.497 6.708.497 3.479 1.615 6.71 3.354 9.815 0 .124.124.124.124.248l.373.746a29.855 29.855 0 006.46 7.206c1.74 1.366 3.603 2.733 5.59 3.85 1.989 1.119 3.976 2.113 6.088 2.982 3.355 1.367 6.958 1.988 10.56 2.112 1.864 0 3.603.497 5.218 1.491 5.467 2.858 7.454 9.815 4.349 15.157z"" fill=""#7A3ED7""/>
        </svg>
      </div>
      <form class=""mt-8"" asp-route=""Login"">
        <div class=""space-y-3"">
            <input type=""hidden"" asp-for=""ReturnUrl"" />
            <div class=""rounded-md shadow-sm space-y-3"">
              <div class=""space-y-1"">
                <label class=""text-sm text-gray-800"" asp-for=""Username"">Username</label>
                <input asp-for=""Username"" id=""Username"" name=""username"" required class=""appearance-none relative block w-full px-3 py-2 border border-gray-300 placeholder-gray-500 text-gray-900 rounded-md focus:outline-none focus:ring-violet-500 focus:border-violet-500 focus:z-10 sm:text-sm"" placeholder=""Username"" />
              </div>
              <div class=""space-y-1"">
                <label class=""text-sm text-gray-800"" asp-for=""Username"">Password</label>
                <input asp-for=""Password"" id=""Password"" name=""Password"" type=""password"" autocomplete=""current-password"" required class=""appearance-none relative block w-full px-3 py-2 border border-gray-300 placeholder-gray-500 text-gray-900 rounded-md focus:outline-none focus:ring-violet-500 focus:border-violet-500 focus:z-10 sm:text-sm"" placeholder=""Password"" />
              </div>
            </div>
        </div>

        <div class=""pt-3 flex items-center justify-between"">
          @if (Model.AllowRememberLogin) {{
            <div class=""flex items-center"">
              <input asp-for=""RememberLogin"" id=""RememberLogin"" name=""RememberLogin"" type=""checkbox"" class=""h-4 w-4 text-violet-600 focus:ring-violet-500 border-gray-300 rounded"" />
              <label asp-for=""RememberLogin"" class=""ml-2 block text-sm text-gray-900""> Remember me </label>
            </div>
          }}

          <!-- <div class=""text-sm"">
            <a href=""#"" class=""font-medium text-violet-600 hover:text-violet-500"">
              Forgot your password?
            </a>
          </div> -->
        </div>

        <div class=""pt-6"">
          <button name=""button"" type=""submit"" value=""login"" class=""group relative w-full flex justify-center py-2 px-4 border border-transparent text-sm font-medium rounded-md text-white bg-violet-600 hover:bg-violet-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-violet-500"">Sign in</button>
        </div>
      </form>
    </div>
    <div class=""rounded-lg bg-gray-100 shadow-md border border-gray-300 px-5 py-4 max-w-lg"">
      <div class=""flex items-center justify-start text-gray-800"">
        <div class="""">
          <svg class=""h-6 w-6 fill-current"" xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 512 512"">
            <!--! Font Awesome Pro 6.0.0 by fontawesome - https://fontawesome.com License - https://fontawesome.com/license (Commercial License) Copyright 2022 Fonticons, Inc. -->
            <path d=""M256 0C114.6 0 0 114.6 0 256s114.6 256 256 256s256-114.6 256-256S397.4 0 256 0zM256 128c17.67 0 32 14.33 32 32c0 17.67-14.33 32-32 32S224 177.7 224 160C224 142.3 238.3 128 256 128zM296 384h-80C202.8 384 192 373.3 192 360s10.75-24 24-24h16v-64H224c-13.25 0-24-10.75-24-24S210.8 224 224 224h32c13.25 0 24 10.75 24 24v88h16c13.25 0 24 10.75 24 24S309.3 384 296 384z"" />
          </svg>
        </div>
        <div class=""px-5 text-sm"">
          <ul class=""space-y-4 sm:space-y-1"">
            <li class="""">User <span class=""text-blue-400 font-medium"">alice</span> with password <span class=""text-blue-400 font-medium"">alice</span> has a Super User role.</li>
            <li class="""">User <span class=""text-blue-400 font-medium"">bob</span> with password <span class=""text-blue-400 font-medium"">bob</span> has a User role.</li>
            <li class="""">User <span class=""text-blue-400 font-medium"">john</span> with password <span class=""text-blue-400 font-medium"">john</span> has no role.</li>
          </ul>
        </div>
      </div>
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

<div class=""min-h-screen flex sm:items-center justify-center bg-white sm:bg-gray-50 py-12 sm:px-6 lg:px-8"">
  <div class=""w-full px-4 space-y-8 bg-white rounded-md sm:shadow-md sm:p-12 sm:max-w-md "">
    <div class="""">
      <h1 class=""font-semibold text-2xl text-gray-900"">Logout</h1>
      <p class=""text-lg text-gray-900 pt-1"">Would you like to logout of IdentityServer?</p>
    </div>

    <form class="""" asp-action=""Logout"">
      <input type=""hidden"" name=""logoutId"" value=""@Model.LogoutId"" />
      <div class="""">
        <button class=""group relative flex justify-center py-2 px-4 border border-transparent text-sm font-medium rounded-md text-white bg-violet-600 hover:bg-violet-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-violet-500"">Logout</button>
      </div>
    </form>
  </div>
</div>";
        }
        
        public static string GetAccessDeniedViewText(string projectDirectory, string authServerProjectName)
        {
            var viewModelsClassPath = ClassPathHelper.AuthServerViewModelsClassPath(projectDirectory, "", authServerProjectName);
            return @$"@* {DuendeDisclosure}// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information. *@


<div class=""min-h-screen flex sm:items-center justify-center bg-white sm:bg-gray-50 py-12 sm:px-6 lg:px-8"">
  <div class=""w-full px-4 space-y-8 bg-white rounded-md sm:shadow-md sm:p-12 sm:max-w-md "">
    <div class="""">
      <h1 class=""font-semibold text-2xl text-gray-900"">Access Denied</h1>
      <p class=""text-lg text-gray-900 pt-1"">You do not have access to that resource.</p>
    </div>
  </div>
</div>";
        }
    }
}