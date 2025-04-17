using AutoBackupSeq.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AutoBackupSeq;

public static class WebhookPayloadSender
{
    public static async Task<bool> SendLogsAsync(List<RequestLog> logs, AppConfig config)
    {
        if (!config.Webhook.SendFilteredLogs || string.IsNullOrWhiteSpace(config.Webhook.Url))
        {
            Console.WriteLine("⚠️ Webhook JSON payload not sent (disabled or URL missing).");
            return false;
        }

        try
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.TryAddWithoutValidation(
                config.Webhook.Header ?? "Authorization",
                config.Webhook.Token);

            var json = JsonSerializer.Serialize(logs, new JsonSerializerOptions { WriteIndented = false });
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(config.Webhook.Url, content);
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
