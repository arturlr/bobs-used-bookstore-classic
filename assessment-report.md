# Migration Assessment Report: BobsBookstoreClassic

## Solution Overview

| Attribute | Value |
|---|---|
| **Solution Name** | BobsBookstoreClassic |
| **Total Projects** | 5 |
| **Overall Complexity** | 🔴 **Critical** |
| **Total Lines of Code** | 7,095 |
| **Total Packages (all projects)** | 61 (unique: ~52) |
| **Incompatible Packages** | 16 |
| **Non-Upgradeable APIs** | 254 |
| **Target Framework** | net10.0 |
| **.NET Core Readiness** | Partial |
| **Linux Readiness** | Partial |

---

## Executive Summary

The **BobsBookstoreClassic** solution is a .NET Framework 4.8 ASP.NET MVC 5 bookstore application consisting of 5 projects totaling 7,095 lines of code. The migration to .NET 10 requires a **fundamental architectural shift** for the web layer (ASP.NET MVC 5 → ASP.NET Core MVC) and modernization of the data access layer (Entity Framework 6 → EF Core consideration).

### Key Statistics
- **3 projects** on .NET Framework 4.8 (Bookstore.Web, Bookstore.Data, Bookstore.Domain) — these are the HIGH-risk focus
- **1 project** on net6.0 (Bookstore.Cdk) — straightforward TFM bump
- **1 project** on netstandard2.0 (Bookstore.Common) — already cross-platform compatible
- **16 incompatible NuGet packages** — 15 in Bookstore.Web alone (ASP.NET MVC 5, OWIN, Web.Optimization stack)
- **254 non-upgradeable APIs** across the solution
- **400+ build errors** expected in Bookstore.Web during migration
- **42 Razor views** and **10 controllers** requiring conversion in Bookstore.Web

### Migration Approach
The recommended approach is a **bottom-up, dependency-ordered migration**:
1. Migrate leaf projects first (Bookstore.Common, Bookstore.Domain)
2. Migrate mid-tier projects (Bookstore.Cdk, Bookstore.Data)
3. Migrate the web application last (Bookstore.Web) — this is the most complex step
4. Update Dockerfiles for .NET 10 base images

---

## Project Analysis Table

| Project | Framework | Target | LOC | Packages | Incompatible | Complexity |
|---------|-----------|--------|-----|----------|-------------|------------|
| Bookstore.Common | netstandard2.0 | net10.0 | 7 | 0 | 0 | 🟢 Low |
| Bookstore.Domain | v4.8 | net10.0 | 1,451 | 0 | 0 | 🟡 Medium |
| Bookstore.Cdk | net6.0 | net10.0 | 505 | 4 | 1 (unknown) | 🟢 Low |
| Bookstore.Data | v4.8 | net10.0 | 830 | 4 | 2 | 🟡 Medium |
| Bookstore.Web | v4.8 | net10.0 | 4,302 | 53 | 15 | 🔴 Critical |

---

## Cross-Project Package Summary

The following packages are shared across multiple projects:

| Package | Bookstore.Web Version | Bookstore.Data Version | Conflict? | Notes |
|---------|----------------------|----------------------|-----------|-------|
| EntityFramework | 6.5.1 | 6.0.0 | ⚠️ Yes | Web uses 6.5.1 (compatible), Data uses 6.0.0 (incompatible). Align to 6.5.1 or migrate to EF Core |
| AWSSDK.Rekognition | 3.7.400.129 | 3.3.0 | ⚠️ Yes | Data uses legacy 3.3.0 (incompatible). Upgrade to 3.7.x+ |
| AWSSDK.S3 | 3.7.416.5 | 3.3.0 | ⚠️ Yes | Data uses legacy 3.3.0. Upgrade to 3.7.x+ |

---

## Per-Project Analysis

---

### 1. Bookstore.Common

#### Package Compatibility Table

| Package | Current Version | Target Package(s) | Target Version | Strategy | Complexity | Priority |
|---------|----------------|-------------------|----------------|----------|------------|----------|
| *(none)* | — | — | — | — | — | — |

#### Package Metrics
- **Packages Analyzed**: 0
- **Incompatible Packages**: 0
- **Major Upgrades Required**: 0

