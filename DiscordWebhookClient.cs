using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace DiscordWebhook
{ 
    public class DiscordWebhookClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _webhookUrl;

        public DiscordWebhookClient(string webhookUrl)
        {
            _httpClient = new HttpClient();
            _webhookUrl = webhookUrl;
        }

        public async Task SendMessageAsync(string message)
        {
            using StringContent jsonContent = new StringContent(
                JsonSerializer.Serialize(new { content = message }),
                Encoding.UTF8,
                "application/json");

            await _httpClient.PostAsync(_webhookUrl, jsonContent);
        }
    }

}