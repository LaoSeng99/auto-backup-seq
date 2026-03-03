using AutoBackupSeq.Models;
using AutoBackupSeq.Services;
using Moq;
using Xunit;

namespace AutoBackupSeq.Tests;

public class SchedulerServiceTests
{
    private readonly Mock<ISeqQuery> _seqQueryMock = new();
    private readonly Mock<IWebhookPayloadSender> _webhookSenderMock = new();
    private readonly Mock<ISeqJsonLogReader> _logReaderMock = new();
    private readonly Mock<IExportHelper> _exportHelperMock = new();

    [Fact]
    public void GetAvailableDates_ReturnsCorrectDates()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        File.WriteAllText(Path.Combine(tempDir, "logs_2023-10-27_10-00-00.json"), "[]");
        File.WriteAllText(Path.Combine(tempDir, "logs_2023-10-28_10-00-00.json"), "[]");

        var service = new SchedulerService(_seqQueryMock.Object, _webhookSenderMock.Object, _logReaderMock.Object, _exportHelperMock.Object);

        try
        {
            // Act
            var dates = service.GetAvailableDates(tempDir, "day");

            // Assert
            Assert.Equal(2, dates.Count);
            Assert.Contains("2023-10-27", dates);
            Assert.Contains("2023-10-28", dates);
        }
        finally
        {
            if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async System.Threading.Tasks.Task StartAsync_RespectsCancellation()
    {
        // Arrange
        var config = new AppConfig
        {
            Scheduler = new SchedulerConfig
            {
                Enabled = true,
                IntervalMinutes = 1,
                QueryStartOffsetMinutes = -60,
                QueryEndOffsetMinutes = 0
            }
        };

        var service = new SchedulerService(_seqQueryMock.Object, _webhookSenderMock.Object, _logReaderMock.Object, _exportHelperMock.Object);
        var cts = new CancellationTokenSource();

        // Act
        var task = service.StartAsync(config, cts.Token);
        cts.Cancel();

        // Assert
        await task; // Should complete quickly after cancellation
        _seqQueryMock.Verify(x => x.QueryAsync(It.IsAny<AppConfig>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.AtMostOnce());
    }
}