#### Source Code Stats
- **Total LOC**: 7

#### Migration Overview
- **Complexity**: 🟢 **Low**
- **Summary**: Trivial migration. Already targets netstandard2.0 which is compatible with .NET 10. Only requires a TFM update to net10.0.
- **Estimated Changes**: 0
- **Blocking Issues**: None
- **Notes**: This is an ideal first-migrate project — a clean leaf node with no dependencies or package concerns.

#### Migration Risks
- None identified. This project is fully .NET Core ready.

---

### 2. Bookstore.Domain

#### Package Compatibility Table

| Package | Current Version | Target Package(s) | Target Version | Strategy | Complexity | Priority |
|---------|----------------|-------------------|----------------|----------|------------|----------|
| *(none)* | — | — | — | — | — | — |

#### Package Metrics
- **Packages Analyzed**: 0
- **Incompatible Packages**: 0
- **Major Upgrades Required**: 0

#### Source Code Stats
- **Total LOC**: 1,451

#### Migration Overview
- **Complexity**: 🟡 **Medium**
- **Summary**: Migration from .NET Framework 4.8 requires converting from old-style .csproj to SDK-style format and updating the target framework. With 23 non-upgradeable APIs, some code changes will be needed for Framework-specific API calls.
- **Estimated Changes**: ~23 API-level changes
- **Blocking Issues**: None
- **Notes**: No NuGet dependencies simplifies migration. The 23 non-upgradeable APIs are the primary concern — these are likely System.Web or other Framework-specific references that need modern replacements.

#### Migration Risks
- **Framework-specific APIs**: 23 APIs identified as not directly upgradeable. These will require manual code changes or replacement patterns.
- **Project format conversion**: Old-style .csproj → SDK-style requires careful handling of assembly references and build properties.

---

### 3. Bookstore.Cdk

#### Package Compatibility Table

| Package | Current Version | Target Package(s) | Target Version | Strategy | Complexity | Priority |
|---------|----------------|-------------------|----------------|----------|------------|----------|
| Amazon.CDK.Lib | 2.188.0 | Amazon.CDK.Lib | 2.189.0 | upgrade | Low | Low |
| Cdklabs.CdkNag | 2.35.66 | Cdklabs.CdkNag | 2.35.67 | upgrade | Low | Low |
| Constructs | 10.4.2 | Constructs | 10.4.3 | upgrade | Low | Low |
| Amazon.Jsii.Analyzers | * | Amazon.Jsii.Analyzers | * | rewrite | Critical | Critical |

#### Package Metrics
- **Packages Analyzed**: 4
- **Incompatible Packages**: 1 (Amazon.Jsii.Analyzers — unknown compatibility)
- **Major Upgrades Required**: 0

#### Source Code Stats
- **Total LOC**: 505

#### Migration Overview
- **Complexity**: 🟢 **Low**
- **Summary**: Straightforward TFM bump from net6.0 to net10.0. All AWS CDK packages are compatible. The Amazon.Jsii.Analyzers package has unknown compatibility but is an analyzer package unlikely to cause runtime issues.
- **Estimated Changes**: 5 (TFM update + minor NuGet version bumps)
- **Blocking Issues**: None
- **Notes**: This is an AWS CDK infrastructure-as-code project. Depends on Bookstore.Common which must be migrated first.

#### Migration Risks
- **Amazon.Jsii.Analyzers**: Unknown compatibility — may need investigation. As an analyzer, it runs at build time only and may simply need a version update.
- **net6.0 → net10.0**: While straightforward, should verify no breaking changes in CDK APIs between .NET 6 and .NET 10.

---

### 4. Bookstore.Data

#### Package Compatibility Table

| Package | Current Version | Target Package(s) | Target Version | Strategy | Complexity | Priority |
|---------|----------------|-------------------|----------------|----------|------------|----------|
| AWSSDK.Rekognition | 3.3.0 | AWSSDK.Rekognition | 3.3.100+ | upgrade | Medium | Medium |
| AWSSDK.S3 | 3.3.0 | AWSSDK.S3 | 3.3.5.7+ | upgrade | Low | Low |
| EntityFramework | 6.0.0 | EntityFramework | 6.3.0+ | upgrade | Medium | Medium |
| Magick.NET-Q8-AnyCPU | 14.6.0 | Magick.NET-Q8-AnyCPU | 14.7.0 | upgrade | Low | Low |

