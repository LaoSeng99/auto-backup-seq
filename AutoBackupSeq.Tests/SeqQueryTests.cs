using AutoBackupSeq.Models;
using AutoBackupSeq.Services;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text.Json;
using Xunit;

namespace AutoBackupSeq.Tests;

public class SeqQueryTests
{
    [Fact]
    public async System.Threading.Tasks.Task QueryAsync_FetchesAndSavesLogs()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>();
        var responseContent = "[{\"Id\": \"event-1\", \"Timestamp\": \"2023-10-27T10:00:00Z\", \"Properties\": []}]";
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent),
            });

        var httpClient = new HttpClient(handlerMock.Object);
        var seqQuery = new SeqQuery(httpClient);

        var config = new AppConfig
        {
            SeqUrl = "http://localhost:5341",
            BackupDirectory = "TestBackups",
            PageSize = 10
        };

        var startTime = DateTime.UtcNow.AddHours(-1);
        var endTime = DateTime.UtcNow;

        var outputDir = Path.Combine(AppContext.BaseDirectory, config.BackupDirectory);
        if (Directory.Exists(outputDir)) Directory.Delete(outputDir, true);

        try
        {
            // Act
            await seqQuery.QueryAsync(config, startTime, endTime);

            // Assert
            Assert.True(Directory.Exists(outputDir));
            var files = Directory.GetFiles(outputDir, "*.json");
            Assert.Single(files);
            var savedContent = File.ReadAllText(files[0]);
            Assert.Contains("event-1", savedContent);
        }
        finally
        {
            if (Directory.Exists(outputDir)) Directory.Delete(outputDir, true);
        }
    }
}
