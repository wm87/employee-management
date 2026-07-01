# Fix: Upgrade auf MySql.EntityFrameworkCore v10 + Tests & Build‑Fixes

## Kurzbeschreibung
Dieser PR stellt den Build für .NET 10 wieder her, ersetzt inkompatible MySQL‑Provider‑Abhängigkeiten und fügt ein kleines xUnit‑Testprojekt hinzu.

## Änderungen
- MySql.EntityFrameworkCore v10.0.7 statt Pomelo.EntityFrameworkCore.MySql
- App.xaml.cs angepasst: UseMySQL(connStr, opts => opts.EnableRetryOnFailure())
- Entfernen Pomelo‑spezifischer using‑Direktiven
- Hinzufügen WpfApp.Tests (xUnit) mit einem Smoke‑Test
- Diverse Build‑Fixes und NuGet‑Anpassungen

## Tests & Ergebnisse
- `dotnet restore && dotnet build Project.sln` — erfolgreich
- `dotnet test Project.sln` — 1 Test ausgeführt, alle bestanden
- Test‑Report: `WpfApp.Tests/TestResults/project_test_results.trx`

## Bekannte Warnungen / Hinweise
- Frühere NU1608‑Warnung (Pomelo 9.x vs. EF.Relational 10.x) wurde durch Provider‑Wechsel adressiert.
- Remote‑Repository wurde verschoben; `origin` aktualisiert auf: https://github.com/wm87/employee-management.git

## Commits (Auszug)
- 5c7ffae Fix: Use MySql.EntityFrameworkCore v10 API (UseMySQL), update App.xaml.cs
- 5652ecb Upgrade: Add Pomelo MySQL provider, fix App.xaml.cs using; add unit test project

## Review‑Checklist
- App.xaml.cs: DB‑Konfiguration, Retry‑Strategie prüfen
- NuGet: EF‑Paketkompatibilität verifizieren
- CI: Pipeline Build + Tests ausführen
- Optional: Lokalen DB‑Verbindungstest gegen MySQL‑Instanz

## Merge‑Vorschlag
Nach Review in Zielbranch mergen; CI vor Merge verifizieren.
