using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using Microsoft.Extensions.Configuration;
using System.Globalization;

namespace AddBorder
{
    public static class AddThatBorder
    {
        private static bool success = true;
        private static IConfigurationRoot _config;

        public static Color ColorFromRgbTriplet(string[] rgb)
        {
            return Color.FromArgb(255, int.Parse(rgb[0]), int.Parse(rgb[1]), int.Parse(rgb[2]));
        }

        public static Color GetUserColor(string[] args)
        {
            string[] configColor = _config["FillColor"].Split(',');
            Color result = ColorFromRgbTriplet(configColor);
            foreach (var arg in args)
            {
                var components = arg.Split(',');
                if (components.Length == 3)
                {
                    result = ColorFromRgbTriplet(components);
                }
            }
            return result;
        }

        public static float GetUserScale(string[] args)
        {
            float result = float.Parse(_config["Scale"]);
            foreach (var arg in args)
            {
                float tempFloat;
                if (float.TryParse(arg, NumberStyles.AllowDecimalPoint, null, out tempFloat))
                {
                    result = float.Parse(arg);
                }
            }
            return result;
        }

        public static FileInfo GetUserSrcFile(string[] args)
        {
            FileInfo fi = new FileInfo("---");
            foreach (var arg in args)
            {
                if (File.Exists(arg))
                {
                    fi = new FileInfo(arg);
                }
            }
            return fi;
        }

        public static bool ProvideBorder(string[] args, IConfigurationRoot config)
        {
            _config = config;
            float scale = 1.05f;
            Color fillColor = Color.FromArgb(0, 0, 0);
            FileInfo srcFile;

            fillColor = GetUserColor(args);
            scale = GetUserScale(args);
            srcFile = GetUserSrcFile(args);

            if (!srcFile.Exists)
            {
                System.Console.WriteLine($"File [{srcFile.FullName}] does not exist.");
                System.Console.WriteLine("Exiting.");
                System.Environment.Exit(-1);
            }

            System.Console.WriteLine($"{"FillColor",-12} : {fillColor.R},{fillColor.G},{fillColor.B}");
            System.Console.WriteLine($"{"Scale",-12} : {scale}");

            FileInfo fi = srcFile;
            System.Console.WriteLine($"{"Source",-12} : {fi.FullName}");
            System.Console.WriteLine($"{"Size",-12} : {fi.Length:#,##0} bytes");

            Bitmap original = new Bitmap($"{fi.FullName}");
            System.Console.WriteLine($"{"Width",-12} : {original.Width}");
            System.Console.WriteLine($"{"Height",-12} : {original.Height}");

            string path = $@"{fi.DirectoryName}\{fi.Name}";
            int idx = path.LastIndexOf('.');

            path = path.Substring(0, idx);
            path += ".bordered" + fi.Extension;
            System.Console.WriteLine($"{"Output",-12} : {path}");
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            resizePic(original, scale, fillColor).Save(path);

            return success;
        }

        private static ImageCodecInfo GetEncoderInfo(String mimeType)
        {
            int j;
            ImageCodecInfo[] encoders;
            encoders = ImageCodecInfo.GetImageEncoders();
            for (j = 0; j < encoders.Length; ++j)
            {
                if (encoders[j].MimeType == mimeType)
                {
                    return encoders[j];
                }
            }
            return null;
        }

        private static void OutputRectangle(Rectangle rectangle, string name = "")
        {
            System.Console.WriteLine($"{name} = {rectangle.Left} {rectangle.Top} {rectangle.Right} {rectangle.Bottom}");
        }

        private static Bitmap resizePic(Bitmap original, float scale, Color fillColor)
        {
            Bitmap bmp = new Bitmap(10, 10);

            if (scale == 1)
            {
                return original;
            }

            int borderSize = 0; // Math.Min((int)(scale * (float)original.Width), (int)(scale * (float)original.Height));

            float scaleW = scale;
            float scaleH = scale;

            if (original.Width > original.Height)
            {
                borderSize = (int)(scale * (float)original.Width) - original.Width;
            }
            else
            {
                borderSize = (int)(scale * (float)original.Height) - original.Height;
            }
            borderSize /= 2;

            int newWidth = original.Width + (2 * borderSize);
            int newHeight = original.Height + (2 * borderSize);

            try
            {
                bmp = new Bitmap(newWidth, newHeight);

                using (Graphics gfx = Graphics.FromImage(bmp))
                using (SolidBrush brush = new SolidBrush(fillColor))
                {
                    gfx.FillRectangle(brush, 0, 0, bmp.Width, bmp.Height);
                }

                Graphics graphics = Graphics.FromImage(bmp);
                graphics.InterpolationMode = InterpolationMode.High;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.SmoothingMode = SmoothingMode.AntiAlias;

                // graphics.DrawString("Copyright 2020 by Galen Babcock",
                //                     new Font(FontFamily.GenericSansSerif, 500, FontStyle.Bold),
                //                     new SolidBrush(Color.White),
                //                     new Point(1000,1000));

                Rectangle destRect = new Rectangle(borderSize, borderSize, original.Width, original.Height);
                Rectangle srcRect = new Rectangle(0, 0, original.Width, original.Height);
                graphics.DrawImage(original,
                            destRect,
                            srcRect,
                            GraphicsUnit.Pixel);
            }
            catch (Exception exc)
            {
                while (exc != null)
                {
                    System.Console.WriteLine($"{exc.GetType().Name} : {exc.Message}");
                    System.Console.WriteLine($"{exc.StackTrace}");
                    exc = exc.InnerException;
                }
                success = false;
            }
            return bmp;
        }
    }
}