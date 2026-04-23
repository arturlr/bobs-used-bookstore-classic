# Migration Plan: BobsBookstoreClassic

- **Solution Path**: /tmp/dotnet_migration_orchestrator/workdir/0573d4b8-3daf-4153-a76e-29d45125a951_extracted/BobsBookstoreClassic.sln
- **Created**: 2026-04-22T23:54:50Z
- **Default Target Framework**: net10.0

## Projects

### 1. Bookstore.Common

- **Path**: app/Bookstore.Common/Bookstore.Common.csproj
- **Current Framework**: netstandard2.0
- **Target Framework**: net10.0

#### Migration Overview

- **Complexity**: low
- **Summary**: Migration of Bookstore.Common from netstandard2.0.
- **Estimated Changes**: 0
- **Blocking Issues**:
  - None
- **Notes**: No special concerns identified.

#### Dependencies

- None

#### NuGet Packages

- None

#### Migration Risks

- None

### 2. Bookstore.Domain

- **Path**: app/Bookstore.Domain/Bookstore.Domain.csproj
- **Current Framework**: v4.8
- **Target Framework**: net10.0

#### Migration Overview

- **Complexity**: medium
- **Summary**: Migration of Bookstore.Domain from v4.8.
- **Estimated Changes**: 0
- **Blocking Issues**:
  - None
- **Notes**: Complexity raised from low to medium (framework floor for v4.8)

#### Dependencies

- None

#### NuGet Packages

- None

#### Migration Risks

- None

### 3. Bookstore.Cdk

- **Path**: app/Bookstore.Cdk/Bookstore.Cdk.csproj
- **Current Framework**: net6.0
- **Target Framework**: net10.0

#### Migration Overview

- **Complexity**: low
- **Summary**: Migration of Bookstore.Cdk from net6.0. 4 NuGet reference(s) to review. 1 project dependency/dependencies.
- **Estimated Changes**: 5
- **Blocking Issues**:
  - None
- **Notes**: No special concerns identified.

#### Dependencies

- Bookstore.Common

#### NuGet Packages

| Package | Current Version | Recommended Version | Compatible |
|---------|----------------|--------------------:|:----------:|
| Amazon.CDK.Lib | 2.188.0 | N/A | yes |
| Cdklabs.CdkNag | 2.35.66 | N/A | yes |
| Constructs | 10.4.2 | N/A | yes |
| Amazon.Jsii.Analyzers | * | N/A | yes |

#### Migration Risks

- None

### 4. Bookstore.Data

- **Path**: app/Bookstore.Data/Bookstore.Data.csproj
- **Current Framework**: v4.8
- **Target Framework**: net10.0

#### Migration Overview

- **Complexity**: medium
- **Summary**: Migration of Bookstore.Data from v4.8. 4 NuGet reference(s) to review. 1 project dependency/dependencies.
- **Estimated Changes**: 5
- **Blocking Issues**:
  - None
- **Notes**: Complexity raised from low to medium (framework floor for v4.8)

#### Dependencies

- Bookstore.Domain

#### NuGet Packages

| Package | Current Version | Recommended Version | Compatible |
|---------|----------------|--------------------:|:----------:|
| AWSSDK.Rekognition | 3.7.400.129 | N/A | yes |
| AWSSDK.S3 | 3.7.416.5 | N/A | yes |
| EntityFramework | 6.5.1 | N/A | yes |
| Magick.NET-Q8-AnyCPU | 14.6.0 | N/A | yes |

#### Migration Risks

- None

### 5. Bookstore.Web

- **Path**: app/Bookstore.Web/Bookstore.Web.csproj
- **Current Framework**: v4.8
- **Target Framework**: net10.0

#### Migration Overview

- **Complexity**: critical
- **Summary**: Migration of Bookstore.Web from v4.8. 9 incompatible NuGet package(s) require replacement. 54 NuGet reference(s) to review. 3 project dependency/dependencies.
- **Estimated Changes**: 84
- **Blocking Issues**:
  - Incompatible NuGet package: Microsoft.AspNet.Mvc
  - Incompatible NuGet package: Microsoft.AspNet.Razor
  - Incompatible NuGet package: Microsoft.AspNet.Web.Optimization
  - Incompatible NuGet package: Microsoft.AspNet.WebPages
  - Incompatible NuGet package: Microsoft.Owin
  - Incompatible NuGet package: Microsoft.Owin.Host.SystemWeb
  - Incompatible NuGet package: Microsoft.Owin.Security
  - Incompatible NuGet package: Microsoft.Owin.Security.Cookies
  - Incompatible NuGet package: Microsoft.Owin.Security.OpenIdConnect
- **Notes**: High NuGet dependency count (54)

#### Dependencies

- Bookstore.Common
- Bookstore.Data
- Bookstore.Domain

#### NuGet Packages

