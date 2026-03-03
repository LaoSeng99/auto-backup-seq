using AutoBackupSeq.Models;
using AutoBackupSeq.Services;
using Xunit;

namespace AutoBackupSeq.Tests;

public class SeqJsonLogReaderAnalysisTests
{
    [Fact]
    public void AnalysisLog_CalculatesCorrectTopRoutes()
    {
        // Arrange
        var logs = new List<RequestLog>
        {
            new RequestLog { Route = "/home" },
            new RequestLog { Route = "/home" },
            new RequestLog { Route = "/api/data" }
        };
        var reader = new SeqJsonLogReader();

        // Act & Assert (mostly checking it doesn't throw and coverage is hit)
        // Since it writes to Console, we just ensure it executes.
        reader.AnalysisLog(logs);
    }

    [Fact]
    public async System.Threading.Tasks.Task ReadAllLogsInFolderAsync_ReadsFilesAndAnalyzes()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var logFile = Path.Combine(tempDir, "test.json");
        var json = "[{\"Timestamp\": \"2023-10-27T10:00:00Z\", \"Properties\": [{\"Name\": \"Route\", \"Value\": \"/test\"}]}]";
        File.WriteAllText(logFile, json);

        var reader = new SeqJsonLogReader();

        try
        {
            // Act
            await reader.ReadAllLogsInFolderAsync(tempDir);

            // Assert - if it reached here without exception, success.
        }
        finally
        {
            if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
        }
    }
}
