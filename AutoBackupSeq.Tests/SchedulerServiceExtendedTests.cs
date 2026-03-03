using AutoBackupSeq.Models;
using AutoBackupSeq.Services;
using Moq;
using Xunit;

namespace AutoBackupSeq.Tests;

public class SchedulerServiceExtendedTests
{
    private readonly Mock<ISeqQuery> _seqQueryMock = new();
    private readonly Mock<IWebhookPayloadSender> _webhookSenderMock = new();
    private readonly Mock<ISeqJsonLogReader> _logReaderMock = new();
    private readonly Mock<IExportHelper> _exportHelperMock = new();

    [Fact]
    public void GetFilesForDate_ReturnsMatchedFiles()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var file1 = Path.Combine(tempDir, "logs_2023-10-27_1.json");
        var file2 = Path.Combine(tempDir, "logs_2023-10-28_1.json");
        File.WriteAllText(file1, "[]");
        File.WriteAllText(file2, "[]");

        var service = new SchedulerService(_seqQueryMock.Object, _webhookSenderMock.Object, _logReaderMock.Object, _exportHelperMock.Object);

        try
        {
            // Act
            var files = service.GetFilesForDate(tempDir, "2023-10-27");

            // Assert
            Assert.Single(files);
            Assert.Contains("logs_2023-10-27_1.json", files[0]);
        }
        finally
        {
            if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async System.Threading.Tasks.Task TestWebhookAsync_WithFiles_SendsLogs()
    {
        // Arrange
        var tempDir = Path.Combine(AppContext.BaseDirectory, "TestBackupsWebhook");
        Directory.CreateDirectory(tempDir);
        var file = Path.Combine(tempDir, "logs_test.json");
        File.WriteAllText(file, "[{\"Timestamp\": \"2023-10-27T10:00:00Z\", \"Properties\": []}]");

        var config = new AppConfig
        {
            Webhook = new WebhookConfig { SendFilteredLogs = true },
            BackupDirectory = "TestBackupsWebhook"
        };

        _logReaderMock.Setup(r => r.MapToLogEntry(It.IsAny<System.Text.Json.JsonElement>()))
                      .Returns(new RequestLog());

        var service = new SchedulerService(_seqQueryMock.Object, _webhookSenderMock.Object, _logReaderMock.Object, _exportHelperMock.Object);

        try
        {
            // Act
            await service.TestWebhookAsync(config);

            // Assert
            _webhookSenderMock.Verify(s => s.SendLogsAsync(It.IsAny<List<RequestLog>>(), config), Times.Once);
        }
        finally
        {
            if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
        }
    }
}
