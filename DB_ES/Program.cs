using System;
using System.Collections.Generic;
using System.Linq;
using WpfApp.Model;

namespace DB_ES
{
    class Program
    {
        static void Main()
        {
            var random = new Random();
            var personList = new List<Person>();

            int cnt = 1000_000; // Anzahl der zu erstellenden Personen

            using var db = new AppDbContext();
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();

            Console.WriteLine("Datenbank wurde neu angelegt.");

            for (int i = 0; i < cnt; i++)
            {
                personList.Add(new Person
                {
                    Vorname = RandomString(random, 10),
                    Nachname = RandomString(random, 10)
                });
            }

            db.Personen.AddRange(personList);
            db.SaveChanges();

            Console.WriteLine($"{cnt} Benutzer eingefügt.");

            // Tabelleneigenschaften abrufen
            var entityType = db.Model.FindEntityType(typeof(Person));
            if (entityType != null)
            {
                foreach (var property in entityType.GetProperties())
                {
                    Console.WriteLine($"Spalte: {property.Name}, Typ: {property.ClrType}");
                }
            }
        }

        static string RandomString(Random rand, int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            return new string([.. Enumerable.Repeat(chars, length).Select(s => s[rand.Next(s.Length)])]);
        }
    }
}