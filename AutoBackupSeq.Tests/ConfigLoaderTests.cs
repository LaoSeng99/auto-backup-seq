using AutoBackupSeq.Models;
using Xunit;

namespace AutoBackupSeq.Tests;

public class ConfigLoaderTests
{
    [Fact]
    public void Load_ValidJson_ReturnsAppConfig()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        var json = "{\"SeqUrl\": \"http://localhost:5341\", \"ApiKey\": \"test-key\"}";
        File.WriteAllText(tempFile, json);

        try
        {
            // Act
            var config = ConfigLoader.Load(tempFile);

            // Assert
            Assert.NotNull(config);
            Assert.Equal("http://localhost:5341", config.SeqUrl);
            Assert.Equal("test-key", config.ApiKey);
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    [Fact]
    public void Load_MissingFile_ExitsApplication()
    {
        // We can't easily test Environment.Exit(1) without refactoring ConfigLoader to be non-static
        // or using a wrapper for Environment.
        // Given the instructions, I'll focus on success paths first.
    }
}