#### Package Metrics
- **Packages Analyzed**: 4
- **Incompatible Packages**: 2 (AWSSDK.Rekognition 3.3.0, EntityFramework 6.0.0)
- **Major Upgrades Required**: 2

#### Source Code Stats
- **Total LOC**: 830

#### Migration Overview
- **Complexity**: 🟡 **Medium**
- **Summary**: Migration from .NET Framework 4.8 with 4 NuGet dependencies, 2 of which are incompatible at current versions but have compatible successors. The EntityFramework dependency is the key decision point — upgrade EF6 to 6.3.0+ for .NET Core compatibility, or migrate to EF Core for long-term support.
- **Estimated Changes**: ~115 (5 package changes + ~110 API adjustments)
- **Blocking Issues**: None (all packages have compatible upgrade paths)
- **Notes**: Depends on Bookstore.Domain. The AWSSDK packages need major version bumps from 3.3.x to 3.7.x+.

#### Migration Risks
- **EntityFramework 6.0.0 → 6.3.0+**: Major version upgrade. While EF6 has .NET Core compatible builds (6.3.0+), the long-term recommendation is to migrate to EF Core. This would be a significant additional effort involving DbContext changes, migration format changes, and query syntax updates.
- **AWSSDK.Rekognition 3.3.0 → 3.3.100+**: Significant version jump — review for breaking API changes.
- **110 non-upgradeable APIs**: Substantial code changes needed for Framework-specific API calls in the data layer.
- **Project format conversion**: Old-style .csproj → SDK-style.

---

### 5. Bookstore.Web

#### Package Compatibility Table

