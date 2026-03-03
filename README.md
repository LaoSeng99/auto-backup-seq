# 🚀 AutoBackupSeq Console Toolkit

**AutoBackupSeq** is a lightweight, configurable CLI tool designed to automate log management from **Seq**. It retrieves logs, filters them by time or custom rules, performs on-the-fly analysis, and can forward them to a server or webhook endpoint. Ideal for backup, analysis, or alerting workflows in enterprise environments.

> **Note:** Much of the code was initially generated or supported by ChatGPT-4o. Feel free to fork and improve!

---

## ✨ Features

- 🔍 **Retrieve Logs from Seq**: Fetch logs based on time ranges with automatic pagination.
- 📂 **Local Log Analysis**: Read and analyze previously saved local JSON log files.
- 📊 **Rich Analysis Metrics**:
  - Top 5 Requested Routes
  - Status Code Breakdown
  - Top 10 Slowest Requests
  - Request Volume by Hour
  - Most Active Staff/Companies
  - Average Duration per Route
- 🛠️ **Automated Scheduler**: Run background tasks to automate log backups and webhook forwarding.
- 🧹 **Log Cleanup**: Automatically delete old backup and exported files to save disk space.
- 🧪 **Webhook Testing**: Easily test your webhook endpoint with the latest data file.
- 💾 **Export Options**: Export analysis results to **HTML** or **CSV**.

---

## 📋 Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- A running [Seq](https://datalust.co/seq) server and an API Key (optional but recommended).

---

## ⚙️ Configuration (`config.json`)

```json
{
  "SeqUrl": "https://your-seq-server.com",
  "ApiKey": "your-seq-server-api-key",
  "BackupDirectory": "backups",
  "ApplicationName": "your-seq-log-app-name",
  "Environment": "Production",
  "LogType": "Request",
  "PageSize": 1000,
  "Scheduler": {
    "Enabled": true,
    "IntervalMinutes": 1440,
    "QueryStartOffsetMinutes": -1440,
    "QueryEndOffsetMinutes": 0
  },
  "Webhook": {
    "Url": "https://your-server.com/webhook",
    "Token": "your-secret-token",
    "Header": "Authorization",
    "SendFilteredLogs": true,
    "RetryCount": 3,
    "RetryIntervalBySec": 3
  }
}
```

### Configuration Fields

| Field | Description |
|-------|-------------|
| `SeqUrl` | The URL of your Seq server. |
| `ApiKey` | Your Seq API key for authentication. |
| `BackupDirectory` | Directory where logs will be saved. |
| `ApplicationName` | The application name filter for Seq queries. |
| `LogType` | The log type filter (e.g., "Request"). |
| `Scheduler.Enabled` | Whether the background scheduler is active. |
| `Scheduler.IntervalMinutes` | How often the scheduler runs. |
| `Webhook.Url` | The endpoint to send logs to. |
| `Webhook.SendFilteredLogs` | If true, logs will be forwarded via webhook after retrieval. |

---

## 📦 Quick Start

1. **Clone the repository.**
2. **Update `config.json`** with your Seq server details and desired settings.
3. **Run the application**:
   ```bash
   dotnet run --project AutoBackupSeq
   ```
4. **Interactive Menu**: Follow the on-screen prompts to filter logs, analyze files, or start the scheduler.

---

## 📁 Project Structure

- `AutoBackupSeq/`
  - `Models/`: Data structures for logs and configuration.
  - `Program.cs`: Entry point and interactive menu logic.
  - `SeqQuery.cs`: Logic for fetching logs from the Seq API.
  - `SchedulerService.cs`: Background task management and file-based operations.
  - `SeqJsonLogReader.cs`: Parsing and analyzing log files.
  - `WebhookPayloadSender.cs`: Webhook integration logic.
  - `ExportHelper.cs`: Utilities for exporting analysis to HTML/CSV.
  - `LogCleaner.cs`: Cleanup logic for old backup/export files.

---

## 🧩 Data Model (`RequestLog.cs`)

The application maps Seq events to the following `RequestLog` structure:

```csharp
public class RequestLog
{
    public string Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string TraceId { get; set; }
    public string Route { get; set; }
    public string Method { get; set; }
    public string Query { get; set; }
    public string RequestBody { get; set; }
    public string UserName { get; set; }
    public string StaffId { get; set; }
    public string CompanyId { get; set; }
    public string CompanyDepartmentId { get; set; }
    public string RemoteIP { get; set; }
    public string UserAgent { get; set; }
    public int StatusCode { get; set; }
    public long Duration { get; set; }
    public long RequestSize { get; set; }
    public long ResponseSize { get; set; }
}
```

---

## 📜 License

This project is licensed under the [MIT License](LICENSE.txt).

---

## ✍️ Author

Developed by **Lao Seng** · AutoBackupSeq v1.0
