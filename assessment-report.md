# Assessment Report: BobsBookstoreClassic

## Solution Overview

| Field | Value |
|-------|-------|
| **Solution Name** | BobsBookstoreClassic |
| **Total Projects** | 5 |
| **Overall Complexity** | High (Critical project: Bookstore.Web) |
| **Total Lines of Code** | 7,095 |
| **Total Packages** | 62 (across all projects) |
| **Incompatible Packages** | 16 |
| **Non-Upgradeable APIs** | 254 |
| **Target Framework** | net10.0 |

---

## Executive Summary

The BobsBookstoreClassic solution is a .NET Framework 4.8 ASP.NET MVC 5 web application with supporting class libraries and an AWS CDK infrastructure project. The transformation to .NET 10 requires significant effort primarily concentrated in the **Bookstore.Web** project, which uses ASP.NET MVC 5, OWIN middleware, and numerous framework-specific packages that have no direct .NET Core equivalents.

### Key Statistics
- **3 projects** target .NET Framework 4.8 (requiring full migration)
- **1 project** targets net6.0 (simple retarget)
- **1 project** targets netstandard2.0 (already compatible)
- **16 incompatible NuGet packages** require replacement or rewrite
- **254 non-upgradeable APIs** need code changes
- **42 Razor views** and **10 controllers** need ASP.NET Core migration

### Transformation Approach
1. Migrate leaf dependencies first (Bookstore.Common, Bookstore.Domain)
2. Migrate data layer (Bookstore.Data) with EF6/EF Core decision
3. Migrate infrastructure (Bookstore.Cdk) - simple retarget
4. Migrate web application (Bookstore.Web) - major rewrite from ASP.NET MVC 5 to ASP.NET Core MVC

---

## Project Analysis Table

| Project | Framework | Target | LOC | Packages | Incompatible | Complexity |
|---------|-----------|--------|-----|----------|--------------|------------|
| Bookstore.Common | netstandard2.0 | net10.0 | 7 | 0 | 0 | Low |
| Bookstore.Domain | v4.8 | net10.0 | 1,451 | 0 | 0 | Medium |
| Bookstore.Cdk | net6.0 | net10.0 | 505 | 4 | 1 | Low |
| Bookstore.Data | v4.8 | net10.0 | 830 | 4 | 2 | Medium |
| Bookstore.Web | v4.8 | net10.0 | 4,302 | 54 | 15 | Critical |

---

## Cross-Project Package Summary

| Package | Projects Using | Version(s) | Conflicts |
|---------|---------------|------------|-----------|
| EntityFramework | Bookstore.Data, Bookstore.Web | 6.0.0 / 6.5.1 | Version mismatch |
| AWSSDK.Rekognition | Bookstore.Data, Bookstore.Web | 3.3.0 / 3.7.400.129 | Version mismatch |
| AWSSDK.S3 | Bookstore.Data, Bookstore.Web | 3.3.0 / 3.7.416.5 | Version mismatch |

---

## Per-Project Assessment

---

### Bookstore.Common

#### Package Compatibility Table

No packages to analyze.

#### Package Metrics
- **Packages Analyzed:** 0
- **Incompatible:** 0
- **Major Upgrades Required:** 0

#### Source Code Stats
- **Total LOC:** 7

#### Transformation Overview
- **Complexity:** Low
- **Summary:** Trivial class library targeting netstandard2.0. Already compatible with modern .NET.
- **Estimated Changes:** 0
- **Blocking Issues:** None
- **Notes:** Can remain on netstandard2.0 or be retargeted to net10.0 with zero code changes.

#### Transformation Risks
- Minimal risk - project is already cross-platform compatible.

---

### Bookstore.Domain

#### Package Compatibility Table

No packages to analyze.

#### Package Metrics
- **Packages Analyzed:** 0
- **Incompatible:** 0
- **Major Upgrades Required:** 0

#### Source Code Stats
- **Total LOC:** 1,451

#### Transformation Overview
- **Complexity:** Medium
- **Summary:** Domain model library targeting .NET Framework 4.8 with no NuGet dependencies. Requires SDK-style project conversion and framework retarget. Has 23 non-upgradeable APIs.
- **Estimated Changes:** 0 package changes; API-level code changes for 23 incompatible APIs
- **Blocking Issues:** None
- **Notes:** Conversion to SDK-style project format is required. Domain models typically migrate cleanly.

#### Transformation Risks
- 23 non-upgradeable APIs may require code rewrites
- Need to verify all System.* API usage is compatible with net10.0

---

### Bookstore.Cdk

#### Package Compatibility Table