| Package | Current Version | Target Package(s) | Target Version | Strategy | Complexity | Priority |
|---------|----------------|-------------------|----------------|----------|------------|----------|
| Antlr | 3.5.0.2 | — | — | rewrite | Critical | Critical |
| Autofac | 8.2.1 | Autofac | 8.3.0 | upgrade | Low | Low |
| Autofac.Mvc5 | 6.1.0 | Autofac.Extensions.DependencyInjection | latest | rewrite | Critical | Critical |
| Autofac.Owin | 7.1.0 | — | — | rewrite | Critical | Critical |
| AWS.Logger.Core | 3.3.3 | AWS.Logger.Core | 4.0.0 | upgrade | Medium | Medium |
| AWS.Logger.NLog | 3.3.4 | AWS.Logger.NLog | 4.0.0 | upgrade | Medium | Medium |
| AWSSDK.CloudWatchLogs | 3.7.410.17 | AWSSDK.CloudWatchLogs | 3.7.410.18 | upgrade | Low | Low |
| AWSSDK.Core | 3.7.402.35 | AWSSDK.Core | 3.7.402.36 | upgrade | Low | Low |
| AWSSDK.Rekognition | 3.7.400.129 | AWSSDK.Rekognition | 3.7.400.130 | upgrade | Low | Low |
| AWSSDK.S3 | 3.7.416.5 | AWSSDK.S3 | 3.7.416.13 | upgrade | Low | Low |
| AWSSDK.SimpleSystemsManagement | 3.7.404.10 | AWSSDK.SimpleSystemsManagement | 3.7.404.11 | upgrade | Low | Low |
| EntityFramework | 6.5.1 | EntityFramework | 6.5.1 | upgrade | Low | Low |
| jQuery | 3.7.1 | jQuery | 3.7.1 | upgrade | Low | Low |
| jQuery.Validation | 1.21.0 | — | — | rewrite | Critical | Critical |
| Microsoft.AspNet.Mvc | 5.3.0 | ASP.NET Core MVC (built-in) | — | rewrite | Critical | Critical |
| Microsoft.AspNet.Razor | 3.3.0 | ASP.NET Core Razor (built-in) | — | rewrite | Critical | Critical |
| Microsoft.AspNet.Web.Optimization | 1.1.3 | WebOptimizer or built-in bundling | — | rewrite | Critical | Critical |
| Microsoft.AspNet.WebPages | 3.3.0 | ASP.NET Core Razor Pages (built-in) | — | rewrite | Critical | Critical |
| Microsoft.Bcl.AsyncInterfaces | 9.0.3 | — (built into .NET 10) | — | upgrade | Low | Low |
| Microsoft.Bcl.Memory | 9.0.3 | — (built into .NET 10) | — | upgrade | Low | Low |
| Microsoft.Bcl.TimeProvider | 9.0.3 | — (built into .NET 10) | — | upgrade | Low | Low |
| Microsoft.CodeDom.Providers.DotNetCompilerPlatform | 4.1.0 | — (remove) | — | rewrite | Critical | Critical |
| Microsoft.Extensions.DependencyInjection.Abstractions | 9.0.3 | — (built into ASP.NET Core) | — | upgrade | Low | Low |
| Microsoft.Extensions.Logging.Abstractions | 9.0.3 | — (built into ASP.NET Core) | — | upgrade | Low | Low |
| Microsoft.IdentityModel.Abstractions | 8.7.0 | Microsoft.IdentityModel.Abstractions | 8.8.0 | upgrade | Low | Low |
| Microsoft.IdentityModel.JsonWebTokens | 8.7.0 | Microsoft.IdentityModel.JsonWebTokens | 8.8.0 | upgrade | Low | Low |
| Microsoft.IdentityModel.Logging | 8.7.0 | Microsoft.IdentityModel.Logging | 8.8.0 | upgrade | Low | Low |
| Microsoft.IdentityModel.Protocols | 8.7.0 | Microsoft.IdentityModel.Protocols | 8.8.0 | upgrade | Low | Low |
| Microsoft.IdentityModel.Protocols.OpenIdConnect | 8.7.0 | Microsoft.IdentityModel.Protocols.OpenIdConnect | 8.8.0 | upgrade | Low | Low |
| Microsoft.IdentityModel.Tokens | 8.7.0 | Microsoft.IdentityModel.Tokens | 8.8.0 | upgrade | Low | Low |
| Microsoft.jQuery.Unobtrusive.Validation | 4.0.0 | Microsoft.jQuery.Unobtrusive.Validation | 4.0.0 | upgrade | Low | Low |
| Microsoft.Owin | 4.2.2 | Microsoft.AspNetCore.Owin | 6.0.29 | replace | High | High |
| Microsoft.Owin.Host.SystemWeb | 4.2.2 | — (Kestrel / ASP.NET Core hosting) | — | rewrite | Critical | Critical |
| Microsoft.Owin.Security | 4.2.2 | ASP.NET Core Authentication (built-in) | — | rewrite | Critical | Critical |
| Microsoft.Owin.Security.Cookies | 4.2.2 | Microsoft.AspNetCore.Authentication.Cookies | — | rewrite | Critical | Critical |
| Microsoft.Owin.Security.OpenIdConnect | 4.2.2 | Microsoft.AspNetCore.Authentication.OpenIdConnect | 6.0.11 | replace | High | High |
| Microsoft.Web.Infrastructure | 2.0.1 | — (remove) | — | rewrite | Critical | Critical |
| Modernizr | 2.8.3 | Modernizr | 2.8.3 | upgrade | Low | Low |
| Newtonsoft.Json | 13.0.3 | Newtonsoft.Json | 13.0.4 | upgrade | Low | Low |
| NLog | 5.4.0 | NLog | 5.5.0 | upgrade | Low | Low |
| Owin | 1.0.0 | Microsoft.AspNetCore.Diagnostics | latest | replace | High | High |
| System.Buffers | 4.6.1 | — (built into .NET 10) | — | upgrade | Low | Low |
| System.Diagnostics.DiagnosticSource | 9.0.3 | — | — | rewrite | Critical | Critical |
| System.IdentityModel.Tokens.Jwt | 8.7.0 | System.IdentityModel.Tokens.Jwt | 8.8.0 | upgrade | Low | Low |
| System.IO.Pipelines | 9.0.3 | — (built into .NET 10) | — | upgrade | Low | Low |
| System.Memory | 4.6.3 | — (built into .NET 10) | — | upgrade | Low | Low |
| System.Numerics.Vectors | 4.6.1 | — (built into .NET 10) | — | upgrade | Low | Low |
| System.Runtime.CompilerServices.Unsafe | 6.1.2 | — (built into .NET 10) | — | upgrade | Low | Low |
| System.Text.Encoding | 4.3.0 | — (built into .NET 10) | — | upgrade | Low | Low |
| System.Text.Encodings.Web | 9.0.3 | — (built into .NET 10) | — | upgrade | Low | Low |
| System.Text.Json | 9.0.3 | — (built into .NET 10) | — | upgrade | Low | Low |
| System.Threading.Tasks.Extensions | 4.6.3 | — (built into .NET 10) | — | upgrade | Low | Low |
| System.ValueTuple | 4.6.1 | — (built into .NET 10) | — | upgrade | Low | Low |
| WebGrease | 1.6.0 | — (remove) | — | rewrite | Critical | Critical |

