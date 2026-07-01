# Progress Details - 03-build-and-fix

## Summary
- Ziel: Lösung bauen und verbleibende Kompatibilitätsprobleme lösen.
- Aktion: Pomelo entfernt, Devart.Data.MySql.EFCore (10.1.123) in Projektdateien eingefügt; Restore & Build ausgeführt.

## Ergebnis
- Build schlägt fehl mit folgenden Problemen:
  - MC1000 / MSB4018: Zugriff verweigert auf Dateien in obj/ (z. B. WpfApp_MarkupCompile.cache / App.g.cs). Ursache: Datei gesperrt oder Berechtigungsproblem auf dem System. Empfehlung: Visual Studio / Prozesse schließen und Build wiederholen.
  - NU1608 Warnungen: Devart.Data.MySql.EFCore 10.1.123 verlangt Microsoft.EntityFrameworkCore.Relational (>=6.x & <7.0), aber Projekte verwenden Microsoft.EntityFrameworkCore.* 10.0.x. Devart ist somit nicht kompatibel mit EF Core 10 in diesem Projekt.

## Diagnose
- Devart unterstützt ältere EF Core Versionen (<=6.x/7.x), keine Unterstützung für EF Core 10 gefunden.
- Pomelo hatte keine 10.x-Version; ein kompatibler, freien Provider für EF Core 10 konnte nicht gefunden.

## Nächste Schritte (Optionen)
1. "downgrade-ef": Microsoft.EntityFrameworkCore.* auf 9.x zurücksetzen, beide Projekte anpassen, dann Build prüfen (behält Pomelo/Devart Kompatibilität).
2. "search-alternatives": Weiter nach anderen (ggf. kommerziellen) Providern suchen, die EF Core 10 unterstützen.
3. "retry-build": Nach Schließen von Visual Studio erneut versuchen (behebt Access-Denied, löst aber nicht die Provider-Versionskonflikte).

Bitte antworte mit einer Option: "downgrade-ef", "search-alternatives" oder "retry-build".


(End of progress details)
