# .NET Version Upgrade Plan

## Overview

**Target**: Upgrade WpfApp solution to .NET 10.0 (LTS)
**Scope**: 2 projects (WpfApp, DB_ES), both currently targeting net8.0-windows

## Strategy

### Selected Strategy
**All-At-Once** — All projects upgraded simultaneously in a single atomic operation.

**Rationale**: Both projects are already on modern .NET (net8.0), well-structured as SDK-style projects, with a simple linear dependency (DB_ES → WpfApp). No complex tier ordering is needed. A single coordinated upgrade provides the fastest path to .NET 10.0.

## Tasks

### 01-prerequisites: Verify SDK and Environment Compatibility

Verify that the .NET 10.0 SDK is installed and properly configured. Check global.json for any version pinning that might conflict with the target framework. Validate the local development environment supports the upgrade.

**Done when**: .NET 10.0 SDK confirmed installed; global.json (if present) is compatible with net10.0; development environment ready.

---

### 02-upgrade-projects: Update Project Files and NuGet Packages

Update both projects (WpfApp.csproj and DB_ES.csproj) to target net10.0-windows. Update all NuGet packages to compatible versions as identified in the assessment:
- 10 packages require version updates (Microsoft.EntityFrameworkCore.* 9.0.10→10.0.8, Microsoft.Extensions.* 9.0.10→10.0.8, etc.)
- 1 package has breaking compatibility: Microsoft.Xaml.Behaviors.Wpf (1.1.135→1.1.39)
- 11 packages are already compatible with .NET 10.0

Restore dependencies and fix all compilation errors. This covers:
- Target framework updates across both projects
- NuGet package version bumping
- Namespace and API compatibility adjustments (particularly WPF APIs: System.ComponentModel.ICollectionView, System.Windows.Visibility, and related types)
- Behavioral changes for System.Uri and System.Xml.Serialization.XmlSerializer if used

**Done when**: Both projects build successfully with zero errors; all NuGet packages are updated to recommended versions; no compiler warnings remain.

---

### 03-validation: Build and Test Full Solution

Execute a complete solution build to verify all code changes compile cleanly. Run the test suite (if available) to ensure runtime behavior is preserved. Document any behavioral changes that require testing or validation.

**Done when**: Solution builds without errors or warnings; all tests pass (if test projects exist); no blocking issues remain.
