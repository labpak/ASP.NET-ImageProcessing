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
    //трансформация Хафа
     public class HoughTransform
    {
        public static Bitmap Hough(Bitmap orig, int determination_accuracy)
        {
            Bitmap res = (Bitmap)orig.Clone();
            int r_max = (int)Math.Sqrt(Math.Pow(orig.Width, 2) + Math.Pow(orig.Height, 2)) + 1;//диагональ, самая большая линия
            int q_max = 180;//угол
            int[,] accum = new int[r_max, q_max];//аккум
            int[,] accumDraw = new int[r_max, q_max];

            //Создание пространства Hough. 
            for (int x = 0; x < res.Width; x++)
            {
                for (int y = 0; y < res.Height; y++)
                {
                    if (res.GetPixel(x, y) == Color.FromArgb(0, 0, 0))
                    {
                        for (int j_q = 0; j_q < q_max; j_q++)
                        {
                            int i_r = (int)(x * Math.Cos(j_q * Math.PI / 180) + y * Math.Sin(j_q * Math.PI / 180));
                            if (i_r > 0)
                                accum[i_r, j_q]++; //Draw sinusoids using the example of an accumay (increment the cell where the sinusoid passes)
                        }
                    }
                }
            }

            //поиск локального максимума
            int r = 0, q = 0, curMax = 0;
            for (int k = 0; k <= determination_accuracy; k++)
            {
                for (int i_r = 0; i_r < r_max; i_r++)
                {
                    for (int j_q = 0; j_q < q_max; j_q++)
                    {
                        if (accum[i_r, j_q] > curMax)
                        {
                            curMax = accum[i_r, j_q];
                            r = i_r;
                            q = j_q;
                        }
                    }
                }

                accum[r, q] = 0;
                accumDraw[r, q] = curMax;//Максимумы занесены в отдельный массив
                curMax = 0;

            }


            List<System.Drawing.Point> lis = new List<System.Drawing.Point>();
            // пересчитать в декартову систему координат
            for (int i_r = 0; i_r < r_max; i_r++)
            {
                for (int j_q = 0; j_q < q_max; j_q++)
                {
                    if (accumDraw[i_r, j_q] != 0)
                    {
                        for (int x = 0; x < res.Width; x++)
                        {
                            int y = (int)((i_r - x * Math.Cos(j_q * Math.PI / 180)) / Math.Sin(j_q * Math.PI / 180));
                            if (y > 0 && y < res.Height)
                            {
                                res.SetPixel(x, y, Color.White);//горизонт
                                res.SetPixel(x, y + 1, Color.White);//толще
                                res.SetPixel(x, y - 1, Color.White);//толще
                                //lis.Add(new System.Drawing.Point(x, y));
                            }
                        }
                        for (int y = 0; y < res.Height; y++)
                        {
                            int x = (int)((i_r - y * Math.Sin(j_q * Math.PI / 180)) / Math.Cos(j_q * Math.PI / 180));
                            if (x > 0 && x < res.Width)
                            {
                                res.SetPixel(x, y, Color.White);
                                //lis.Add(new System.Drawing.Point(x, y));
                            }
                        }
                    }
                }
            }

            return res;
        }
    }
}
