using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using WpfApp.Model;
using WpfApp.Model.Data;
using WpfApp.Services.Elasticsearch;

namespace WpfApp.Services;

public class ReindexService
{
    private readonly ElasticsearchService _elasticsearchService;
    private readonly Func<DbManager> _getDbManager;
    private readonly Func<string, Task> _setStatus;
    private readonly ILogger<ReindexService> _logger;

    public ReindexService(
        ElasticsearchService elasticsearchService,
        Func<DbManager> getDbManager,
        Func<string, Task> setStatus,
        ILogger<ReindexService> logger)
    {
        _elasticsearchService = elasticsearchService;
        _getDbManager = getDbManager;
        _setStatus = setStatus;
        _logger = logger;
    }

    public async Task ReindexAsync(CancellationToken token)
    {
        const int batchSize = 10000;
        const int maxDegreeOfParallelism = 6;
        int totalIndexed = 0;

        _logger.LogInformation("Beginne Reindexing...");

        // Index löschen und neu anlegen mit optimierten Einstellungen
        await _elasticsearchService.DeleteIndexIfExistsAsync();
        await _elasticsearchService.EnsureIndexAsync(
            numberOfReplicas: 0,
            refreshInterval: "-1"
        );

        var dbManager = _getDbManager();
        var queue = new BlockingCollection<List<Person>>(boundedCapacity: maxDegreeOfParallelism * 2);

        // Produzent: lädt Daten sequentiell in die BlockingCollection
        var producer = Task.Run(async () =>
        {
            int offset = 0;
            while (!token.IsCancellationRequested)
            {
                var batch = await dbManager.LoadPagedPersonsAsync(offset, batchSize, "");
                if (batch.Count == 0)
                    break;

                queue.Add(batch, token);
                offset += batch.Count;
                Interlocked.Add(ref totalIndexed, batch.Count);
            }

            queue.CompleteAdding();
        }, token);

        // Konsumenten: paralleles Indexieren der Daten
        var consumers = Enumerable.Range(0, maxDegreeOfParallelism).Select(_ => Task.Run(async () =>
        {
            foreach (var personsBatch in queue.GetConsumingEnumerable(token))
            {
                var batchIndex = personsBatch.Select(p => new PersonSearchIndex
                {
                    Id = p.Id,
                    Vorname = p.Vorname,
                    Nachname = p.Nachname
                }).ToList();

                var response = await _elasticsearchService.BulkIndexAsync(batchIndex, token);

                if (response.Errors)
                {
                    var fehler = string.Join(Environment.NewLine,
                        response.ItemsWithErrors.Select(item =>
                            $"Fehler bei ID {item.Id}: {item.Error?.Reason ?? "Unbekannter Fehler"}"));

                    _logger.LogError("Fehler beim Indexieren:\n{Fehler}", fehler);
                }

                if (!response.IsValidResponse)
                {
                    _logger.LogWarning("Warnung beim Indexieren:\n{DebugInfo}", response.DebugInformation);
                }

                _logger.LogInformation("Batch indexiert: {Count} Personen", personsBatch.Count);
                await _setStatus($"Reindex: {totalIndexed} Personen indexiert");
            }
        }, token)).ToArray();

        await producer;
        await Task.WhenAll(consumers);

        // Index aktualisieren: Refresh und Einstellungen zurücksetzen
        await _elasticsearchService.UpdateIndexSettingsAsync(
            numberOfReplicas: 1,
            refreshInterval: "1s"
        );
        await _elasticsearchService.RefreshIndexAsync();

        await _setStatus($"Reindex abgeschlossen: {totalIndexed} Personen indexiert");
        _logger.LogInformation("Reindex abgeschlossen mit {Count} Einträgen", totalIndexed);
    }
}
