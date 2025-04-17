
# 🚀 AutoBackupSeq Console Toolkit

一个用于 **备份与分析 Seq 日志** 的命令行工具，适用于企业内部环境中需要定期归档、Webhook 通报或本地分析的场景。

---

## ✨ 功能特色

- 🔍 从远程 Seq 拉取日志（可指定时间范围）
- 📂 分析本地日志文件
- 🛠️ 启动定时任务后台备份
- 🧹 自动清理旧文件
- 🧪 测试 Webhook 推送机制

---

## ⚙️ 配置文件示例 (`config.json`)

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

## 📦 快速开始

```bash
dotnet run --project AutoBackupSeq
```

> 默认行为将读取 `config.json` 并显示交互菜单。

---

## 🧱 日志 Payload 模板（空结构）

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

## 🧩 C# 类定义（`LogEvent.cs`）

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

## 🧩 TypeScript 接口定义（`LogEvent.ts`）

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

## 🧪 测试建议（可选）

- `SchedulerServiceTests.cs`
- `WebhookServiceTests.cs`
- `LogCleanerTests.cs`

---

## 📜 License

MIT License

---

## ✍️ 作者

Developed by **Lao Seng** · AutoBackupSeq v1.0