| Package | Current Version | Target Package(s) | Target Version | Strategy | Complexity | Priority |
|---------|----------------|-------------------|----------------|----------|------------|----------|
| Amazon.CDK.Lib | 2.188.0 | Amazon.CDK.Lib | 2.189.0 | upgrade | Low | Low |
| Cdklabs.CdkNag | 2.35.66 | Cdklabs.CdkNag | 2.35.67 | upgrade | Low | Low |
| Constructs | 10.4.2 | Constructs | 10.4.3 | upgrade | Low | Low |
| Amazon.Jsii.Analyzers | * | *(unknown)* | - | rewrite | Critical | Critical |

#### Package Metrics
- **Packages Analyzed:** 4
- **Incompatible:** 1 (Amazon.Jsii.Analyzers - UNKNOWN)
- **Major Upgrades Required:** 0

#### Source Code Stats
- **Total LOC:** 505

#### Transformation Overview
- **Complexity:** Low
- **Summary:** AWS CDK infrastructure project already on net6.0. Simple retarget to net10.0 with minor package version bumps.
- **Estimated Changes:** 5 (framework retarget + 3 package upgrades + 1 analyzer review)
- **Blocking Issues:** None (Amazon.Jsii.Analyzers is build-time only)
- **Notes:** Dependencies on Bookstore.Common. All CDK packages are actively maintained.

#### Transformation Risks
- Amazon.Jsii.Analyzers compatibility is unknown - may need to be removed or replaced
- CDK constructs may have breaking changes between minor versions (unlikely for patch versions)

---

### Bookstore.Data

#### Package Compatibility Table

| Package | Current Version | Target Package(s) | Target Version | Strategy | Complexity | Priority |
|---------|----------------|-------------------|----------------|----------|------------|----------|
| AWSSDK.Rekognition | 3.3.0 | AWSSDK.Rekognition | 3.3.100 | upgrade | Medium | Medium |
| AWSSDK.S3 | 3.3.0 | AWSSDK.S3 | 3.3.5.7 | upgrade | Low | Low |
| EntityFramework | 6.0.0 | EntityFramework | 6.3.0 | upgrade | Medium | Medium |
| Magick.NET-Q8-AnyCPU | 14.6.0 | Magick.NET-Q8-AnyCPU | 14.7.0 | upgrade | Low | Low |

#### Package Metrics
- **Packages Analyzed:** 4
- **Incompatible:** 2 (AWSSDK.Rekognition 3.3.0, EntityFramework 6.0.0)
- **Major Upgrades Required:** 2

#### Source Code Stats
- **Total LOC:** 830

#### Transformation Overview
- **Complexity:** Medium
- **Summary:** Data access layer targeting .NET Framework 4.8. Uses Entity Framework 6 and AWS SDK. Requires SDK-style conversion, framework retarget, and package upgrades. Has 110 non-upgradeable APIs.
- **Estimated Changes:** 5 (project format + framework + 2 incompatible package upgrades + API changes)
- **Blocking Issues:** None (all packages have compatible upgrades)
- **Notes:** Consider whether to keep EF6 or migrate to EF Core. Depends on Bookstore.Domain.

#### Transformation Risks
- EntityFramework 6 to 6.3.0 may require DbContext/configuration changes
- 110 non-upgradeable APIs represent significant code-level changes
- AWS SDK major version jump (3.3.0 to 3.3.100) may have breaking API changes
- Decision needed: stay on EF6 or migrate to EF Core

---

### Bookstore.Web

#### Package Compatibility Table

