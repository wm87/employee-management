# 🧑‍💼 Mitarbeiterverwaltung

Die **Mitarbeiterverwaltung** ist eine moderne WPF-Anwendung zur Verwaltung und Visualisierung von Personendaten. Sie nutzt aktuelle Technologien wie **MVVM**, **C#**, **MySQL**, **Elasticsearch (ES)** und **Docker**. Die Anwendung unterstützt verschiedene Datenquellen wie **MySQL**, **JSON** und **XML**, und bietet umfassende Funktionen zur Filterung, Bearbeitung und Löschung von Daten.

Die Verwaltung der MySQL-Datenbank erfolgt dabei über **Entity Framework (EF Core)**, sodass die Datenbankstruktur durch das Framework automatisch erstellt und verwaltet wird.

## 🚀 Technologien

### 🔧 MVVM (Model-View-ViewModel)
Architekturpattern zur Trennung von UI (View), Logik (ViewModel) und Datenmodell (Model). Erleichtert Wartung, Testbarkeit und Erweiterbarkeit.

### 💻 C#
Hauptprogrammiersprache der Anwendung. Wird zur Entwicklung der WPF-Anwendung verwendet.

### 🐬 MySQL
Relationale Datenbank zur strukturierten Speicherung der Personendaten.  
Die Datenbank wird über **Entity Framework Core** verwaltet. Das Framework übernimmt die Migrationen und das Schema-Management.

### 🟣 Entity Framework Core
Object-Relational Mapper (ORM) für .NET.  
Ermöglicht die Modellierung der Datenbankstruktur im Code und die automatische Verwaltung von Migrationen, sodass keine manuelle Pflege des Datenbankschemas notwendig ist.

### 🔍 Elasticsearch
Suchmaschine zur performanten Filterung und schnellen Indizierung der Daten. Eingesetzt für die Live-Suche in der Anwendung.

### 🐳 Docker
Containerisierung der Infrastrukturkomponenten (MySQL, Elasticsearch) mittels `docker-compose`.


## 📦 Anforderungen

- .NET Desktop Runtime (.NET 8 empfohlen)
- Docker
- Visual Studio (mit WPF-Unterstützung)

## 📊 Funktionen der Anwendung
* DataGrid zur Anzeige von Personendaten

* Bearbeiten und Löschen direkt im Datagrid

* Datenquellenumschaltung über Radiobuttons:
    - MySQL
    - JSON
    - XML

* Elasticsearch-basierte Filterung (z. B. nach Name, Vorname)

* 🔄 Elasticsearch Reindex Verwendung

    - Bei Neuimport von Daten
    - Aktualisiert den Elasticsearch-Index mit allen Personen

* Paging (standardmäßig 1.000 Datensätze pro Seite)


## ▶️ Projektstart

### 🐳 Docker Setup

Die Anwendung nutzt `docker-compose`, um **Elasticsearch** und **MySQL** bereitzustellen.

### docker-compose.yaml (Ausschnitt)

```yaml
services:
  elasticsearch:
    image: docker.elastic.co/elasticsearch/elasticsearch:9.0.2
    ...
    ports:
      - "9200:9200"
    volumes:
      - esdata:/usr/share/elasticsearch/data
    ...

  mysql:
    image: mysql:latest
    ...
    ports:
      - "3306:3306"
    volumes:
      - mysqldata:/var/lib/mysql
    ...
```

### 🐳 Docker-Container starten

```bash
cd DB_ES
docker-compose up -d
```

### 🐬 MySQL-Daten importieren

im Projekt-Ordner ***DB_ES*** kann die **Program.cs** zum Import von 1 Mio. Dummy-Personen mit folgenden Attributen genutzt werden:

- Nachname
- Vorname
- Geschlecht
- Abteilung
- Geburtsdatum

Die Anzahl der Dummy-Datensätze ist frei wählbar!
Die MySQL-Datenbank wird dabei durch Entity Framework Core aufgebaut und verwaltet.

### 💻 WpfApp starten

im Projekt-Ordner ***WpfApp*** kann die WpfApp für die Mitarbeiterverwaltung gestartet werden.