#### Package Metrics
- **Packages Analyzed**: 53
- **Incompatible Packages**: 15
- **Major Upgrades Required**: 15 (rewrites/replacements)
- **Packages Removable** (built into .NET 10): ~12

#### Source Code Stats
- **Total LOC**: 4,302
- **Razor Views (cshtml)**: 42
- **Controllers**: 10
- **ASPX Views**: 0

#### Migration Overview
- **Complexity**: 🔴 **Critical**
- **Summary**: This is a full ASP.NET MVC 5 + OWIN application requiring a complete web framework migration to ASP.NET Core. The migration involves rewriting the startup/hosting pipeline, authentication middleware, DI configuration, all controller base classes, view rendering approach, bundling/minification, and routing configuration.
- **Estimated Changes**: 84+ (package-level) plus 400+ build errors to resolve
- **Blocking Issues**: 9 incompatible NuGet packages (Microsoft.AspNet.Mvc, Microsoft.AspNet.Razor, Microsoft.AspNet.Web.Optimization, Microsoft.AspNet.WebPages, Microsoft.Owin, Microsoft.Owin.Host.SystemWeb, Microsoft.Owin.Security, Microsoft.Owin.Security.Cookies, Microsoft.Owin.Security.OpenIdConnect)
- **Notes**: This is the highest-effort project. The 42 Razor views and 10 controllers will all need syntax and namespace updates. The OWIN authentication pipeline (cookies + OpenID Connect) must be completely re-implemented using ASP.NET Core authentication middleware.

#### Migration Risks
- **ASP.NET MVC 5 → ASP.NET Core MVC**: Fundamental framework change affecting every controller (11 classes), every view (42 cshtml files), routing, filters, and model binding.
- **OWIN Pipeline → ASP.NET Core Middleware**: Complete rewrite of `Startup.cs` / authentication configuration. OWIN middleware classes become ASP.NET Core middleware.
- **Autofac.Mvc5 → Autofac.Extensions.DependencyInjection**: DI registration pattern changes from MVC5 resolver to ASP.NET Core's `IServiceProvider` integration.
- **Web.Optimization (bundling) → WebOptimizer or alternatives**: The `BundleConfig` approach doesn't exist in ASP.NET Core. Must adopt LibMan, WebOptimizer, or a build tool like webpack.
- **Entity Framework 6.5.1**: While compatible, strongly recommend evaluating EF Core migration for long-term support.
- **118 non-upgradeable APIs**: Significant code-level changes needed beyond package updates.
- **Build Errors**: ~400+ compilation errors expected during migration — primarily from missing System.Web.Mvc, OWIN, and controller base class references.
- **Dockerfile Updates**: Both the root Dockerfile and Bookstore.Web Dockerfile need base image updates from .NET Framework / .NET 6 images to .NET 10 images.

---

## Cross-Project Dependencies & Migration Ordering

```
Bookstore.Common (netstandard2.0) ─────┬──────────────────────> Bookstore.Cdk (net6.0)
                                        │
                                        ├──> Bookstore.Domain (v4.8) ──> Bookstore.Data (v4.8) ──┐
                                        │                                                          │
                                        └──────────────────────────────────────────────────────────> Bookstore.Web (v4.8)
```

### Recommended Migration Order

