# Migrate ASP.NET Framework 4.8 MVC Application to .NET 8

## Objective
Migrate an ASP.NET MVC application from .NET Framework 4.8 to .NET 8 (ASP.NET Core MVC), modernizing the project structure, dependency injection, configuration system, and framework-specific APIs while maintaining application functionality and leveraging the performance and cross-platform benefits of modern .NET.

## Summary
This transformation upgrades a legacy ASP.NET MVC application running on .NET Framework 4.8 to the modern .NET 8 platform with ASP.NET Core MVC. The migration involves converting the project file format from the legacy .csproj format to SDK-style projects, replacing OWIN-based middleware with ASP.NET Core middleware, migrating from packages.config to PackageReference, updating Entity Framework 6 to Entity Framework Core, replacing Web.config with appsettings.json, modernizing the dependency injection container from Autofac to the built-in ASP.NET Core DI container, and updating all NuGet packages to their .NET 8 compatible versions. The transformation maintains the existing MVC pattern, controller logic, and view structure while adapting to the new framework conventions and APIs.

## Entry Criteria
1. The application is an ASP.NET MVC application targeting .NET Framework 4.8 or earlier
2. The project uses the traditional .csproj format with extensive XML configuration
3. The application uses packages.config for NuGet package management
4. The application uses OWIN middleware for authentication and startup configuration
5. The application uses Entity Framework 6.x for data access
6. Configuration is stored in Web.config files
7. The application uses System.Web.Mvc for MVC functionality
8. Dependency injection is managed through Autofac or another third-party container
9. The solution contains .cshtml Razor views using ASP.NET MVC syntax

## Implementation Steps

1. **Backup and prepare the project**
   - Create a full backup of the existing solution
   - Document all external dependencies and third-party integrations
   - Identify custom middleware, filters, and HTTP modules that will need migration

2. **Create new .NET 8 project structure**
   - Create a new ASP.NET Core MVC project targeting .NET 8 using `dotnet new mvc`
   - Migrate the solution structure to use SDK-style .csproj files
   - Remove `<TargetFrameworkVersion>v4.8</TargetFrameworkVersion>` and replace with `<TargetFramework>net8.0</TargetFramework>`
   - Remove MSBuild imports and PropertyGroups specific to .NET Framework

3. **Convert project files to SDK-style format**
   - Replace the verbose .csproj format with SDK-style: `<Project Sdk="Microsoft.NET.Sdk.Web">`
   - Remove explicit file includes (SDK-style projects auto-include files by convention)
   - Convert packages.config to PackageReference format in the .csproj file
   - Remove unnecessary MSBuild targets and build customizations

4. **Migrate NuGet packages to .NET 8 compatible versions**
   - Replace `EntityFramework` 6.x with `Microsoft.EntityFrameworkCore` and `Microsoft.EntityFrameworkCore.SqlServer` for .NET 8
   - Replace `System.Web.Mvc` with ASP.NET Core MVC (included in the SDK)
   - Update AWS SDK packages (AWSSDK.Core, AWSSDK.S3, AWSSDK.Rekognition, AWSSDK.CloudWatchLogs, AWSSDK.SimpleSystemsManagement) to latest versions compatible with .NET 8
   - Replace `Autofac.Mvc5` and `Autofac.Owin` with `Autofac.Extensions.DependencyInjection` for ASP.NET Core
   - Replace `Microsoft.Owin` packages with ASP.NET Core middleware equivalents
   - Update `Newtonsoft.Json` or migrate to System.Text.Json (built into .NET 8)
   - Replace `NLog` with `NLog.Web.AspNetCore` for .NET 8
   - Update `AWS.Logger.NLog` to latest version supporting .NET 8
   - Remove packages that are now part of the framework (Microsoft.Web.Infrastructure, System.Web.Optimization)

5. **Migrate configuration from Web.config to appsettings.json**
   - Create appsettings.json and appsettings.Development.json files
   - Migrate connection strings from `<connectionStrings>` section to appsettings.json format
   - Convert `<appSettings>` to JSON configuration structure
   - Migrate authentication settings to ASP.NET Core authentication configuration
   - Update configuration access code to use `IConfiguration` interface instead of `ConfigurationManager`

