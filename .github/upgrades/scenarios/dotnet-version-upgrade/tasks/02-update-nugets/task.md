# 02-update-nugets: Update incompatible NuGet packages

Identifiziere und aktualisiere NuGet-Pakete, die nicht mit net11.0 kompatibel sind. Wenn keine kompatible Version verfügbar ist, dokumentiere das Paket als Blocker im Task.

## Scope Inventory
- Affected projects:
  - DB_ES\DB_ES.csproj
  - WpfApp\WpfApp.csproj
- Context:
  - Ziel-Framework wurde auf Nutzerwunsch auf `net10.0` zurückgesetzt.
  - Die ursprüngliche Assessment-Markierung bezog sich auf net11.0-Kompatibilitätswarnungen; mit net10.0 sind viele Pakete bereits passend.
- Action plan:
  - Prüfe alle PackageReference-Versionen in beiden Projekten gegen net10-Kompatibilität und dokumentiere erforderliche Updates.

**Done when**: Alle Pakete, die in assessment als inkompatibel markiert wurden (bezogen auf net11.0), sind entweder für net10.0 kompatibel oder als Blocker dokumentiert in progress-details.md.
