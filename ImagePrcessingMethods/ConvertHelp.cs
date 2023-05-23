using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.IO;
using System.Drawing.Imaging;
using AForge.Imaging;
using AForge.Imaging.Filters;
using System.Runtime.InteropServices;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

namespace ImageProcessing.ImageProcessingMethods
{
    public class ConvertHelp
    {
        public static bool[][] Image2Bool(System.Drawing.Image img)
        {
            Bitmap bmp = new Bitmap(img);
            bool[][] s = new bool[bmp.Height][];
            for (int y = 0; y < bmp.Height; y++)
            {
                s[y] = new bool[bmp.Width];
                for (int x = 0; x < bmp.Width; x++)
                    s[y][x] = bmp.GetPixel(x, y).GetBrightness() < 0.3;
            }
            return s;

        }

        public static System.Drawing.Image Bool2Image(bool[][] s)
        {
            Bitmap bmp = new Bitmap(s[0].Length, s.Length);
            using (Graphics g = Graphics.FromImage(bmp)) g.Clear(Color.White);
            for (int y = 0; y < bmp.Height; y++)
                for (int x = 0; x < bmp.Width; x++)
                    if (s[y][x]) bmp.SetPixel(x, y, Color.Black);

            return (Bitmap)bmp;
        }

        public static Bitmap CreateNonIndexedImage(System.Drawing.Image src)
        {
            Bitmap newBmp = new Bitmap(src.Width, src.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            using (Graphics gfx = Graphics.FromImage(newBmp))
            {
                gfx.DrawImage(src, 0, 0);
            }

            return newBmp;
        }
        public static Bitmap LoadBitmap(string filename)
        {
            using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
                return new Bitmap(fs);
        }
        public static Bitmap Binarization(Bitmap bmp)
        {
            Image<Gray, Byte> imageCV = bmp.ToImage<Gray, byte>();//Image<Gray, Byte> imageCV = bmp.ToImage<Gray, byte>();
            Mat src = imageCV.Mat;

            Mat gray = new Mat();
            if (imageCV.NumberOfChannels == 3)
                CvInvoke.CvtColor(src, gray, Emgu.CV.CvEnum.ColorConversion.Rgb2Gray);
            else
                gray = src;

            Mat fin = new Mat();
            CvInvoke.Threshold(gray, fin, 127, 255, ThresholdType.Binary | ThresholdType.Otsu);
            Bitmap res = fin.ToBitmap();
            return res;
        }
    }
}
