using CommunityToolkit.Mvvm.ComponentModel;
using System.Xml.Serialization;

namespace WpfApp.Model
{
    public enum Gender { m, w, d }
    public enum Department { GB1, GB2, GB3, GB4, GB5, GB6, StB, D }

    [Serializable]
    public partial class Person : ObservableObject
    {
        [ObservableProperty]
        [XmlElement]
        private int id;

        [ObservableProperty]
        [XmlElement]
        private string vorname = string.Empty;

        [ObservableProperty]
        [XmlElement]
        private string nachname = string.Empty;

        [ObservableProperty]
        [XmlElement]
        private DateTime geburtsdatum = DateTime.Today;

        [ObservableProperty]
        [XmlElement]
        private Gender geschlecht;

        [ObservableProperty]
        [XmlElement]
        private Department abteilung;

        // Parameterloser Konstruktor erforderlich für XmlSerializer
        public Person() { }
    }
}
