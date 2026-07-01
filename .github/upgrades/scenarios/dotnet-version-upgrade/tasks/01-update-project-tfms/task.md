# 01-update-project-tfms: Update project TargetFramework elements

Ändere die <TargetFramework> / <TargetFrameworks> Einträge in den Projektdateien auf `net11.0`.

## Scope Inventory
- Affected projects:
  - DB_ES\DB_ES.csproj
  - WpfApp\WpfApp.csproj
- Key concerns:
  - Both projects target `net10.0-windows` and require updating to `net11.0-windows`.
  - Some packages may be incompatible after retargeting; package updates are handled in task 02.

**Done when**: Beide Projektdateien (DB_ES and WpfApp) zeigen `net11.0` im TargetFramework und die Projekte kompiliert sich erfolgreich (siehe Task 03).
