using System.Text.Json;
using System.Net.Http.Headers;
using AutoBackupSeq.Models;

namespace AutoBackupSeq;

public static class SeqQuery
{
    public static async Task QueryAsync(AppConfig config, DateTime startTime, DateTime endTime)
    {
        string? afterId = null;
        int total = 0;

        using var client = new HttpClient();
        if (!string.IsNullOrWhiteSpace(config.ApiKey))
            client.DefaultRequestHeaders.Add("X-Seq-ApiKey", config.ApiKey);

        string outputDir = Path.Combine(AppContext.BaseDirectory, config.BackupDirectory);
        Directory.CreateDirectory(outputDir);

        string filter = $"Application = '{config.ApplicationName}' and @Level = 'Information' and LogType = '{config.LogType}'";
        string encodedFilter = Uri.EscapeDataString(filter);

        string fileName = $"logs_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.json";
        string outputFile = Path.Combine(outputDir, fileName);

        await using var stream = new FileStream(outputFile, FileMode.Create, FileAccess.Write);
        await using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true });

        writer.WriteStartArray();

        while (true)
        {
            var baseUrl = config.SeqUrl.TrimEnd('/');

            var url = $"{baseUrl}/api/events?count={config.PageSize}&start={startTime:o}&end={endTime:o}&filter={encodedFilter}" +
                      (afterId != null ? $"&afterId={afterId}" : "");

            var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"❌ Seq query failed. Status: {response.StatusCode}");
                break;
            }

            var json = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<List<JsonElement>>(json);
            if (data == null || data.Count == 0) break;

            foreach (var item in data)
                JsonSerializer.Serialize(writer, item);

            total += data.Count;

            afterId = data.LastOrDefault().GetProperty("Id").GetString();

            if (data.Count < config.PageSize) break;
        }

        writer.WriteEndArray();
        await writer.FlushAsync();

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\n✅ Total {total} events saved to: {outputFile}");
        Console.ResetColor();
    }
}
