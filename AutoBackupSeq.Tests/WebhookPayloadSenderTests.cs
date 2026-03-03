using AutoBackupSeq.Models;
using AutoBackupSeq.Services;
using Moq;
using Moq.Protected;
using System.Net;
using Xunit;

namespace AutoBackupSeq.Tests;

public class WebhookPayloadSenderTests
{
    [Fact]
    public async System.Threading.Tasks.Task SendLogsAsync_SuccessStatusCode_ReturnsTrue()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{}"),
            });

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
        Assert.True(result);
    }

    [Fact]
    public async System.Threading.Tasks.Task SendLogsAsync_ErrorStatusCode_ReturnsFalse()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent("Error"),
            });

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
