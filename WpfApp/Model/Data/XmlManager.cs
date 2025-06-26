using System.IO;
using System.Xml.Serialization;

namespace WpfApp.Model.Data
{
    public class XmlManager
    {
        private const string XmlDatei = "personen.xml";

        public async Task<List<Person>> LoadPersonsAsync()
        {
            if (!File.Exists(XmlDatei))
                return new List<Person>();

            await using var stream = new FileStream(XmlDatei, FileMode.Open);
            var serializer = new XmlSerializer(typeof(List<Person>));
            return (List<Person>)serializer.Deserialize(stream)!;
        }

        public async Task InsertPersonAsync(Person person)
        {
            var personen = await LoadPersonsAsync();
            person.Id = personen.Any() ? personen.Max(p => p.Id) + 1 : 1;
            personen.Add(person);
            await SaveAllAsync(personen);
        }

        public async Task UpdatePersonAsync(Person person)
        {
            var personen = await LoadPersonsAsync();
            var index = personen.FindIndex(p => p.Id == person.Id);
            if (index >= 0)
            {
                personen[index] = person;
                await SaveAllAsync(personen);
            }
        }

        public async Task DeletePersonAsync(int id)
        {
            var personen = await LoadPersonsAsync();
            var neu = personen.Where(p => p.Id != id).ToList();
            await SaveAllAsync(neu);
        }

        private async Task SaveAllAsync(List<Person> personen)
        {
            await using var stream = new FileStream(XmlDatei, FileMode.Create);
            var serializer = new XmlSerializer(typeof(List<Person>));
            serializer.Serialize(stream, personen);
        }
    }
}
