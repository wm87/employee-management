using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Core.Bulk;
using Elastic.Clients.Elasticsearch.Mapping;
using Elastic.Transport;
using System.Diagnostics;
using WpfApp.Services.Elasticsearch;
using Properties = Elastic.Clients.Elasticsearch.Mapping.Properties;

namespace WpfApp.Services
{
    public class ElasticsearchService
    {
        private readonly ElasticsearchClient _client;
        private const string IndexName = "personen_index";

        public long TotalHits { get; set; }

        public ElasticsearchService()
        {
            var settings = new ElasticsearchClientSettings(new Uri("http://localhost:9200"))
                .Authentication(new BasicAuthentication("elastic", "deinPasswort"))
                .ServerCertificateValidationCallback((o, cert, chain, errors) => true)
                .DefaultIndex(IndexName)
                .EnableHttpCompression(); // Aktiviert gzip → spart Bandbreite

            _client = new ElasticsearchClient(settings);
            _ = EnsureIndexAsync(); // Index ggf. anlegen
        }

        // Erstellt Index mit optionaler Optimierung (z. B. für schnelles Reindexing)
        public async Task EnsureIndexAsync(int numberOfReplicas = 1, string refreshInterval = "1s")
        {
            var existsResponse = await _client.Indices.ExistsAsync(IndexName);

            if (!existsResponse.Exists)
            {
                var createResponse = await _client.Indices.CreateAsync(IndexName, c => c
                    .Settings(s => s
                        .MaxResultWindow(20000)
                        .NumberOfReplicas(numberOfReplicas)
                        .RefreshInterval(refreshInterval)
                        .NumberOfShards(1) // Bei Bedarf anpassen für Cluster-Setup
                    )
                    .Mappings(m => m
                        .Properties(new Properties
                        {
                            { "id", new IntegerNumberProperty() },
                            { "vorname", new TextProperty() },
                            { "nachname", new TextProperty() }
                        })
                    )
                );

                if (!createResponse.IsValidResponse)
                {
                    throw new Exception($"Fehler beim Erstellen des Index: {createResponse.ElasticsearchServerError?.Error?.Reason}");
                }


            }
        }

        public async Task UpdateIndexSettingsAsync(int numberOfReplicas, string refreshInterval)
        {
            var response = await _client.Transport.RequestAsync<StringResponse>(
                HttpMethod.PUT,
                $"/{IndexName}/_settings",
                PostData.Serializable(new
                {
                    index = new
                    {
                        number_of_replicas = numberOfReplicas,
                        refresh_interval = refreshInterval
                    }
                }),
                cancellationToken: default
            );

            // Fix: StringResponse does not have HttpStatusCode property. Use ApiCallDetails.HttpStatusCode instead.
            if (response.ApiCallDetails.HttpStatusCode != 200)
            {
                throw new Exception($"Fehler beim Aktualisieren der Index-Einstellungen: {response.Body}");
            }
        }

        public async Task IndexPersonAsync(PersonSearchIndex person)
        {
            var response = await _client.IndexAsync(person, i => i.Index(IndexName).Id(person.Id));
            if (!response.IsValidResponse)
            {
                Debug.WriteLine($"Fehler beim Indexieren: {response.ElasticsearchServerError?.Error?.Reason}");
            }
        }

        public async Task<(List<int> Ids, long TotalHits)> SearchPersonsAsync(string query, int from = 0, int size = 10)
        {
            var response = await _client.SearchAsync<PersonSearchIndex>(s => s
                .Indices(IndexName)
                .From(from)
                .Size(size)
                .Query(q => q
                    .QueryString(qs => qs
                        .Fields(new[] { "vorname", "nachname" })
                        .Query($"{query}*")
                    )
                )
            );

            if (!response.IsValidResponse)
            {
                Debug.WriteLine("Elasticsearch-Suchfehler: " + response.ElasticsearchServerError?.Error?.Reason);
                return ([], 0);
            }

            var ids = response.Hits
                .Where(h => h.Source is not null)
                .Select(h => h.Source!.Id)
                .ToList();

            return (ids, response.Total);
        }

        public async Task<BulkResponse> BulkIndexAsync(List<PersonSearchIndex> persons, CancellationToken token)
        {
            BulkRequest bulkRequest = new(IndexName)
            {
                Operations = persons.Select(p => new BulkIndexOperation<PersonSearchIndex>(p)
                {
                    Id = p.Id.ToString()
                }).Cast<IBulkOperation>().ToList()
            };

            var response = await _client.BulkAsync(bulkRequest, token);

            if (!response.IsValidResponse)
            {
                Debug.WriteLine($"Bulk-Fehler: {response.ElasticsearchServerError?.Error?.Reason}");
            }

            if (response.Errors)
            {
                foreach (var item in response.ItemsWithErrors)
                {
                    Debug.WriteLine($"❌ Fehler beim Indexieren von ID {item.Id}: {item.Error?.Reason}");
                }
            }

            return response;
        }

        public async Task DeleteIndexIfExistsAsync()
        {
            var exists = await _client.Indices.ExistsAsync(IndexName);
            if (exists.Exists)
            {
                var deleteResponse = await _client.Indices.DeleteAsync(IndexName);
                if (!deleteResponse.IsValidResponse)
                {
                    throw new Exception($"Fehler beim Löschen des Index: {deleteResponse.ElasticsearchServerError?.Error?.Reason}");
                }
            }
        }

        public async Task RefreshIndexAsync()
        {
            await _client.Indices.RefreshAsync(IndexName);
        }
    }
}
