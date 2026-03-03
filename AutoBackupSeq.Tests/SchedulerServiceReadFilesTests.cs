using AutoBackupSeq.Models;
using AutoBackupSeq.Services;
using Moq;
using System.Text;
using Xunit;

namespace AutoBackupSeq.Tests;

[Collection("Console Tests")]
public class SchedulerServiceReadFilesTests
{
    private readonly Mock<ISeqQuery> _seqQueryMock = new();
    private readonly Mock<IWebhookPayloadSender> _webhookSenderMock = new();
    private readonly Mock<ISeqJsonLogReader> _logReaderMock = new();
    private readonly Mock<IExportHelper> _exportHelperMock = new();

    [Fact]
    public async System.Threading.Tasks.Task ReadFilesByDate_ProcessesFilesAndAnalyzes()
    {
        // Arrange
        var tempDir = Path.Combine(AppContext.BaseDirectory, "TestBackupsRead");
        if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
        Directory.CreateDirectory(tempDir);
        File.WriteAllText(Path.Combine(tempDir, "logs_2023-10-27_10-00-00.json"), "[{\"Timestamp\": \"2023-10-27T10:00:00Z\", \"Properties\": []}]");

        var config = new AppConfig
        {
            BackupDirectory = "TestBackupsRead"
        };

        var input = new StringBuilder();
        input.AppendLine("1");
        input.AppendLine("1");
        input.AppendLine("4");
        Console.SetIn(new StringReader(input.ToString()));

        _logReaderMock.Setup(r => r.MapToLogEntry(It.IsAny<System.Text.Json.JsonElement>()))
                      .Returns(new RequestLog());

        var service = new SchedulerService(_seqQueryMock.Object, _webhookSenderMock.Object, _logReaderMock.Object, _exportHelperMock.Object);

        try
        {
            await service.ReadFilesByDate(config);
            _logReaderMock.Verify(r => r.AnalysisLog(It.IsAny<List<RequestLog>>()), Times.Once);
        }
        finally
        {
            if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
            Console.SetIn(new StreamReader(Console.OpenStandardInput()));
        }
    }
}
