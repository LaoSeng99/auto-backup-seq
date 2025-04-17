using AutoBackupSeq.Models;
using System.Diagnostics.Metrics;
using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace AutoBackupSeq;
public static class SchedulerService
{
    public static async Task StartAsync(AppConfig config, CancellationToken cancellationToken)
    {
        if (!config.Scheduler.Enabled)
        {
            Console.WriteLine("⚠️ Scheduler is disabled in config.json");
            return;
        }

        while (!cancellationToken.IsCancellationRequested)
        {
            var now = DateTime.UtcNow;
            var start = now.AddMinutes(config.Scheduler.QueryStartOffsetMinutes);
            var end = now.AddMinutes(config.Scheduler.QueryEndOffsetMinutes);

            Console.WriteLine($"\n⏳ Scheduled run: {start:yyyy-MM-dd HH:mm} → {end:HH:mm}");
            await SeqQuery.QueryAsync(config, start, end);
            if (config.Webhook.SendFilteredLogs)
            {
                try
                {
                    var dir = Path.Combine(AppContext.BaseDirectory, config.BackupDirectory);
                    var latestFile = Directory.GetFiles(dir, "*.json")
                        .Select(f => new FileInfo(f))
                        .OrderByDescending(f => f.LastWriteTime)
                        .FirstOrDefault();

                    if (latestFile != null)
                    {
                        await using var stream = File.OpenRead(latestFile.FullName);
                        var raw = await JsonSerializer.DeserializeAsync<List<JsonElement>>(stream);
                        var logs = raw?.Select(SeqJsonLogReader.MapToLogEntry).ToList() ?? new();

                        bool success = false;
                        for (int i = 0; i < config.Webhook.RetryCount; i++)
                        {
                            success = await WebhookPayloadSender.SendLogsAsync(logs, config);
                            if (success) break;
                            Console.WriteLine($"🔁 Webhook retry {i + 1}/{config.Webhook.RetryCount}...");
                            await Task.Delay((config.Webhook.RetryIntervalBySec * 1000));
                        }

                        if (!success)
                        {
                            Console.WriteLine("❌ All webhook retries failed.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("⚠️ No backup file found for webhook push.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Webhook send failed during scheduler: {ex.Message}");
                }
            }


            Console.WriteLine($"🕒 Waiting {config.Scheduler.IntervalMinutes} minutes...");
            await Task.Delay(TimeSpan.FromMinutes(config.Scheduler.IntervalMinutes));
        }
    }


    public static List<string> GetAvailableDates(string backupDir, string groupBy = "day")
    {
        if (!Directory.Exists(backupDir)) return new();

        var files = Directory.GetFiles(backupDir, "logs_*.json");
        var dateMatches = new List<DateTime>();

        foreach (var file in files)
        {
            var match = Regex.Match(Path.GetFileName(file), @"logs_(\d{4}-\d{2}-\d{2})");
            if (match.Success && DateTime.TryParse(match.Groups[1].Value, out var dt))
                dateMatches.Add(dt);
        }

        return groupBy switch
        {
            "month" => dateMatches.Select(d => d.ToString("yyyy-MM")).Distinct().ToList(),
            "year" => dateMatches.Select(d => d.ToString("yyyy")).Distinct().ToList(),
            "week" => dateMatches.Select(d => ISOWeek.GetYear(d) + "/W" + ISOWeek.GetWeekOfYear(d)).Distinct().ToList(),
            _ => dateMatches.Select(d => d.ToString("yyyy-MM-dd")).Distinct().ToList()
        };
    }

    public static List<string> GetFilesForDate(string backupDir, string datePrefix)
    {
        if (!Directory.Exists(backupDir)) return new();
        return Directory.GetFiles(backupDir, $"logs_{datePrefix}*.json").ToList();
    }

    public static async Task ReadFilesByDate(AppConfig config)
    {
        var groupBy = "day";
        Console.WriteLine("📆 Choose how you want to group files: ");
        Console.WriteLine("[1] Group by Day");
        Console.WriteLine("[2] Group by Month");
        Console.WriteLine("[3] Group by Year");
        Console.WriteLine("[4] Group by Week");
        Console.Write("Your option:");
        var inputGroup = Console.ReadLine()?.Trim();
        switch (inputGroup)
        {
            case "1": groupBy = "day"; break;
            case "2": groupBy = "month"; break;
            case "3": groupBy = "year"; break;
            case "4": groupBy = "week"; break;
            default: Console.WriteLine("⚠️ Invalid input. Defaulting to 'day'."); break;
        }

        var dates = GetAvailableDates(config.BackupDirectory, groupBy);
        if (!dates.Any())
        {
            Console.WriteLine("⚠️ No available log dates found.");
            return;
        }

        Console.WriteLine($"\n📅 Available {groupBy}s:");
        for (int i = 0; i < dates.Count; i++)
            Console.WriteLine($"[{i + 1}] {dates[i]}");

        Console.Write("Select one to read (by number): ");
        if (!int.TryParse(Console.ReadLine(), out int selected) || selected < 1 || selected > dates.Count)
        {
            Console.WriteLine("❌ Invalid selection.");
            return;
        }

        var selectedPrefix = dates[selected - 1];
        var files = GetFilesForDate(config.BackupDirectory, selectedPrefix);
        if (!files.Any())
        {
            Console.WriteLine("⚠️ No log files found for the selected date.");
            return;
        }

        Console.WriteLine("\n📂 Files to analyze:");
        foreach (var f in files)
            Console.WriteLine($"  - {Path.GetFileName(f)}");

        var allLogEntries = new List<RequestLog>();

        foreach (var file in files)
        {
            Console.WriteLine($"\n📂 Reading: {Path.GetFileName(file)}");
            await using var stream = File.OpenRead(file);
            var logs = await JsonSerializer.DeserializeAsync<List<JsonElement>>(stream);
            if (logs == null) continue;

            foreach (var log in logs)
            {
                if (log.ValueKind != JsonValueKind.Object || !log.TryGetProperty("Properties", out var props)) continue;
                var entry = SeqJsonLogReader.MapToLogEntry(log);
                allLogEntries.Add(entry);
            }
        }

        SeqJsonLogReader.AnalysisLog(allLogEntries);

        Console.WriteLine("");
        Console.WriteLine("💾 Do you want to export the analysis?");

        Console.WriteLine("[1] HTML");
        Console.WriteLine("[2] CSV");
        Console.WriteLine("[3] Send to Webhook");
        Console.WriteLine("[4] Quit");
        var exportChoice = Console.ReadLine()?.Trim();

        var exportDir = Path.Combine(AppContext.BaseDirectory, "exports");
        Directory.CreateDirectory(exportDir);

        switch (exportChoice)
        {
            case "1":
                var htmlFile = Path.Combine(exportDir, $"analysis_{DateTime.Now:yyyyMMdd_HHmmss}.html");
                ExportHelper.ExportToHtml(allLogEntries, htmlFile);
                Console.WriteLine($"✅ Exported to HTML: {htmlFile}");
                break;
            case "2":
                var csvFile = Path.Combine(exportDir, $"analysis_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
                ExportHelper.ExportToCsv(allLogEntries, csvFile);
                Console.WriteLine($"✅ Exported to CSV: {csvFile}");
                break;
            case "3":
                await WebhookPayloadSender.SendLogsAsync(allLogEntries, config);
                break;
            default:
            case "4": break;
        }
    }

    public static async Task TestWebhookAsync(AppConfig config)
    {
        if (!config.Webhook.SendFilteredLogs)
        {
            Console.WriteLine("⚠️ Webhook sending is disabled in config.json");
            return;
        }

        Console.WriteLine("🧪 Testing webhook with latest file...");
        var dir = Path.Combine(AppContext.BaseDirectory, config.BackupDirectory);
        var latestFile = Directory.GetFiles(dir, "*.json")
            .Select(f => new FileInfo(f))
            .OrderByDescending(f => f.LastWriteTime)
            .FirstOrDefault();

        if (latestFile == null)
        {
            Console.WriteLine("❌ No file found to test.");
            return;
        }

        await using var stream = File.OpenRead(latestFile.FullName);
        var raw = await JsonSerializer.DeserializeAsync<List<JsonElement>>(stream);
        var logs = raw?.Select(SeqJsonLogReader.MapToLogEntry).ToList() ?? new();
        await WebhookPayloadSender.SendLogsAsync(logs, config);
    }
}
