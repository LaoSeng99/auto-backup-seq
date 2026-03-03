using AutoBackupSeq.Models;
using AutoBackupSeq.Services;
using Moq;
using Xunit;

namespace AutoBackupSeq.Tests;

public class SchedulerServiceGroupByTests
{
    private readonly Mock<ISeqQuery> _seqQueryMock = new();
    private readonly Mock<IWebhookPayloadSender> _webhookSenderMock = new();
    private readonly Mock<ISeqJsonLogReader> _logReaderMock = new();
    private readonly Mock<IExportHelper> _exportHelperMock = new();

    [Theory]
    [InlineData("month", "2023-10")]
    [InlineData("year", "2023")]
    [InlineData("week", "2023/W43")]
    [InlineData("day", "2023-10-27")]
    public void GetAvailableDates_HandlesDifferentGroupings(string groupBy, string expected)
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        File.WriteAllText(Path.Combine(tempDir, "logs_2023-10-27_10-00-00.json"), "[]");

        var service = new SchedulerService(_seqQueryMock.Object, _webhookSenderMock.Object, _logReaderMock.Object, _exportHelperMock.Object);

        try
        {
            // Act
            var dates = service.GetAvailableDates(tempDir, groupBy);

            // Assert
            Assert.Contains(expected, dates);
        }
        finally
        {
            if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
        }
    }
}
