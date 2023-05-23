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
    public class MedianFilter
    {
        public static Bitmap MFilter(Bitmap bm)
        {
            List<byte> elementList = new List<byte>();

            byte[,] image = new byte[bm.Width, bm.Height];

            //в чб 
            for (int j = 0; j < bm.Height; j++)
            {
                for (int i = 0; i < bm.Width; i++)
                {
                    var c = bm.GetPixel(i, j);
                    byte gray = (byte)(.333 * c.R + .333 * c.G + .333 * c.B);
                    image[i, j] = gray;
                }
            }

            //сам фильтр
            for (int j = 0; j <= bm.Height - 3; j++)
                for (int i = 0; i <= bm.Width - 3; i++)
                {
                    for (int x = i; x <= i + 2; x++)
                        for (int y = j; y <= j + 2; y++)
                        {
                            elementList.Add(image[x, y]);
                        }
                    byte[] elements = elementList.ToArray();
                    elementList.Clear();
                    Array.Sort<byte>(elements);
                    Array.Reverse(elements);
                    byte color = elements[4];
                    bm.SetPixel(i + 1, j + 1, Color.FromArgb(color, color, color));
                }
            return bm;
        }
    }
}
