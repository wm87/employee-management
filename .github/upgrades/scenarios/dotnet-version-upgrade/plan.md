# .NET Version Upgrade Plan

## Overview

**Target**: Upgrade solution to net11.0 (user confirmed, Preview)
**Scope**: 2 projects (DB_ES, WpfApp). Small solution with linear dependencies.

## Tasks

### 01-update-project-tfms: Update project TargetFramework elements

Ändere die <TargetFramework> / <TargetFrameworks> Einträge in den Projektdateien auf `net11.0`.

**Done when**: Beide Projektdateien (DB_ES and WpfApp) zeigen `net11.0` im TargetFramework und die Projekte kompiliert sich erfolgreich (siehe Task 03).

---

### 02-update-nugets: Update incompatible NuGet packages

Identifiziere und aktualisiere NuGet-Pakete, die nicht mit net11.0 kompatibel sind. Wenn keine kompatible Version verfügbar ist, dokumentiere das Paket als Blocker im Task.

**Done when**: Alle Pakete, die in assessment als inkompatibel markiert wurden, wurden entweder auf kompatible Versionen gehoben oder als Blocker in progress-details.md dokumentiert.

---

### 03-build-and-fix: Build solution and fix compilation/runtime issues

Baue die gesamte Lösung, behebe Compilerfehler und behandelte Breaking-Change-Codepfade, die in assessment identifiziert wurden (z. B. Api.0001, Api.0003).

**Done when**: Die Lösung baut fehlerfrei und ohne Warnungen (Warnungen als Fehler behandeln) in der Working-Branch.

---

### 04-run-tests: Execute automated tests and smoke tests

Führe vorhandene Unit-Tests und manuelle Smoke-Tests aus, um Verhalten sicherzustellen.

**Done when**: Alle Unit-Tests bestehen; Smoke-Tests zeigen keine regressiven Fehler.

---

### 05-finalize: Finalize changes and push

Überprüfe Änderungen, aktualisiere scenario-instructions.md decisions, committe ggf. abschließende Änderungen und erstelle PR nach origin.

**Done when**: Alle Tasks abgeschlossen und PR erstellt (oder der Benutzer entscheidet manuell).
