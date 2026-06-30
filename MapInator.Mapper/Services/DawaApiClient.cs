using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using MapInator.Shared.Models;
using Microsoft.Extensions.Logging;

namespace MapInator.Mapper.Services;

public class DawaApiClient(HttpClient http, ILogger<DawaApiClient> logger)
{
    private const int PageSize = 1000;
    private const int DelayBetweenPagesMs = 200;
    private const int MaxRetries = 3;

    public async Task<List<StednavnHovedtype>> GetTypesAsync(CancellationToken ct = default)
    {
        return await RetryAsync(async () =>
        {
            var result = await http.GetFromJsonAsync<List<StednavnHovedtype>>(
                "stednavntyper", ct);
            return result ?? [];
        }, "stednavntyper");
    }

    public async IAsyncEnumerable<Stednavn> GetAllForTypeAsync(
        string hovedtype,
        string undertype,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        int page = 1;
        while (true)
        {
            ct.ThrowIfCancellationRequested();
            var url = $"stednavne?per_side={PageSize}&side={page}&hovedtype={Uri.EscapeDataString(hovedtype)}&undertype={Uri.EscapeDataString(undertype)}";
            var batch = await RetryAsync(
                () => http.GetFromJsonAsync<List<Stednavn>>(url, ct)!,
                url);

            if (batch is null || batch.Count == 0)
                yield break;

            foreach (var item in batch)
                yield return item;

            logger.LogDebug("Fetched page {Page} for {Type}/{Sub} ({Count} records)",
                page, hovedtype, undertype, batch.Count);

            if (batch.Count < PageSize)
                yield break;

            page++;
            await Task.Delay(DelayBetweenPagesMs, ct);
        }
    }

    private async Task<T> RetryAsync<T>(Func<Task<T>> action, string context)
    {
        var delays = new[] { 1000, 2000, 4000 };
        for (int attempt = 0; attempt < MaxRetries; attempt++)
        {
            try
            {
                return await action();
            }
            catch (Exception ex) when (attempt < MaxRetries - 1)
            {
                logger.LogWarning("Attempt {A} failed for {Context}: {Msg}. Retrying in {D}ms…",
                    attempt + 1, context, ex.Message, delays[attempt]);
                await Task.Delay(delays[attempt]);
            }
        }
        return await action();
    }
}
