# 02-upgrade-projects: Update Project Files and NuGet Packages

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