6. **Replace Global.asax with Program.cs and Startup pattern**
   - Remove Global.asax and Global.asax.cs
   - Create Program.cs as the application entry point using .NET 8 minimal hosting model
   - Migrate Application_Start logic to Program.cs service configuration
   - Convert route registration from RouteConfig to Program.cs endpoint routing
   - Migrate bundle configuration to use ASP.NET Core bundling or external tools (Webpack, Vite)
   - Convert filter registration to use ASP.NET Core filter pipeline

7. **Migrate OWIN middleware to ASP.NET Core middleware**
   - Remove Startup.cs OWIN configuration class
   - Replace `Microsoft.Owin.Security.Cookies` with ASP.NET Core Cookie Authentication
   - Replace `Microsoft.Owin.Security.OpenIdConnect` with `Microsoft.AspNetCore.Authentication.OpenIdConnect`
   - Migrate custom OWIN middleware (e.g., LocalAuthenticationMiddleware) to ASP.NET Core middleware pattern
   - Update authentication configuration to use ASP.NET Core `AddAuthentication()` and `AddCookie()` methods
   - Replace OWIN context access with `HttpContext` in ASP.NET Core

8. **Migrate dependency injection from Autofac to ASP.NET Core DI**
   - Remove DependencyInjectionSetup.cs that configures Autofac
   - Register services using `builder.Services` in Program.cs
   - Use built-in ASP.NET Core DI for controller, service, and repository registration
   - Optionally integrate Autofac if advanced scenarios require it using `builder.Host.UseServiceProviderFactory()`
   - Update constructor injection in controllers to use ASP.NET Core patterns

9. **Update Entity Framework 6 to Entity Framework Core**
   - Replace DbContext inheritance from `System.Data.Entity.DbContext` to `Microsoft.EntityFrameworkCore.DbContext`
   - Update connection string configuration to use `DbContextOptionsBuilder`
   - Replace `Database.SetInitializer` with EF Core migration strategy
   - Convert Entity Framework 6 LINQ queries to EF Core compatible syntax
   - Update lazy loading configuration (EF Core requires explicit configuration)
   - Migrate database initializers to EF Core seeding pattern
   - Replace `DbSet.Find()` with equivalent EF Core methods where behavior differs
   - Update transaction handling from `TransactionScope` to EF Core transaction APIs

10. **Migrate controllers and action methods**
    - Update controller base class from `System.Web.Mvc.Controller` to `Microsoft.AspNetCore.Mvc.Controller`
    - Replace `System.Web.Mvc` attributes with `Microsoft.AspNetCore.Mvc` attributes
    - Update `ActionResult` types to ASP.NET Core equivalents (e.g., `ViewResult`, `JsonResult`, `RedirectToActionResult`)
    - Replace `HttpContext.Current` with injected `HttpContext` from controller base
    - Update `Request`, `Response`, and `Session` access patterns to ASP.NET Core APIs
    - Replace `Server.MapPath` with `IWebHostEnvironment.ContentRootPath` or `WebRootPath`
    - Update model binding attributes and validation to ASP.NET Core conventions
    - Replace `Request.Form`, `Request.QueryString` with strongly-typed parameter binding

11. **Update Razor views and view syntax**
    - Update `_ViewStart.cshtml` to use ASP.NET Core layout conventions
    - Replace `@using System.Web.Mvc` with `@using Microsoft.AspNetCore.Mvc`
    - Update `@Html` helper methods to ASP.NET Core tag helpers where applicable
    - Replace `@Url.Content()` with `~` path prefix or tag helpers
    - Update form helpers to use ASP.NET Core tag helpers (`<form asp-action="..."`)
    - Replace `@Scripts.Render` and `@Styles.Render` with environment tag helpers or direct references
    - Update `_ViewImports.cshtml` to include ASP.NET Core namespaces and tag helpers
    - Replace `ViewBag` with strongly-typed view models where possible (best practice)

12. **Migrate authentication and authorization**
    - Replace `[Authorize]` attributes (they work similarly but verify configuration)
    - Update claims-based identity access from `ClaimsPrincipal` to ASP.NET Core patterns
    - Replace `User.Identity` casts with ASP.NET Core identity APIs
    - Update cookie authentication configuration to ASP.NET Core format
    - Migrate OpenID Connect configuration to ASP.NET Core authentication middleware
    - Update sign-in and sign-out logic to use `HttpContext.SignInAsync` and `SignOutAsync`

