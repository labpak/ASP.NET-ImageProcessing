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
    public class Segment
    {
        public static Bitmap SegmentWords(Bitmap originalBitmap)
        {
            byte[,] arr = GetBitMapMatrix(originalBitmap);   //source array from originalBitmap
            int w = arr.GetLength(0);               //width of 2D array
            int h = arr.GetLength(1);
            List<int> l = new List<int>();
            Image<Gray, Byte> img = originalBitmap.ToImage<Gray, byte>();//Image<Bgr, Byte> imageCV = bmp.ToImage<Bgr, byte>();

            for (int i = 0; i < w; i++)
            {
                int count = 0;
                for (int j = 0; j < h; j++)
                    if (arr[i, j] < 128)
                        count++;
                l.Add(count);
            }

            List<Pair<int, int>> ld2 = new List<Pair<int, int>>();//
            int c = 0, limit = 0;

            //поиск границ строк
            for (int i = 0; i < l.Count(); i = c)
            {
                Pair<int, int> p = new Pair<int, int>();
                if (l[i] > limit)//->начлась новая строка
                {
                    p.First = i;
                    c = i + 1;

                    while (l[c] > limit)
                    {
                        c++;
                        if (c >= l.Count())
                            break;
                    }
                    p.Second = c - 1;
                    ld2.Add(p);
                }
                else
                    c = i + 1;
            }

            //строки
            List<Rectangle> lr = new List<Rectangle>();
            for (int i = 0; i < ld2.Count(); i++)
            {
                Rectangle r = new Rectangle(ld2[i].First, 0, ld2[i].Second - ld2[i].First + 1, h);
                lr.Add(r);
            }

            for (int i = 0; i < lr.Count(); i++)
            {
                Rectangle r = lr[i];
                img.ROI = lr[i];//для вывода отдельных линий 
                CvInvoke.Imshow("roi" + i.ToString(), img);//CvInvoke.Imshow("roi", img);// CvInvoke.Imshow("roi"+i.ToString(), img);
                CvInvoke.Rectangle(img, r, new MCvScalar(90, 0, 255), thickness: 0, lineType: LineType.EightConnected);
            }
            Bitmap res = img.ToBitmap();

            return res;
        }

        public static Bitmap SegmentString(Bitmap originalBitmap)
        {
            byte[,] arr = GetBitMapMatrix(originalBitmap);   //source array from originalBitmap
            int w = arr.GetLength(0);               //width of 2D array
            int h = arr.GetLength(1);
            List<int> l = new List<int>();
            Image<Gray, Byte> img = originalBitmap.ToImage<Gray, byte>();//Image<Bgr, Byte> imageCV = bmp.ToImage<Bgr, byte>();

            for (int i = 0; i < h; i++)
            {
                int count = 0;
                for (int j = 0; j < w; j++)
                    if (arr[j, i] == 0)
                        count++;
                l.Add(count);
            }

            List<Pair<int, int>> ld2 = new List<Pair<int, int>>();//
            int cnt = 0, threshold = 15;

            //поиск границ строк
            for (int i = 0; i < l.Count(); i = cnt)
            {
                Pair<int, int> p = new Pair<int, int>();
                if (l[i] > threshold)//->начлась новая строка
                {
                    p.First = i;
                    cnt = i + 1;

                    while (l[cnt] > threshold)
                    {
                        cnt++;
                        if (cnt >= l.Count())
                            break;
                    }
                    p.Second = cnt - 1;
                    ld2.Add(p);
                }
                else
                    cnt = i + 1;
            }

            //строки
            List<Rectangle> lr = new List<Rectangle>();
            for (int i = 0; i < ld2.Count(); i++)
            {
                Rectangle r = new Rectangle(0, ld2[i].First, w, ld2[i].Second - ld2[i].First + (ld2[i].Second - ld2[i].First) / 2);
                lr.Add(r);
            }

            for (int i = 0; i < lr.Count(); i++)
            {
                Rectangle r = lr[i];
                img.ROI = lr[i];//для вывода отдельных линий 
                CvInvoke.Imshow("roi" + i.ToString(), img);//CvInvoke.Imshow("roi", img);// CvInvoke.Imshow("roi"+i.ToString(), img);
                CvInvoke.Rectangle(img, r, new MCvScalar(90, 0, 255), thickness: 0, lineType: LineType.EightConnected);
            }

            /*using (Graphics gr = Graphics.FromImage(originalBitmap))
            {
                for (int y = 0; y < l.Count(); y++)
                {
                    //scale each bin so that it is drawn 200 pixels wide from the left edge
                    gr.DrawLine(Pens.Black, new PointF(0, y), new PointF(l[y], y));
                }
            }*/


            Bitmap res = img.ToBitmap();

            return res;
        }

        private static byte[,] GetBitMapMatrix(Bitmap bm)
        {
            Bitmap b1 = new Bitmap(bm);

            int hight = b1.Height;
            int width = b1.Width;

            byte[,] Matrix = new byte[width, hight];
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < hight; j++)
                {
                    Matrix[i, j] = (byte)b1.GetPixel(i, j).ToArgb();
                }
            }
            return Matrix;
        }
    }
}

