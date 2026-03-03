using AutoBackupSeq.Services;
using System.Text.Json;
using Xunit;

namespace AutoBackupSeq.Tests;

public class SeqJsonLogReaderTests
{
    [Fact]
    public void MapToLogEntry_ValidJson_ReturnsRequestLog()
    {
        // Arrange
        var json = @"{
            ""Timestamp"": ""2023-10-27T10:00:00Z"",
            ""Properties"": [
                { ""Name"": ""TraceId"", ""Value"": ""trace123"" },
                { ""Name"": ""Route"", ""Value"": ""/api/test"" },
                { ""Name"": ""StatusCode"", ""Value"": 200 },
                { ""Name"": ""Duration"", ""Value"": 150 }
            ]
        }";
        var doc = JsonDocument.Parse(json);
        var reader = new SeqJsonLogReader();

        // Act
        var result = reader.MapToLogEntry(doc.RootElement);

        // Assert
        Assert.Equal("trace123", result.TraceId);
        Assert.Equal("/api/test", result.Route);
        Assert.Equal(200, result.StatusCode);
        Assert.Equal(150, result.Duration);
    }
}
