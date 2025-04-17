
# 🚀 AutoBackupSeq Console Toolkit

**AutoBackupSeq** is a lightweight and configurable CLI tool that automatically retrieves logs from **Seq**, filters them by time or custom rules, and **forwards them to your server or webhook endpoint**. Ideal for backup, analysis, or alerting workflows in enterprise environments.

**Most of the code is generated or supported by ChatGPT-4o. If you encounter any issues, they may or may not be fixed by me. Feel free to fork the project and make improvements if you're interested.
---

## ✨ Features

- 🔍 Retrieve logs from Seq (filterable by time range)
- 📂 Analyze previously saved local log files
- 🛠️ Start background scheduler to automate backups
- 🧹 Automatically clean up old backup/exported files
- 🧪 Test webhook delivery with latest data file

---

## ⚙️ Configuration Example (`config.json`)

```json
{
  "SeqUrl": "http://your-seq-server.com",
  "ApiKey": "your-server-key",
  "BackupDirectory": "Backups",
  "ApplicationName": "your-log-appication-name",
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

---

## 📦 Quick Start

```bash
dotnet run --project AutoBackupSeq
```

> By default, the app reads `config.json` and launches an interactive menu.

---

## 🧱 Log Payload Template (Empty Structure)

```json
{
  "Id": "",
  "Timestamp": "",
  "TraceId": "",
  "Route": "",
  "Method": "",
  "Query": "",
  "RequestBody": "",
  "UserName": "",
  "StaffId": "",
  "CompanyId": "",
  "CompanyDepartmentId": "",
  "RemoteIP": "",
  "UserAgent": "",
  "StatusCode": 0,
  "Duration": 0,
  "RequestSize": 0,
  "ResponseSize": 0
}
```

---

## 🧩 C# Class Definition (`LogEvent.cs`)

```csharp
public class LogEvent
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
    public int Duration { get; set; }
    public int RequestSize { get; set; }
    public int ResponseSize { get; set; }
}
```

---

## 🧩 TypeScript Interface (`LogEvent.ts`)

```ts
export interface LogEvent {
  Id: string;
  Timestamp: string;
  TraceId: string;
  Route: string;
  Method: string;
  Query: string;
  RequestBody: string;
  UserName: string;
  StaffId: string;
  CompanyId: string;
  CompanyDepartmentId: string;
  RemoteIP: string;
  UserAgent: string;
  StatusCode: number;
  Duration: number;
  RequestSize: number;
  ResponseSize: number;
}
```

---

## 🧪 Suggested Tests

- `SchedulerServiceTests.cs`
- `WebhookServiceTests.cs`
- `LogCleanerTests.cs`

---

## 📜 License

MIT License

---

## ✍️ Author

Developed by **Lao Seng** · AutoBackupSeq v1.0
