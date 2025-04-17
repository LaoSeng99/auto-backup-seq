
using AutoBackupSeq.Models;
using System.Text.Json;

namespace AutoBackupSeq;

public class SeqJsonLogReader
{
    public static async Task ReadAllLogsInFolderAsync(string folderPath)
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

    public static void AnalysisLog(List<RequestLog> datas)
    {
        Console.WriteLine($"\n📊 Total log entries: {datas.Count}");
        /** 🔥 Top 5 Requested Routes
              - /users                                   9 times
              - /notifications/logs                      9 times
              - /companies                               9 times
              - /home                                    9 times
              - /Auth/login                              9 times
            ✅ 表示这些 API 或页面是你系统中被访问最多的路由（按访问次数排序）
            📌 用途：
            🔍 帮助你知道哪些页面或 API 是最常被使用的功能（重点监控 / 缓存）
            🎯 优化前端/后端资源（常访问页面应首屏加载）
            📦 后续可建立热门 API/页面访问趋势图
         */
        Console.WriteLine("\n🔥 Top 5 Requested Routes:");
        var topRoutes = datas.GroupBy(e => e.Route)
            .Select(g => new { Route = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(5);

        foreach (var r in topRoutes)
            Console.WriteLine($"  - {r.Route,-40} {r.Count} times");

        /** 📉 Status Code Breakdown
          - 200        54 times
          - 302        9 times
            ✅ 统计各类 HTTP 响应码的出现次数
            200 表示成功，302 表示跳转（如登录重定向）
            📌 用途：
            🚨 快速判断是否有大量 4xx（用户错误）或 5xx（服务错误）
            🔐 如果太多 302，说明用户常被重定向（可能未登录）
            📈 可分析接口稳定性和错误率
         */
        Console.WriteLine("\n📉 Status Code Breakdown:");
        var statusBreakdown = datas.GroupBy(e => e.StatusCode)
            .Select(g => new { Code = g.Key, Count = g.Count() })
            .OrderBy(g => g.Code);

        foreach (var s in statusBreakdown)
            Console.WriteLine($"  - {s.Code,-10} {s.Count} times");

        /**🐢 Top 10 Slowest Requests
          - /Auth/login         516 ms | TraceId: ea9cfa...
          - /home               193 ms | TraceId: 479e1c...
            ✅ 根据 Duration 记录最慢的请求（响应时间），含 TraceId 可查详细链路
            📌 用途：
            🎯 找出性能瓶颈 API → 可优先优化
            🧪 TraceId 可结合其他日志追踪性能细节（如 DB 慢查询）
         */
        Console.WriteLine("\n🐢 Top 10 Slowest Requests:");
        foreach (var item in datas.OrderByDescending(e => e.Duration).Take(10))
        {
            Console.WriteLine($"  - {item.Route,-40} {item.Duration,6} ms | TraceId: {item.TraceId}");
        }

        /**⏰ 每小时请求量
           - 19:00 - 63 requests
            ✅ 分析每小时请求量 → 此处所有请求都集中在 19 点
            📌 用途：
            ⏱️ 判断系统高峰期（未来部署调度资源）
            可结合日历分析工作日 vs 非工作日流量差异
         */
        Console.WriteLine("\n⏰ Request Volume by Hour:");
        var hourly = datas.GroupBy(e => e.Timestamp.Hour)
            .Select(g => new { Hour = g.Key, Count = g.Count() })
            .OrderBy(g => g.Hour);

        foreach (var h in hourly)
            Console.WriteLine($"  - {h.Hour:00}:00 - {h.Count} requests");

        /** 👨‍💼 Most Active Staff
        ❌ 当前无数据显示 → 所有 StaffId == "-"（代表匿名或系统）
        📌 用途：
        🧑‍💼 识别最活跃用户或 QA 测试账号
        可追踪关键用户行为（例如 admin 或 staff）
        ✅ 后续若 IUserContext 成功注入 StaffId，分析将生效。
        */
        Console.WriteLine("\n👨‍💼 Most Active Staff:");
        var activeStaff = datas.GroupBy(e => e.StaffId)
            .Where(g => g.Key != "-")
            .Select(g => new { Staff = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(5);

        foreach (var s in activeStaff)
            Console.WriteLine($"  - {s.Staff,-10} {s.Count} times");

        /**
         🏢 Most Active Companies
        ❌ 当前无数据显示 → CompanyId 也为 "-"，同样为匿名调用
        📌 用途：
        对 SaaS 平台非常关键！可看哪家公司最活跃
        可用于分布式租户调用频率分析（多租户系统）
         */
        Console.WriteLine("\n🏢 Most Active Companies:");
        var companies = datas.GroupBy(e => e.CompanyId)
            .Where(g => g.Key != "-")
            .Select(g => new { Company = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(5);

        foreach (var c in companies)
            Console.WriteLine($"  - {c.Company,-36} {c.Count} times");

        /**
         📊 Average Duration per Route
        - /Auth/login         516.0 ms
        - /home               193.0 ms
        - /companies/4/edit   110.0 ms
        ✅ 每个 Route 的平均响应时间
        📌 用途：
        找出哪些 API/页面平均响应时间偏慢
        可发现“非偶发性”的性能问题（平均慢 ≠ 偶发高峰）
         */
        Console.WriteLine("\n📊 Average Duration per Route:");
        var avgDurations = datas.GroupBy(e => e.Route)
            .Select(g => new { Route = g.Key, Avg = g.Average(e => e.Duration) })
            .OrderByDescending(x => x.Avg)
            .Take(5);

        foreach (var a in avgDurations)
            Console.WriteLine($"  - {a.Route,-40} {a.Avg,6:F1} ms");
    }

    public static RequestLog MapToLogEntry(JsonElement log)
    {
        var props = log.GetProperty("Properties");

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