| Package | Current Version | Target Package(s) | Target Version | Strategy | Complexity | Priority |
|---------|----------------|-------------------|----------------|----------|------------|----------|
| Antlr | 3.5.0.2 | *(no recommendation)* | - | rewrite | Critical | Critical |
| Autofac | 8.2.1 | Autofac | 8.3.0 | upgrade | Low | Low |
| Autofac.Mvc5 | 6.1.0 | *(no recommendation)* | - | rewrite | Critical | Critical |
| Autofac.Owin | 7.1.0 | *(no recommendation)* | - | rewrite | Critical | Critical |
| AWS.Logger.Core | 3.3.3 | AWS.Logger.Core | 4.0.0 | upgrade | Medium | Medium |
| AWS.Logger.NLog | 3.3.4 | AWS.Logger.NLog | 4.0.0 | upgrade | Medium | Medium |
| AWSSDK.CloudWatchLogs | 3.7.410.17 | AWSSDK.CloudWatchLogs | 3.7.410.18 | upgrade | Low | Low |
| AWSSDK.Core | 3.7.402.35 | AWSSDK.Core | 3.7.402.36 | upgrade | Low | Low |
| AWSSDK.Rekognition | 3.7.400.129 | AWSSDK.Rekognition | 3.7.400.130 | upgrade | Low | Low |
| AWSSDK.S3 | 3.7.416.5 | AWSSDK.S3 | 3.7.416.13 | upgrade | Low | Low |
| AWSSDK.SimpleSystemsManagement | 3.7.404.10 | AWSSDK.SimpleSystemsManagement | 3.7.404.11 | upgrade | Low | Low |
| EntityFramework | 6.5.1 | EntityFramework | 6.5.1 | upgrade | Low | Low |
| jQuery | 3.7.1 | jQuery | 3.7.1 | upgrade | Low | Low |
| jQuery.Validation | 1.21.0 | *(no recommendation)* | - | rewrite | Critical | Critical |
| Microsoft.AspNet.Mvc | 5.3.0 | *(no recommendation)* | - | rewrite | Critical | Critical |
| Microsoft.AspNet.Razor | 3.3.0 | *(no recommendation)* | - | rewrite | Critical | Critical |
| Microsoft.AspNet.Web.Optimization | 1.1.3 | *(no recommendation)* | - | rewrite | Critical | Critical |
| Microsoft.AspNet.WebPages | 3.3.0 | *(no recommendation)* | - | rewrite | Critical | Critical |
| Microsoft.Bcl.AsyncInterfaces | 9.0.3 | Microsoft.Bcl.AsyncInterfaces | 9.0.4 | upgrade | Low | Low |
| Microsoft.CodeDom.Providers.DotNetCompilerPlatform | 4.1.0 | *(no recommendation)* | - | rewrite | Critical | Critical |
| Microsoft.Owin | 4.2.2 | Microsoft.AspNetCore.Owin | 6.0.29 | replace | High | High |
| Microsoft.Owin.Host.SystemWeb | 4.2.2 | *(no recommendation)* | - | rewrite | Critical | Critical |
| Microsoft.Owin.Security | 4.2.2 | *(no recommendation)* | - | rewrite | Critical | Critical |
| Microsoft.Owin.Security.Cookies | 4.2.2 | *(no recommendation)* | - | rewrite | Critical | Critical |
| Microsoft.Owin.Security.OpenIdConnect | 4.2.2 | Microsoft.AspNetCore.Authentication.OpenIdConnect | 6.0.11 | replace | High | High |
| Microsoft.Web.Infrastructure | 2.0.1 | *(no recommendation)* | - | rewrite | Critical | Critical |
| Newtonsoft.Json | 13.0.3 | Newtonsoft.Json | 13.0.4 | upgrade | Low | Low |
| NLog | 5.4.0 | NLog | 5.5.0 | upgrade | Low | Low |
| Owin | 1.0.0 | Microsoft.AspNetCore.Diagnostics | * | replace | High | High |
| System.Diagnostics.DiagnosticSource | 9.0.3 | *(no recommendation)* | - | rewrite | Critical | Critical |
| WebGrease | 1.6.0 | *(no recommendation)* | - | rewrite | Critical | Critical |

*Note: 41 additional compatible packages (minor upgrades or keep) omitted for brevity. See transformation-plan.md for full list.*

#### Package Metrics
- **Packages Analyzed:** 54
- **Incompatible:** 15
- **Major Upgrades Required:** 2 (AWS.Logger.Core 3.x->4.x, AWS.Logger.NLog 3.x->4.x)

#### Source Code Stats
- **Total LOC:** 4,302

#### Transformation Overview
- **Complexity:** Critical
- **Summary:** Full ASP.NET MVC 5 to ASP.NET Core MVC migration. Requires complete replacement of the OWIN pipeline, authentication system, bundling/optimization, DI framework integration, and routing. 42 Razor views and 10 controllers need migration.
- **Estimated Changes:** 84
- **Blocking Issues:** 9 incompatible packages with no direct upgrade path (require architectural replacement)
- **Notes:** This is the highest-effort project. Depends on all other projects being migrated first.

#### Transformation Risks
- **ASP.NET MVC 5 to ASP.NET Core MVC:** Fundamental architecture change; controllers, views, routing, filters all need updates
- **OWIN to ASP.NET Core Middleware:** Complete pipeline replacement required
- **Authentication:** OpenID Connect and Cookie auth must be re-implemented with ASP.NET Core Authentication
- **Autofac.Mvc5:** Must migrate to Autofac.Extensions.DependencyInjection or built-in DI
- **Bundling:** No direct equivalent to System.Web.Optimization - need alternative approach
- **118 non-upgradeable APIs:** Significant code-level changes needed
- **Build Errors:** 400+ build errors in preassessment (missing ASP.NET references)
- **View Engine:** 42 Razor views need Tag Helper migration

