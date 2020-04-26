using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using Microsoft.Extensions.Configuration;

namespace AddBorder
{
    public static class AddThatBorder
    {
        public static void ProvideBorder(string[] args, IConfigurationRoot config)
        {
            float scale = 1.05f;
            Color fillColor = Color.FromArgb(0, 0, 0);

            try
            {
                string[] colorComponents = config["FillColor"].Split(new char[] { ',' });
                if (colorComponents.Length == 3)
                {
                    fillColor = Color.FromArgb(int.Parse(colorComponents[0]), int.Parse(colorComponents[1]), int.Parse(colorComponents[2]));
                }

                scale = float.Parse(config["Scale"]);
            }
            catch (Exception exc)
            {
                while (exc != null)
                {
                    System.Console.WriteLine($"{exc.GetType().Name} : {exc.Message}");
                    exc = exc.InnerException;
                }
            }

            if (!File.Exists(args[0]))
            {
                System.Console.WriteLine($"ERROR: File [{args[0]}] does not exist.");
                return;
            }

#region args for scale and fillColor
            // if (args.Length > 1)
            // {
            //     string scaleString = args[1];
            //     float argValue = float.Parse(scaleString);
            //     if (argValue > 1.0f)
            //     {
            //         scale = argValue;
            //     }
            //     else
            //     {
            //         scale = 1.0f + float.Parse(scaleString) / 100f;
            //     }
            // }

            // if (args.Length > 2)
            // {
            //     string[] comps = args[2].Split(new char[] { ',' });
            //     fillColor = Color.FromArgb(int.Parse(comps[0]), int.Parse(comps[1]), int.Parse(comps[2]));
            // }
#endregion

            System.Console.WriteLine($"{"FillColor", -12} : {fillColor.R},{fillColor.G},{fillColor.B}");
            System.Console.WriteLine($"{"Scale", -12} : {scale}");

            FileInfo fi = new FileInfo(args[0]);
            System.Console.WriteLine($"{"Source",-12} : {fi.FullName}");
            System.Console.WriteLine($"{"Size",-12} : {fi.Length:#,##0} bytes");

            Bitmap original = new Bitmap($"{fi.FullName}");
            System.Console.WriteLine($"{"Width",-12} : {original.Width}");
            System.Console.WriteLine($"{"Height",-12} : {original.Height}");

            string path = $@"{fi.DirectoryName}\{fi.Name}";
            int idx = path.LastIndexOf('.');

            path = path.Substring(0, idx);
            path += ".bordered" + fi.Extension;
            System.Console.WriteLine($"{"Output", -12} : {path}");
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            resizePic(original, scale, fillColor).Save(path);
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
            Bitmap bmp = new Bitmap(newWidth, newHeight);

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
            return bmp;
        }
    }
}