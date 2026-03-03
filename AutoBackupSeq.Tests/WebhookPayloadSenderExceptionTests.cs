using AutoBackupSeq.Models;
using AutoBackupSeq.Services;
using Moq;
using Moq.Protected;
using System.Net;
using Xunit;

namespace AutoBackupSeq.Tests;

public class WebhookPayloadSenderExceptionTests
{
    [Fact]
    public async System.Threading.Tasks.Task SendLogsAsync_Exception_ReturnsFalse()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ThrowsAsync(new HttpRequestException("Network error"));

        var httpClient = new HttpClient(handlerMock.Object);
        var sender = new WebhookPayloadSender(httpClient);
        var config = new AppConfig
        {
            Webhook = new WebhookConfig
            {
                SendFilteredLogs = true,
                Url = "http://webhook.com"
            }
        };
        var logs = new List<RequestLog>();

        // Act
        var result = await sender.SendLogsAsync(logs, config);

        // Assert
        Assert.False(result);
    }
}
