using AutoBackupSeq.Models;
using AutoBackupSeq.Services;
using Moq;
using System.Text.Json;
using Xunit;

namespace AutoBackupSeq.Tests;

public class SchedulerServiceWebhookTests
{
    private readonly Mock<ISeqQuery> _seqQueryMock = new();
    private readonly Mock<IWebhookPayloadSender> _webhookSenderMock = new();
    private readonly Mock<ISeqJsonLogReader> _logReaderMock = new();
    private readonly Mock<IExportHelper> _exportHelperMock = new();

    [Fact]
    public async System.Threading.Tasks.Task StartAsync_SendsWebhook_WhenEnabled()
    {
        // Arrange
        var tempDir = Path.Combine(AppContext.BaseDirectory, "TestBackupsWebhookStart");
        if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
        Directory.CreateDirectory(tempDir);
        var file = Path.Combine(tempDir, "logs_test.json");
        File.WriteAllText(file, "[]");

        var config = new AppConfig
        {
            Scheduler = new SchedulerConfig
            {
                Enabled = true,
                IntervalMinutes = 1,
                QueryStartOffsetMinutes = -60,
                QueryEndOffsetMinutes = 0
            },
            Webhook = new WebhookConfig
            {
                SendFilteredLogs = true,
                RetryCount = 1,
                RetryIntervalBySec = 0
            },
            BackupDirectory = "TestBackupsWebhookStart"
        };

        _webhookSenderMock.Setup(x => x.SendLogsAsync(It.IsAny<List<RequestLog>>(), It.IsAny<AppConfig>()))
                          .ReturnsAsync(true);

        var service = new SchedulerService(_seqQueryMock.Object, _webhookSenderMock.Object, _logReaderMock.Object, _exportHelperMock.Object);
        var cts = new CancellationTokenSource();

        // Act
        var task = service.StartAsync(config, cts.Token);

        // Give it a moment to run one iteration
        await Task.Delay(500);
        cts.Cancel();

        // Assert
        _webhookSenderMock.Verify(x => x.SendLogsAsync(It.IsAny<List<RequestLog>>(), It.IsAny<AppConfig>()), Times.AtLeastOnce());

        if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
    }
}
