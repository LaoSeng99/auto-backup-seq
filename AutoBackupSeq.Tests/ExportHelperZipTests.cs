using AutoBackupSeq.Models;
using AutoBackupSeq.Services;
using System.Text;
using Xunit;

namespace AutoBackupSeq.Tests;

public class ExportHelperZipTests
{
    [Fact]
    public void ZipExportDirectory_CreatesZipFile()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        File.WriteAllText(Path.Combine(tempDir, "test.csv"), "test");

        var helper = new ExportHelper();

        try
        {
            // Act
            var zipPath = helper.ZipExportDirectory(tempDir);

            // Assert
            Assert.True(File.Exists(zipPath));
            Assert.EndsWith(".zip", zipPath);
        }
        finally
        {
            if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void ZipExportDirectory_EmptyDirectory_ReturnsEmpty()
    {
         // Arrange
        var helper = new ExportHelper();

        // Act
        var result = helper.ZipExportDirectory("non-existent-dir");

        // Assert
        Assert.Equal(string.Empty, result);
    }
}
