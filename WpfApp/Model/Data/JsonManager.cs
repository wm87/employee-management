using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WpfApp.Model.Data
{
    public class JsonManager
    {
        readonly string jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "personen.json");

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };

        public async Task<List<Person>> LoadPersonsAsync()
        {
            if (!File.Exists(jsonPath))
                return [];

            using FileStream stream = File.OpenRead(jsonPath);
            var personenListe = await JsonSerializer.DeserializeAsync<List<Person>>(stream, JsonOptions);

            return personenListe ?? [];
        }

        public async Task InsertPersonAsync(Person neuePerson)
        {
            List<Person> personenListe = [];

            if (File.Exists(jsonPath))
            {
                using FileStream readStream = File.OpenRead(jsonPath);
                var geladene = await JsonSerializer.DeserializeAsync<List<Person>>(readStream, JsonOptions);
                if (geladene != null)
                    personenListe = geladene;
            }

            // 🔹 ID setzen basierend auf der höchsten existierenden ID
            neuePerson.Id = personenListe.Count > 0 ? personenListe.Max(p => p.Id) + 1 : 1;

            // 🔹 Neue Person hinzufügen
            personenListe.Add(neuePerson);

            // 🔹 Neue Liste speichern
            using FileStream writeStream = File.Create(jsonPath);
            await JsonSerializer.SerializeAsync(writeStream, personenListe, JsonOptions);
        }

        public async Task UpdatePersonAsync(Person updatedPerson)
        {
            if (!File.Exists(jsonPath))
                return;

            // Bestehende Personen laden
            string json = await File.ReadAllTextAsync(jsonPath);
            var personenListe = JsonSerializer.Deserialize<List<Person>>(json, JsonOptions) ?? [];

            // Die Person anhand der ID finden
            var index = personenListe.FindIndex(p => p.Id == updatedPerson.Id);
            if (index == -1)
                return; // Person nicht gefunden

            // Person ersetzen
            personenListe[index] = updatedPerson;

            // Datei aktualisieren
            using FileStream stream = File.Create(jsonPath);
            await JsonSerializer.SerializeAsync(stream, personenListe, JsonOptions);
        }
        public async Task DeletePersonAsync(int id)
        {
            if (!File.Exists(jsonPath))
                return;

            // JSON-Datei lesen und deserialisieren
            string json = await File.ReadAllTextAsync(jsonPath);
            var personenListe = JsonSerializer.Deserialize<List<Person>>(json, JsonOptions) ?? [];

            // Person mit passender ID suchen und entfernen
            var personToDelete = personenListe.FirstOrDefault(p => p.Id == id);
            if (personToDelete is null)
                return; // Nichts zu löschen

            personenListe.Remove(personToDelete);

            // JSON-Datei überschreiben
            using FileStream stream = File.Create(jsonPath);
            await JsonSerializer.SerializeAsync(stream, personenListe, JsonOptions);
        }

    }
}