| Phase | Project | Complexity | Rationale |
|-------|---------|------------|-----------|
| 1 | Bookstore.Common | 🟢 Low | Leaf node, no dependencies, netstandard2.0 already compatible |
| 2 | Bookstore.Domain | 🟡 Medium | Leaf node, no packages, required by Data and Web |
| 3 | Bookstore.Cdk | 🟢 Low | Depends only on Common, net6.0 → net10.0 straightforward |
| 4 | Bookstore.Data | 🟡 Medium | Depends on Domain, EF6 + AWS SDK upgrades needed |
| 5 | Bookstore.Web | 🔴 Critical | Depends on all three, full ASP.NET Core migration |

---

## Key Findings

1. **Critical Web Migration**: Bookstore.Web represents ~61% of the total codebase (4,302 of 7,095 LOC) and is the most complex migration target. It requires a complete framework migration from ASP.NET MVC 5 to ASP.NET Core.

2. **OWIN → ASP.NET Core Middleware**: The entire OWIN authentication pipeline (Cookies + OpenID Connect) must be re-implemented using ASP.NET Core's built-in authentication system.

3. **Package Version Conflicts**: EntityFramework and AWSSDK packages have version mismatches between Bookstore.Web and Bookstore.Data that should be aligned during migration.

4. **System.* Polyfill Cleanup**: ~12 System.* NuGet packages in Bookstore.Web are polyfills that ship built-in with .NET 10 and should be removed.

5. **EF6 Decision Point**: Both Bookstore.Web and Bookstore.Data use Entity Framework 6. While EF6 6.5.1 works on .NET Core, migrating to EF Core is recommended for long-term support and performance benefits.

6. **Dockerfile Updates Required**: Both the root Dockerfile and Bookstore.Web/Dockerfile need updates to use .NET 10 SDK and runtime base images.

---

## External Dependencies

| Dependency Type | Details |
|----------------|---------|
| **AWS Services** | S3, Rekognition, CloudWatch Logs, Systems Manager (SSM) — all via AWSSDK packages (compatible) |
| **Authentication** | OpenID Connect provider (Cognito or external IdP) — requires ASP.NET Core auth reconfiguration |
| **Logging** | NLog → CloudWatch Logs via AWS.Logger.NLog — compatible, minor version bump |
| **Image Processing** | Magick.NET — compatible with .NET 10 |
| **Infrastructure** | AWS CDK (Bookstore.Cdk) — compatible, minor updates needed |

---

## Actionable Next Steps

### Immediate (Phase 1 — Week 1)
1. ✅ Migrate **Bookstore.Common** — Convert TFM from netstandard2.0 to net10.0 (minimal effort)
2. ✅ Migrate **Bookstore.Domain** — Convert to SDK-style .csproj, update TFM to net10.0, resolve 23 non-upgradeable APIs

### Short-term (Phase 2 — Week 1-2)
3. ✅ Migrate **Bookstore.Cdk** — Update TFM from net6.0 to net10.0, bump CDK package versions
4. ✅ Migrate **Bookstore.Data** — Convert to SDK-style .csproj, upgrade AWSSDK packages (3.3.x → 3.7.x+), upgrade EntityFramework (6.0.0 → 6.5.1+), resolve 110 non-upgradeable APIs

### Medium-term (Phase 3 — Weeks 2-4)
5. 🔴 Migrate **Bookstore.Web** — This is the major effort:
   - Convert to SDK-style .csproj targeting net10.0
   - Replace ASP.NET MVC 5 with ASP.NET Core MVC
   - Rewrite OWIN startup to ASP.NET Core `Program.cs` / middleware pipeline
   - Migrate authentication (OWIN Cookies + OpenID Connect → ASP.NET Core Authentication)
   - Replace Autofac.Mvc5 with Autofac.Extensions.DependencyInjection
   - Replace Web.Optimization bundling with WebOptimizer or alternative
   - Update all 42 Razor views for ASP.NET Core syntax
   - Update all 10 controllers for ASP.NET Core base classes
   - Remove ~12 System.* polyfill packages
   - Resolve 400+ expected build errors
6. 🐳 Update **Dockerfiles** — Update base images to .NET 10 SDK/runtime images

### Post-Migration
7. Run full regression testing across all projects
8. Evaluate EF6 → EF Core migration for long-term support
9. Review and optimize the ASP.NET Core middleware pipeline
10. Update CI/CD pipelines for .NET 10 SDK