13. **Update logging from NLog to ASP.NET Core logging with NLog**
    - Configure NLog using nlog.config compatible with ASP.NET Core
    - Replace direct NLog logger instantiation with `ILogger<T>` dependency injection
    - Update NLog AWS CloudWatch target configuration for .NET 8
    - Replace `LogManager.GetCurrentClassLogger()` with constructor-injected `ILogger<T>`
    - Update logging statements to use structured logging patterns

14. **Migrate Areas**
    - Update Area registration from `AreaRegistration.RegisterAllAreas()` to ASP.NET Core convention
    - Remove `AdminAreaRegistration.cs` and rely on ASP.NET Core area routing
    - Update area routes in Program.cs using `MapAreaControllerRoute()`
    - Verify area-specific views are in correct folder structure (Areas/{AreaName}/Views/{Controller})

15. **Update static file handling**
    - Move static files from project root to `wwwroot` folder (ASP.NET Core convention)
    - Update Content, Scripts, and Images folders to be under `wwwroot`
    - Configure static file middleware in Program.cs with `app.UseStaticFiles()`
    - Update references in views to point to wwwroot-relative paths

16. **Migrate custom helpers and extensions**
    - Update `ClaimsPrincipalExtensions`, `ControllerExtensions`, `HttpContextExtensions` to use ASP.NET Core types
    - Replace `System.Web.HttpContext` with `Microsoft.AspNetCore.Http.HttpContext`
    - Update custom attributes (MaxFileSizeAttribute, ImageTypesAttribute) to inherit from ASP.NET Core attribute base classes
    - Migrate HTML helper extensions to Tag Helpers (recommended ASP.NET Core pattern)

17. **Update file upload handling**
    - Replace `HttpPostedFileBase` with `IFormFile`
    - Update file upload validation attributes to work with `IFormFile`
    - Change file access patterns from `HttpPostedFileBase.InputStream` to `IFormFile.OpenReadStream()`

18. **Migrate session state**
    - Configure session in Program.cs using `builder.Services.AddSession()`
    - Add `app.UseSession()` middleware before routing
    - Update session access to use `HttpContext.Session` with ASP.NET Core APIs
    - Consider migrating to distributed cache for production scenarios

19. **Update compilation and build process**
    - Remove `MvcBuildViews` target (ASP.NET Core handles view compilation differently)
    - Configure Razor runtime compilation for development if needed
    - Update CI/CD pipelines to use `dotnet build` and `dotnet publish` instead of MSBuild
    - Remove web.config transforms (use environment-specific appsettings files instead)

20. **Test and validate the migration**
    - Run the application and verify all routes work correctly
    - Test authentication and authorization flows
    - Validate database connectivity and Entity Framework Core queries
    - Test file uploads and static file serving
    - Verify logging is working correctly with NLog and CloudWatch
    - Test all AWS SDK integrations (S3, Rekognition, SSM, CloudWatch)
    - Perform end-to-end testing of all features
    - Review and test error handling and custom error pages

## Validation / Exit Criteria

1. The application builds successfully targeting .NET 8 without errors or warnings
2. All project files use SDK-style format with `<Project Sdk="Microsoft.NET.Sdk.Web">`
3. All NuGet packages are compatible with .NET 8 and referenced using PackageReference format
4. The application starts successfully and Program.cs correctly configures all services and middleware
5. Configuration is loaded from appsettings.json and environment-specific configuration files work correctly
6. Entity Framework Core successfully connects to the database and all queries execute correctly
7. All controllers, actions, and routes are accessible and return expected results
8. Razor views render correctly with updated syntax and tag helpers
9. Authentication and authorization function correctly with ASP.NET Core identity system
10. File uploads work correctly using IFormFile
11. Static files are served correctly from wwwroot folder
12. Session state functions as expected (if used)
13. Logging works correctly with NLog writing to CloudWatch Logs
14. All AWS SDK integrations (S3, Rekognition, SSM) function correctly
15. Custom middleware, filters, and extensions operate as expected
16. Areas routing works correctly for Admin area and all area-specific controllers
17. All existing functionality has been tested and verified to work in .NET 8
18. Performance metrics meet or exceed .NET Framework 4.8 baseline
19. The application can be published and deployed to target environment successfully
20. No runtime dependencies on .NET Framework remain in the deployed application
