using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AutoBackupSeq.Models;

public class AppConfig
{
    public string SeqUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string BackupDirectory { get; set; } = "Backups";
    public string ApplicationName { get; set; } = string.Empty;
    public string Environment { get; set; } = "Production";
    public string LogType { get; set; } = "Request";
    public int PageSize { get; set; } = 1000;
    public SchedulerConfig Scheduler { get; set; } = new();
    public WebhookConfig Webhook { get; set; } = new();
}

public class SchedulerConfig
{
    public bool Enabled { get; set; } = false;
    public int IntervalMinutes { get; set; } = 60;
    public int QueryStartOffsetMinutes { get; set; } = -60;
    public int QueryEndOffsetMinutes { get; set; } = 0;
}

public class WebhookConfig
{
    public string Url { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string Header { get; set; } = "Authorization";
    public bool SendFilteredLogs { get; set; } = false;
    public int RetryCount { get; set; } = 3;
    public int RetryIntervalBySec { get; set; } = 3;
}


public static class ConfigLoader
{
    public static AppConfig Load(string path)
    {
        if (!File.Exists(path))
        {
            Console.WriteLine($"❌ Config file not found: {path}");
            Environment.Exit(1);
        }

        var json = File.ReadAllText(path);
        var config = JsonSerializer.Deserialize<AppConfig>(json);
        if (config == null) throw new InvalidOperationException("❌ Invalid config format.");
        return config;
    }
}

