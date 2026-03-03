using AutoBackupSeq.Models;

namespace AutoBackupSeq.Services;

public interface IWebhookPayloadSender
{
    Task<bool> SendLogsAsync(List<RequestLog> logs, AppConfig config);
}

public class WebhookPayloadSender : IWebhookPayloadSender
{
    private readonly HttpClient _httpClient;

    public WebhookPayloadSender(HttpClient? httpClient = null)
    {
        _httpClient = httpClient ?? new HttpClient();
    }

    public async Task<bool> SendLogsAsync(List<RequestLog> logs, AppConfig config)
    {
        if (!config.Webhook.SendFilteredLogs || string.IsNullOrWhiteSpace(config.Webhook.Url))
        {
            Console.WriteLine("⚠️ Webhook JSON payload not sent (disabled or URL missing).");
            return false;
        }

        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, config.Webhook.Url);
            request.Headers.TryAddWithoutValidation(
                config.Webhook.Header ?? "Authorization",
                config.Webhook.Token);

            var json = System.Text.Json.JsonSerializer.Serialize(logs, new System.Text.Json.JsonSerializerOptions { WriteIndented = false });
            request.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("📤 Filtered logs sent to Webhook ✅");
                return true;
            }
            else
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"❌ Webhook rejected the payload: {response.StatusCode}");
                Console.WriteLine($"❌ Webhook rejected the message: {responseJson}");
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Webhook error: {ex.Message}");
            return false;
        }
    }
}
