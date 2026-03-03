using AutoBackupSeq.Models;
using System.Text.Json;

namespace AutoBackupSeq.Services;

public interface ISeqJsonLogReader
{
    Task ReadAllLogsInFolderAsync(string folderPath);
    void AnalysisLog(List<RequestLog> datas);
    RequestLog MapToLogEntry(JsonElement log);
}

public class SeqJsonLogReader : ISeqJsonLogReader
{
    public async Task ReadAllLogsInFolderAsync(string folderPath)
    {
        if (!Directory.Exists(folderPath))
        {
            Console.WriteLine($"❌ Folder not found : {folderPath}");
            return;
        }

        var allFiles = Directory.GetFiles(folderPath, "*.json")
            .Concat(Directory.GetFiles(folderPath, "*.txt"))
            .ToList();

        if (!allFiles.Any())
        {
            Console.WriteLine("⚠️ No log files found.");
            return;
        }

        var allLogEntries = new List<RequestLog>();

        foreach (var file in allFiles)
        {
            Console.WriteLine($"\n📂 Reading: {Path.GetFileName(file)}");
            await using var stream = File.OpenRead(file);
            var logs = await JsonSerializer.DeserializeAsync<List<JsonElement>>(stream);

            if (logs == null) continue;


            foreach (var log in logs)
            {
                if (log.ValueKind != JsonValueKind.Object || !log.TryGetProperty("Properties", out var props)) continue;

                var entry = MapToLogEntry(log);
                allLogEntries.Add(entry);
            }
        }


        AnalysisLog(allLogEntries);
    }

    public void AnalysisLog(List<RequestLog> datas)
    {
        Console.WriteLine($"\n📊 Total log entries: {datas.Count}");

        Console.WriteLine("\n🔥 Top 5 Requested Routes:");
        var topRoutes = datas.GroupBy(e => e.Route)
            .Select(g => new { Route = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(5);

        foreach (var r in topRoutes)
            Console.WriteLine($"  - {r.Route,-40} {r.Count} times");

        Console.WriteLine("\n📉 Status Code Breakdown:");
        var statusBreakdown = datas.GroupBy(e => e.StatusCode)
            .Select(g => new { Code = g.Key, Count = g.Count() })
            .OrderBy(g => g.Code);

        foreach (var s in statusBreakdown)
            Console.WriteLine($"  - {s.Code,-10} {s.Count} times");

        Console.WriteLine("\n🐢 Top 10 Slowest Requests:");
        foreach (var item in datas.OrderByDescending(e => e.Duration).Take(10))
        {
            Console.WriteLine($"  - {item.Route,-40} {item.Duration,6} ms | TraceId: {item.TraceId}");
        }

        Console.WriteLine("\n⏰ Request Volume by Hour:");
        var hourly = datas.GroupBy(e => e.Timestamp.Hour)
            .Select(g => new { Hour = g.Key, Count = g.Count() })
            .OrderBy(g => g.Hour);

        foreach (var h in hourly)
            Console.WriteLine($"  - {h.Hour:00}:00 - {h.Count} requests");

        Console.WriteLine("\n👨‍💼 Most Active Staff:");
        var activeStaff = datas.GroupBy(e => e.StaffId)
            .Where(g => g.Key != "-")
            .Select(g => new { Staff = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(5);

        foreach (var s in activeStaff)
            Console.WriteLine($"  - {s.Staff,-10} {s.Count} times");

        Console.WriteLine("\n🏢 Most Active Companies:");
        var companies = datas.GroupBy(e => e.CompanyId)
            .Where(g => g.Key != "-")
            .Select(g => new { Company = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(5);

        foreach (var c in companies)
            Console.WriteLine($"  - {c.Company,-36} {c.Count} times");

        Console.WriteLine("\n📊 Average Duration per Route:");
        var avgDurations = datas.GroupBy(e => e.Route)
            .Select(g => new { Route = g.Key, Avg = g.Average(e => e.Duration) })
            .OrderByDescending(x => x.Avg)
            .Take(5);

        foreach (var a in avgDurations)
            Console.WriteLine($"  - {a.Route,-40} {a.Avg,6:F1} ms");
    }

    public RequestLog MapToLogEntry(JsonElement log)
    {
        if (!log.TryGetProperty("Properties", out var props))
        {
            return new RequestLog { Timestamp = log.GetProperty("Timestamp").GetDateTime().ToLocalTime() };
        }

        return new RequestLog
        {
            Timestamp = log.GetProperty("Timestamp").GetDateTime().ToLocalTime(),
            TraceId = GetPropString(props, "TraceId") ?? "-",
            Route = GetPropString(props, "Route") ?? "-",
            Method = GetPropString(props, "Method") ?? "-",
            Query = GetPropString(props, "Query") ?? "-",
            RequestBody = GetPropString(props, "RequestBody") ?? "",
            UserName = GetPropString(props, "UserName") ?? "-",
            StaffId = GetPropString(props, "StaffId") ?? "-",
            CompanyId = GetPropString(props, "CompanyId") ?? "-",
            CompanyDepartmentId = GetPropString(props, "CompanyDepartmentId") ?? "-",
            RemoteIP = GetPropString(props, "RemoteIP") ?? "-",
            UserAgent = GetPropString(props, "UserAgent") ?? "-",
            StatusCode = GetPropInt(props, "StatusCode"),
            Duration = GetPropLong(props, "Duration"),
            RequestSize = GetPropLong(props, "RequestBytes"),
            ResponseSize = GetPropLong(props, "ResponseBytes")
        };
    }


    private static string? GetPropString(JsonElement props, string name)
    {
        foreach (var prop in props.EnumerateArray())
        {
            if (prop.TryGetProperty("Name", out var n) && n.GetString() == name &&
                prop.TryGetProperty("Value", out var v) && v.ValueKind == JsonValueKind.String)
                return v.GetString();
        }
        return null;
    }

    private static int GetPropInt(JsonElement props, string name)
    {
        foreach (var prop in props.EnumerateArray())
        {
            if (prop.TryGetProperty("Name", out var n) && n.GetString() == name &&
                prop.TryGetProperty("Value", out var v) && v.TryGetInt32(out int result))
                return result;
        }
        return 0;
    }

    private static long GetPropLong(JsonElement props, string name)
    {
        foreach (var prop in props.EnumerateArray())
        {
            if (prop.TryGetProperty("Name", out var n) && n.GetString() == name &&
                prop.TryGetProperty("Value", out var v) && v.TryGetInt64(out long result))
                return result;
        }
        return 0;
    }
}