| Package | Current Version | Recommended Version | Compatible |
|---------|----------------|--------------------:|:----------:|
| Antlr | 3.5.0.2 | N/A | yes |
| Autofac | 8.2.1 | N/A | yes |
| Autofac.Mvc5 | 6.1.0 | N/A | yes |
| Autofac.Owin | 7.1.0 | N/A | yes |
| AWS.Logger.Core | 3.3.3 | N/A | yes |
| AWS.Logger.NLog | 3.3.4 | N/A | yes |
| AWSSDK.CloudWatchLogs | 3.7.410.17 | N/A | yes |
| AWSSDK.Core | 3.7.402.35 | N/A | yes |
| AWSSDK.Rekognition | 3.7.400.129 | N/A | yes |
| AWSSDK.S3 | 3.7.416.5 | N/A | yes |
| AWSSDK.SimpleSystemsManagement | 3.7.404.10 | N/A | yes |
| EntityFramework | 6.5.1 | N/A | yes |
| jQuery | 3.7.1 | N/A | yes |
| jQuery.Validation | 1.21.0 | N/A | yes |
| Microsoft.AspNet.Mvc | 5.3.0 | N/A | yes |
| Microsoft.AspNet.Razor | 3.3.0 | N/A | yes |
| Microsoft.AspNet.Web.Optimization | 1.1.3 | N/A | yes |
| Microsoft.AspNet.WebPages | 3.3.0 | N/A | yes |
| Microsoft.Bcl.AsyncInterfaces | 9.0.3 | N/A | yes |
| Microsoft.Bcl.Memory | 9.0.3 | N/A | yes |
| Microsoft.Bcl.TimeProvider | 9.0.3 | N/A | yes |
| Microsoft.CodeDom.Providers.DotNetCompilerPlatform | 4.1.0 | N/A | yes |
| Microsoft.Extensions.DependencyInjection.Abstractions | 9.0.3 | N/A | yes |
| Microsoft.Extensions.Logging.Abstractions | 9.0.3 | N/A | yes |
| Microsoft.IdentityModel.Abstractions | 8.7.0 | N/A | yes |
| Microsoft.IdentityModel.JsonWebTokens | 8.7.0 | N/A | yes |
| Microsoft.IdentityModel.Logging | 8.7.0 | N/A | yes |
| Microsoft.IdentityModel.Protocols | 8.7.0 | N/A | yes |
| Microsoft.IdentityModel.Protocols.OpenIdConnect | 8.7.0 | N/A | yes |
| Microsoft.IdentityModel.Tokens | 8.7.0 | N/A | yes |
| Microsoft.jQuery.Unobtrusive.Validation | 4.0.0 | N/A | yes |
| Microsoft.Owin | 4.2.2 | N/A | yes |
| Microsoft.Owin.Host.SystemWeb | 4.2.2 | N/A | yes |
| Microsoft.Owin.Security | 4.2.2 | N/A | yes |
| Microsoft.Owin.Security.Cookies | 4.2.2 | N/A | yes |
| Microsoft.Owin.Security.OpenIdConnect | 4.2.2 | N/A | yes |
| Microsoft.Web.Infrastructure | 2.0.1 | N/A | yes |
| Modernizr | 2.8.3 | N/A | yes |
| Newtonsoft.Json | 13.0.3 | N/A | yes |
| NLog | 5.4.0 | N/A | yes |
| Owin | 1.0 | N/A | yes |
| System.Buffers | 4.6.1 | N/A | yes |
| System.Diagnostics.DiagnosticSource | 9.0.3 | N/A | yes |
| System.IdentityModel.Tokens.Jwt | 8.7.0 | N/A | yes |
| System.IO.Pipelines | 9.0.3 | N/A | yes |
| System.Memory | 4.6.3 | N/A | yes |
| System.Numerics.Vectors | 4.6.1 | N/A | yes |
| System.Runtime.CompilerServices.Unsafe | 6.1.2 | N/A | yes |
| System.Text.Encoding | 4.3.0 | N/A | yes |
| System.Text.Encodings.Web | 9.0.3 | N/A | yes |
| System.Text.Json | 9.0.3 | N/A | yes |
| System.Threading.Tasks.Extensions | 4.6.3 | N/A | yes |
| System.ValueTuple | 4.6.1 | N/A | yes |
| WebGrease | 1.6.0 | N/A | yes |

#### Migration Risks

- Incompatible NuGet package: Microsoft.AspNet.Mvc
- Incompatible NuGet package: Microsoft.AspNet.Razor
- Incompatible NuGet package: Microsoft.AspNet.Web.Optimization
- Incompatible NuGet package: Microsoft.AspNet.WebPages
- Incompatible NuGet package: Microsoft.Owin
- Incompatible NuGet package: Microsoft.Owin.Host.SystemWeb
- Incompatible NuGet package: Microsoft.Owin.Security
- Incompatible NuGet package: Microsoft.Owin.Security.Cookies
- Incompatible NuGet package: Microsoft.Owin.Security.OpenIdConnect