---

## Cross-Project Dependencies and Transformation Ordering

### Dependency Graph
- Bookstore.Common (leaf - no dependencies)
  - Bookstore.Cdk (depends on Common)
- Bookstore.Domain (leaf - no dependencies)
  - Bookstore.Data (depends on Domain)
    - Bookstore.Web (depends on Common, Domain, Data)

### Recommended Transformation Order

| Phase | Project | Rationale |
|-------|---------|-----------|
| 1 | Bookstore.Common | Leaf dependency, trivial migration |
| 2 | Bookstore.Domain | Leaf dependency, no packages, clean domain model |
| 3 | Bookstore.Cdk | Depends on Common only, already on net6.0 |
| 4 | Bookstore.Data | Depends on Domain, needs EF/SDK upgrades |
| 5 | Bookstore.Web | Depends on all others, most complex migration |

---

## Key Findings

1. **Critical Path:** Bookstore.Web is the bottleneck - it contains 60% of the codebase and requires a full ASP.NET MVC 5 to Core architectural migration.
2. **EF6 Decision Required:** Both Bookstore.Data and Bookstore.Web use Entity Framework 6. A decision is needed whether to keep EF6 (supported on .NET Core) or migrate to EF Core.
3. **Authentication Rewrite:** The OWIN-based OpenID Connect authentication must be completely rewritten for ASP.NET Core.
4. **Low-Risk Projects:** Bookstore.Common (already netstandard2.0) and Bookstore.Cdk (already net6.0) are low-risk, quick wins.
5. **Package Consolidation Opportunity:** AWS SDK versions differ between Bookstore.Data (3.3.0) and Bookstore.Web (3.7.x) - should be unified during migration.

---

## External Dependencies

| Dependency | Type | Impact |
|------------|------|--------|
| AWS CDK | Infrastructure-as-Code | Low - CDK packages are compatible |
| AWS SDK (S3, Rekognition, SSM, CloudWatch) | Cloud Services | Low - all support modern .NET |
| Entity Framework 6 | ORM | Medium - works on .NET Core but consider EF Core |
| NLog + AWS Logger | Logging | Low - compatible with modern .NET |
| OpenID Connect (Cognito/AD) | Authentication | High - requires full rewrite for ASP.NET Core |
| Magick.NET | Image Processing | Low - cross-platform compatible |
| jQuery/Modernizr | Frontend | Low - client-side, no .NET impact |

---

## Actionable Next Steps

1. **Phase 1 - Quick Wins (Bookstore.Common + Bookstore.Domain):**
   - Retarget Bookstore.Common to net10.0 (or keep netstandard2.0)
   - Convert Bookstore.Domain to SDK-style project and retarget to net10.0
   - Address 23 non-upgradeable APIs in Bookstore.Domain

2. **Phase 2 - Infrastructure (Bookstore.Cdk):**
   - Retarget from net6.0 to net10.0
   - Upgrade Amazon.CDK.Lib, Cdklabs.CdkNag, Constructs to latest versions
   - Verify Amazon.Jsii.Analyzers compatibility or remove

3. **Phase 3 - Data Layer (Bookstore.Data):**
   - Convert to SDK-style project and retarget to net10.0
   - Upgrade EntityFramework 6.0.0 to 6.3.0+ (or begin EF Core migration)
   - Upgrade AWSSDK.Rekognition and AWSSDK.S3 to latest versions
   - Address 110 non-upgradeable APIs

4. **Phase 4 - Web Application (Bookstore.Web):**
   - Create new ASP.NET Core MVC project structure (Program.cs, appsettings.json)
   - Replace OWIN pipeline with ASP.NET Core middleware
   - Migrate 10 controllers to ASP.NET Core MVC patterns
   - Migrate 42 Razor views to ASP.NET Core conventions (Tag Helpers)
   - Replace authentication: OWIN Security to ASP.NET Core Authentication
   - Replace Autofac.Mvc5 with Autofac.Extensions.DependencyInjection (or built-in DI)
   - Replace bundling: System.Web.Optimization to WebOptimizer or LibMan
   - Remove legacy packages (WebGrease, Antlr, Microsoft.Web.Infrastructure)
   - Address 118 non-upgradeable APIs and 400+ build errors

5. **Decision Points:**
   - EF6 vs EF Core: Determine if data access layer should migrate to EF Core
   - DI Framework: Keep Autofac or switch to built-in ASP.NET Core DI
   - Bundling Strategy: Choose between WebOptimizer, LibMan, or build-time tools
   - Authentication Provider: Confirm identity provider (Cognito, Azure AD, etc.)
