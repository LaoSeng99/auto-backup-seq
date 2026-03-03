using AutoBackupSeq.Models;
using AutoBackupSeq.Services;
using Moq;
using System.Text;
using Xunit;

namespace AutoBackupSeq.Tests;

[Collection("Console Tests")]
public class SchedulerServiceReadFilesExtendedTests
{
    private readonly Mock<ISeqQuery> _seqQueryMock = new();
    private readonly Mock<IWebhookPayloadSender> _webhookSenderMock = new();
    private readonly Mock<ISeqJsonLogReader> _logReaderMock = new();
    private readonly Mock<IExportHelper> _exportHelperMock = new();

    [Fact]
    public async System.Threading.Tasks.Task ReadFilesByDate_ExportsToHtml_WhenSelected()
    {
        // Arrange
        var tempDir = Path.Combine(AppContext.BaseDirectory, "TestBackupsReadHtml");
        if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
        Directory.CreateDirectory(tempDir);
        File.WriteAllText(Path.Combine(tempDir, "logs_2023-10-27_10-00-00.json"), "[]");

        var config = new AppConfig { BackupDirectory = "TestBackupsReadHtml" };

        var input = new StringBuilder();
        input.AppendLine("1");
        input.AppendLine("1");
        input.AppendLine("1");
        Console.SetIn(new StringReader(input.ToString()));

        _logReaderMock.Setup(r => r.MapToLogEntry(It.IsAny<System.Text.Json.JsonElement>()))
                      .Returns(new RequestLog());

        var service = new SchedulerService(_seqQueryMock.Object, _webhookSenderMock.Object, _logReaderMock.Object, _exportHelperMock.Object);

        try
        {
            await service.ReadFilesByDate(config);
            _exportHelperMock.Verify(e => e.ExportToHtml(It.IsAny<List<RequestLog>>(), It.IsAny<string>()), Times.Once);
        }
        finally
        {
            if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
            Console.SetIn(new StreamReader(Console.OpenStandardInput()));
        }
    }

    [Fact]
    public async System.Threading.Tasks.Task ReadFilesByDate_SendsToWebhook_WhenSelected()
    {
        // Arrange
        var tempDir = Path.Combine(AppContext.BaseDirectory, "TestBackupsReadWebhook");
        if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
        Directory.CreateDirectory(tempDir);
        File.WriteAllText(Path.Combine(tempDir, "logs_2023-10-27_10-00-00.json"), "[]");

        var config = new AppConfig { BackupDirectory = "TestBackupsReadWebhook" };

        var input = new StringBuilder();
        input.AppendLine("1");
        input.AppendLine("1");
        input.AppendLine("3");
        Console.SetIn(new StringReader(input.ToString()));

        _logReaderMock.Setup(r => r.MapToLogEntry(It.IsAny<System.Text.Json.JsonElement>()))
                      .Returns(new RequestLog());

        var service = new SchedulerService(_seqQueryMock.Object, _webhookSenderMock.Object, _logReaderMock.Object, _exportHelperMock.Object);

        try
        {
            await service.ReadFilesByDate(config);
            _webhookSenderMock.Verify(w => w.SendLogsAsync(It.IsAny<List<RequestLog>>(), config), Times.Once);
        }
        finally
        {
            if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
            Console.SetIn(new StreamReader(Console.OpenStandardInput()));
        }
    }
}
