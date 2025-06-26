using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Data;
using WpfApp.Model;
using WpfApp.Model.Data;
using WpfApp.Services;
using WpfApp.Services.Elasticsearch;

namespace WpfApp
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly JsonManager jsonManager = new();
        private readonly XmlManager xmlManager = new();
        private DbManager? dbManager;
        private readonly ElasticsearchService elasticsearchService = new();
        private readonly ILogger<MainViewModel> _logger;
        private readonly ReindexService _reindexService;

        private CancellationTokenSource? _statusCts;
        private CancellationTokenSource? _updateCts;
        private CancellationTokenSource? _filterCts;
        private CancellationTokenSource? _reindexCts;

        private const int PageSize = 1000;
        private int _currentPage = 1;
        private int _totalCount = 0;

        [ObservableProperty] private string? server = "127.0.0.1";
        [ObservableProperty] private string? database = "personen_db";
        [ObservableProperty] private string? username = "test_user";
        [ObservableProperty] private string? password = "sicheres_passwort";

        [ObservableProperty] private string vorname = string.Empty;
        [ObservableProperty] private string nachname = string.Empty;
        [ObservableProperty] private DateTime geburtsdatum = DateTime.Today;
        [ObservableProperty] private Gender geschlecht = Gender.m;
        [ObservableProperty] private Department abteilung = Department.GB1;

        [ObservableProperty] private BulkObservableCollection<Person> personen = new();
        [ObservableProperty] private Person? selectedPerson;

        [ObservableProperty] private string filterText = string.Empty;
        [ObservableProperty] private string? statusText;

        [ObservableProperty] private int totalPages = 1;
        [ObservableProperty] private bool canGoNext = false;
        [ObservableProperty] private bool canGoPrevious = false;

        public ICollectionView PersonenView { get; }

        public enum Datenquelle { MySQL, JSON, XML }

        private Datenquelle _ausgewaehlteDatenquelle;
        public Datenquelle AusgewaehlteDatenquelle
        {
            get => _ausgewaehlteDatenquelle;
            set
            {
                if (SetProperty(ref _ausgewaehlteDatenquelle, value))
                {
                    VerbindungseinstellungenVisibility = value == Datenquelle.MySQL ? Visibility.Visible : Visibility.Collapsed;
                    VerbindenCommand.NotifyCanExecuteChanged();
                    LoeschenCommand.NotifyCanExecuteChanged();

                    if (value == Datenquelle.JSON || value == Datenquelle.XML)
                        _ = LadenAsync();
                    else
                        Personen.Clear();

                    PersonenView.Refresh();
                }
            }
        }

        private Visibility _verbindungseinstellungenVisibility = Visibility.Visible;
        public Visibility VerbindungseinstellungenVisibility
        {
            get => _verbindungseinstellungenVisibility;
            set => SetProperty(ref _verbindungseinstellungenVisibility, value);
        }

        private Task ShowShortStatus(string msg) => ShowStatus(msg, 3000);


        public MainViewModel()
        {
            if (App.LoggerFactory == null)
                throw new InvalidOperationException("LoggerFactory is not initialized.");

            _logger = App.LoggerFactory.CreateLogger<MainViewModel>();
            _logger.LogInformation("MainViewModel gestartet");

            // 👇 Hier initialisierst du die Collection mit Beobachtungs-Callback
            Personen = new BulkObservableCollection<Person>(BeobachtePerson);

            PersonenView = CollectionViewSource.GetDefaultView(Personen);
            PersonenView.Filter = obj =>
                AusgewaehlteDatenquelle switch
                {
                    Datenquelle.MySQL => true,
                    _ => FilterPersonen(obj)
                };

            AusgewaehlteDatenquelle = Datenquelle.MySQL;

            _reindexService = new ReindexService(elasticsearchService, () => dbManager!, ShowShortStatus, App.LoggerFactory.CreateLogger<ReindexService>());
        }

        partial void OnFilterTextChanged(string value)
        {
            _filterCts?.Cancel();
            _filterCts = new CancellationTokenSource();
            var token = _filterCts.Token;

            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(10, token);
                    if (!token.IsCancellationRequested)
                    {
                        await Application.Current.Dispatcher.InvokeAsync(async () =>
                        {
                            _currentPage = 1;
                            await LadeAktuelleSeiteAsync();
                        });
                    }
                }
                catch (TaskCanceledException) { }
            });
        }

        private bool FilterPersonen(object obj)
        {
            if (obj is not Person person) return false;
            if (string.IsNullOrWhiteSpace(FilterText)) return true;

            var filter = FilterText.ToLower();
            return person.Vorname.Contains(filter) || person.Nachname.Contains(filter);
        }

        public async Task ShowStatus(string message, int durationMs = 5000)
        {
            _statusCts?.Cancel();
            _statusCts = new CancellationTokenSource();
            var token = _statusCts.Token;

            StatusText = message;
            try
            {
                await Task.Delay(durationMs, token);
                StatusText = null;
            }
            catch (TaskCanceledException) { }
        }

        private void BeobachtePerson(Person person)
        {
            person.PropertyChanged -= Person_PropertyChanged;
            person.PropertyChanged += Person_PropertyChanged;
        }

        private void EntferneBeobachtung(Person person) =>
            person.PropertyChanged -= Person_PropertyChanged;

        [RelayCommand]
        public async Task LadenAsync()
        {
            try
            {
                if (AusgewaehlteDatenquelle == Datenquelle.MySQL)
                {
                    if (dbManager == null)
                    {
                        using var scope = App.AppHost.Services.CreateScope();
                        dbManager = scope.ServiceProvider.GetRequiredService<DbManager>();
                    }

                    _currentPage = 1;
                    await LadeAktuelleSeiteAsync();
                }
                else
                {
                    // Personen zuerst von Beobachtung abmelden (optional, falls nötig)
                    foreach (var p in Personen)
                        EntferneBeobachtung(p);

                    // Jetzt komplett ersetzen mit ReplaceAll, das automatisch BeobachtePerson aufruft
                    IEnumerable<Person> loaded = AusgewaehlteDatenquelle switch
                    {
                        Datenquelle.JSON => await jsonManager.LoadPersonsAsync(),
                        Datenquelle.XML => await xmlManager.LoadPersonsAsync(),
                        _ => Enumerable.Empty<Person>()
                    };

                    Personen.ReplaceAll(loaded);
                }

                PersonenView.Refresh();
                LoeschenCommand.NotifyCanExecuteChanged();
                await ShowStatus("Daten geladen");
            }
            catch (Exception ex)
            {
                await ShowStatus($"Fehler beim Laden: {ex.Message}", 7000);
            }
        }

        private async Task LadeAktuelleSeiteAsync()
        {
            if (AusgewaehlteDatenquelle != Datenquelle.MySQL || dbManager == null)
                return;

            try
            {
                List<int>? ids = null;
                long totalHits = 0;

                foreach (var p in Personen)
                    EntferneBeobachtung(p);


                int offset = (_currentPage - 1) * PageSize;

                if (!string.IsNullOrWhiteSpace(FilterText))
                {
                    (ids, totalHits) = await elasticsearchService.SearchPersonsAsync(
                        FilterText,
                        from: offset,
                        size: PageSize
                    );

                    _totalCount = (int)totalHits;
                    Debug.WriteLine($"Elasticsearch hat {ids.Count} IDs zurückgegeben.");

                    // ✅ IDs direkt laden – kein Paging mehr hier!
                    var result = await dbManager.LoadPersonsByIdsAsync(ids);

                    Personen.ReplaceAll(result);
                }
                else
                {
                    // Standard-MySQL Paging
                    var result = await dbManager.LoadPagedPersonsAsync(offset, PageSize);
                    _totalCount = await dbManager.CountPersonsAsync("", null);

                    Personen.ReplaceAll(result);
                }

                TotalPages = (_totalCount + PageSize - 1) / PageSize;
                CanGoNext = _currentPage < TotalPages;
                CanGoPrevious = _currentPage > 1;

                PersonenView.Refresh();
                LoeschenCommand.NotifyCanExecuteChanged();
                _ = ShowStatus($"{_currentPage} von {TotalPages} : {_totalCount}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Fehler beim Laden der Seite: {ex}");
            }
        }


        [RelayCommand(CanExecute = nameof(CanVerbinden))]
        public async Task VerbindenAsync()
        {
            if (AusgewaehlteDatenquelle == Datenquelle.MySQL)
                await LadenAsync();
        }

        private bool CanVerbinden() => AusgewaehlteDatenquelle == Datenquelle.MySQL;

        [RelayCommand]
        public async Task SpeichernAsync()
        {
            try
            {
                var neuePerson = new Person
                {
                    Vorname = Vorname,
                    Nachname = Nachname,
                    Geburtsdatum = Geburtsdatum,
                    Geschlecht = Geschlecht,
                    Abteilung = Abteilung
                };

                if (AusgewaehlteDatenquelle == Datenquelle.MySQL && dbManager == null)
                    await LadenAsync();

                switch (AusgewaehlteDatenquelle)
                {
                    case Datenquelle.MySQL:
                        await dbManager!.InsertPersonAsync(neuePerson);                        
                        await IndexierePersonAsync(neuePerson);
                        break;
                    case Datenquelle.JSON:
                        await jsonManager.InsertPersonAsync(neuePerson);
                        break;
                    case Datenquelle.XML:
                        await xmlManager.InsertPersonAsync(neuePerson);
                        break;
                }

                Personen.Add(neuePerson);
                Leeren();
                await ShowStatus("Datensatz gespeichert!");
            }
            catch (Exception ex)
            {
                await ShowStatus($"Fehler beim Speichern: {ex.Message}", 7000);
            }
        }

        private async void Person_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not Person person) return;

            _updateCts?.Cancel();
            _updateCts = new CancellationTokenSource();
            var token = _updateCts.Token;

            try
            {
                await Task.Delay(800, token);

                if (AusgewaehlteDatenquelle == Datenquelle.MySQL && dbManager == null)
                    await LadenAsync();

                switch (AusgewaehlteDatenquelle)
                {
                    case Datenquelle.MySQL:
                        await dbManager!.UpdatePersonAsync(person);
                        await IndexierePersonAsync(person);
                        break;
                    case Datenquelle.JSON:
                        await jsonManager.UpdatePersonAsync(person);
                        break;
                    case Datenquelle.XML:
                        await xmlManager.UpdatePersonAsync(person);
                        break;
                }

                await ShowStatus("Datensatz aktualisiert");
            }
            catch (TaskCanceledException) { }
            catch (Exception ex)
            {
                await ShowStatus($"Fehler beim Aktualisieren: {ex.Message}", 7000);
            }
        }

        private async Task IndexierePersonAsync(Person p)
        {
            await elasticsearchService.IndexPersonAsync(new PersonSearchIndex
            {
                Id = p.Id,
                Nachname = p.Nachname,
                Vorname = p.Vorname
            });
        }

        [RelayCommand(CanExecute = nameof(CanLoeschen))]
        public async Task LoeschenAsync()
        {
            if (SelectedPerson == null) return;

            try
            {
                if (AusgewaehlteDatenquelle == Datenquelle.MySQL && dbManager == null)
                    await LadenAsync();

                switch (AusgewaehlteDatenquelle)
                {
                    case Datenquelle.MySQL:
                        await dbManager!.DeletePersonAsync(SelectedPerson.Id);
                        break;
                    case Datenquelle.JSON:
                        await jsonManager.DeletePersonAsync(SelectedPerson.Id);
                        break;
                    case Datenquelle.XML:
                        await xmlManager.DeletePersonAsync(SelectedPerson.Id);
                        break;
                }

                Personen.Remove(SelectedPerson);
                SelectedPerson = null;
                await ShowStatus("Datensatz gelöscht");
            }
            catch (Exception ex)
            {
                await ShowStatus($"Fehler beim Löschen: {ex.Message}", 7000);
            }
        }

        private bool CanLoeschen() => SelectedPerson != null;

        partial void OnSelectedPersonChanged(Person? value)
        {
            LoeschenCommand.NotifyCanExecuteChanged();
        }

        private void Leeren()
        {
            Vorname = string.Empty;
            Nachname = string.Empty;
            Geburtsdatum = DateTime.Today;
            Geschlecht = Gender.m;
            Abteilung = Department.GB1;
        }

        [RelayCommand]
        private async Task NaechsteSeiteAsync()
        {
            if (CanGoNext)
            {
                _currentPage++;
                await LadeAktuelleSeiteAsync();
            }
        }

        [RelayCommand]
        private async Task VorherigeSeiteAsync()
        {
            if (CanGoPrevious)
            {
                _currentPage--;
                await LadeAktuelleSeiteAsync();
            }
        }

        [RelayCommand(CanExecute = nameof(CanStartReindex))]
        public async Task StartReindexAsync()
        {
            if (dbManager == null)
            {
                await ShowStatus("Keine DB-Verbindung für Reindex!");
                return;
            }

            if (_reindexCts != null)
            {
                await ShowStatus("Reindex läuft bereits!");
                return;
            }

            _reindexCts = new CancellationTokenSource();
            var token = _reindexCts.Token;

            try
            {
                await _reindexService.ReindexAsync(token);
            }
            catch (TaskCanceledException)
            {
                await ShowStatus("Reindex abgebrochen");
            }
            catch (Exception ex)
            {
                await ShowStatus($"Fehler beim Reindex: {ex.Message}", 7000);
            }
            finally
            {
                _reindexCts = null;
            }
        }

        private bool CanStartReindex() => _reindexCts == null;
    }
}