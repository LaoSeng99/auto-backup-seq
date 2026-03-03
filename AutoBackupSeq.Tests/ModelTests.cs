using AutoBackupSeq.Models;
using Xunit;

namespace AutoBackupSeq.Tests;

public class ModelTests
{
    [Fact]
    public void AppConfig_Properties_Work()
    {
        var config = new AppConfig
        {
            SeqUrl = "url",
            ApiKey = "key",
            BackupDirectory = "dir",
            ApplicationName = "app",
            Environment = "env",
            LogType = "type",
            PageSize = 100,
            Scheduler = new SchedulerConfig(),
            Webhook = new WebhookConfig()
        };

        Assert.Equal("url", config.SeqUrl);
        Assert.Equal("key", config.ApiKey);
        Assert.Equal("dir", config.BackupDirectory);
        Assert.Equal("app", config.ApplicationName);
        Assert.Equal("env", config.Environment);
        Assert.Equal("type", config.LogType);
        Assert.Equal(100, config.PageSize);
        Assert.NotNull(config.Scheduler);
        Assert.NotNull(config.Webhook);
    }

    [Fact]
    public void RequestLog_Properties_Work()
    {
        var log = new RequestLog
        {
            Id = "id",
            Timestamp = DateTime.UtcNow,
            TraceId = "trace",
            Route = "route",
            Method = "method",
            Query = "query",
            RequestBody = "body",
            UserName = "user",
            StaffId = "staff",
            CompanyId = "company",
            CompanyDepartmentId = "dept",
            RemoteIP = "ip",
            UserAgent = "agent",
            StatusCode = 200,
            Duration = 10,
            RequestSize = 1,
            ResponseSize = 2
        };

        Assert.Equal("id", log.Id);
        Assert.Equal("trace", log.TraceId);
        Assert.Equal(200, log.StatusCode);
    }

    [Fact]
    public void SeqEvent_Properties_Work()
    {
        var evt = new SeqEvent
        {
            Id = "id",
            Timestamp = DateTime.UtcNow,
            Level = "info",
            MessageTemplateTokens = new List<SeqToken> { new SeqToken { Text = "text" } },
            Properties = new List<SeqProperty> { new SeqProperty { Name = "name", Value = "value" } }
        };

        Assert.Equal("id", evt.Id);
        Assert.Equal("info", evt.Level);
        Assert.Equal("text", evt.MessageTemplateTokens[0].Text);
        Assert.Equal("name", evt.Properties[0].Name);
        Assert.Equal("value", evt.Properties[0].Value);
    }
}
