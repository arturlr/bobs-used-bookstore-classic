# Migration Plan: BobsBookstoreClassic

- **Solution Path**: a6cdb0d9-98e7-40fc-a949-496a754ff432_extracted/sourceCode/BobsBookstoreClassic.sln
- **Created**: 2026-04-28T00:11:19Z
- **Default Target Framework**: net10.0

## Projects

### 1. Bookstore.Common

- **Path**: a6cdb0d9-98e7-40fc-a949-496a754ff432_extracted/sourceCode/app/Bookstore.Common/Bookstore.Common.csproj
- **Current Framework**: net10.0
- **Target Framework**: net10.0

#### Migration Overview

- **Complexity**: low
- **Summary**: Migration of Bookstore.Common from net10.0.
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

- **Path**: a6cdb0d9-98e7-40fc-a949-496a754ff432_extracted/sourceCode/app/Bookstore.Domain/Bookstore.Domain.csproj
- **Current Framework**: net10.0
- **Target Framework**: net10.0

#### Migration Overview

- **Complexity**: low
- **Summary**: Migration of Bookstore.Domain from net10.0.
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

### 3. Bookstore.Data

- **Path**: a6cdb0d9-98e7-40fc-a949-496a754ff432_extracted/sourceCode/app/Bookstore.Data/Bookstore.Data.csproj
- **Current Framework**: net10.0
- **Target Framework**: net10.0

#### Migration Overview

- **Complexity**: medium
- **Summary**: Migration of Bookstore.Data from net10.0. 6 NuGet reference(s) to review. 1 project dependency.
- **Estimated Changes**: 10
- **Blocking Issues**:
  - None
- **Notes**: No special concerns identified.

#### Dependencies

- Bookstore.Domain

#### NuGet Packages

| Package | Current Version | Recommended Version | Compatible |
|---------|----------------|--------------------:|:----------:|
| System.Configuration.ConfigurationManager | 10.0.5 | N/A | yes |
| Magick.NET-Q8-AnyCPU | 14.11.1 | N/A | yes |
| AWSSDK.Rekognition | 3.7.400.129 | N/A | yes |
| AWSSDK.S3 | 3.7.416.5 | N/A | yes |
| Microsoft.EntityFrameworkCore | 10.0.5 | N/A | yes |
| Microsoft.EntityFrameworkCore.SqlServer | 10.0.5 | N/A | yes |

#### Migration Risks

- None

### 4. Bookstore.Web

- **Path**: a6cdb0d9-98e7-40fc-a949-496a754ff432_extracted/sourceCode/app/Bookstore.Web/Bookstore.Web.csproj
- **Current Framework**: net10.0
- **Target Framework**: net10.0

#### Migration Overview

- **Complexity**: medium
- **Summary**: Migration of Bookstore.Web from net10.0. 12 NuGet reference(s) to review. 3 project dependencies.
- **Estimated Changes**: 18
- **Blocking Issues**:
  - None
- **Notes**: Moderate NuGet dependency count (12)

#### Dependencies

- Bookstore.Common
- Bookstore.Data
- Bookstore.Domain

#### NuGet Packages

| Package | Current Version | Recommended Version | Compatible |
|---------|----------------|--------------------:|:----------:|
| Autofac | 8.2.1 | N/A | yes |
| Autofac.Extensions.DependencyInjection | 10.0.0 | N/A | yes |
| AWSSDK.CloudWatchLogs | 3.7.410.17 | N/A | yes |
| AWSSDK.Core | 3.7.402.35 | N/A | yes |
| AWSSDK.Rekognition | 3.7.400.129 | N/A | yes |
| AWSSDK.S3 | 3.7.416.5 | N/A | yes |
| AWSSDK.SimpleSystemsManagement | 3.7.404.10 | N/A | yes |
| AWS.Logger.Core | 3.3.3 | N/A | yes |
| AWS.Logger.NLog | 3.3.4 | N/A | yes |
| Microsoft.AspNetCore.Authentication.OpenIdConnect | 10.0.5 | N/A | yes |
| Newtonsoft.Json | 13.0.3 | N/A | yes |
| NLog | 5.4.0 | N/A | yes |

#### Migration Risks

- None
