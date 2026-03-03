using AutoBackupSeq.Services;
using Xunit;

namespace AutoBackupSeq.Tests;

public class LogCleanerExceptionTests
{
    [Fact]
    public void CleanupOldFiles_FolderDoesNotExist_ReturnsEarly()
    {
        // Arrange
        var cleaner = new LogCleaner();

        // Act & Assert (Should not throw)
        cleaner.CleanupOldFiles("non-existent-folder", 30);
    }
}
