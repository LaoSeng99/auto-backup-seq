using AutoBackupSeq.Services;
using Xunit;

namespace AutoBackupSeq.Tests;

public class LogCleanerTests
{
    [Fact]
    public void CleanupOldFiles_DeletesOldFiles()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        var oldFile = Path.Combine(tempDir, "old.json");
        var newFile = Path.Combine(tempDir, "new.json");

        File.WriteAllText(oldFile, "{}");
        File.WriteAllText(newFile, "{}");

        // Set old file to 40 days ago
        File.SetLastWriteTime(oldFile, DateTime.Now.AddDays(-40));

        var cleaner = new LogCleaner();

        try
        {
            // Act
            cleaner.CleanupOldFiles(tempDir, 30);

            // Assert
            Assert.False(File.Exists(oldFile));
            Assert.True(File.Exists(newFile));
        }
        finally
        {
            if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
        }
    }
}
