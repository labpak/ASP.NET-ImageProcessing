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
    public class SkeletBuild
    {
        //Скелетизация Зонга-Суня
        public static bool[][] ZhangSuenThinning(bool[][] s)
        {
            bool[][] temp = ArrayClone(s);
            int count = 0;
            do
            {
                count = Step(1, temp, s);
                temp = ArrayClone(s);
                count += Step(2, temp, s);
                temp = ArrayClone(s);
            }
            while (count > 0);

            return s;
        }

        private static int Step(int stepNo, bool[][] temp, bool[][] s)
        {
            int count = 0;

            for (int i = 1; i < temp.Length - 1; i++)
            {
                for (int j = 1; j < temp[0].Length - 1; j++)
                {
                    if (SuenThinningAlg(i, j, temp, stepNo == 2))
                    {
                        // если изменения происходят
                        if (s[i][j]) count++;
                        s[i][j] = false;
                    }
                }
            }
            return count;
        }

        private static bool SuenThinningAlg(int x, int y, bool[][] s, bool even)
        {
            bool p2 = s[x][y - 1];
            bool p3 = s[x + 1][y - 1];
            bool p4 = s[x + 1][y];
            bool p5 = s[x + 1][y + 1];
            bool p6 = s[x][y + 1];
            bool p7 = s[x - 1][y + 1];
            bool p8 = s[x - 1][y];
            bool p9 = s[x - 1][y - 1];


            int bp1 = NumberOfNonZeroNeighbors(x, y, s);
            if (bp1 >= 2 && bp1 <= 6) //2nd условие
            {
                if (NumberOfZeroToOneTransitionFromP9(x, y, s) == 1)
                {
                    if (even)
                    {
                        if (!((p2 && p4) && p8))
                        {
                            if (!((p2 && p6) && p8))
                            {
                                return true;
                            }
                        }
                    }
                    else
                    {
                        if (!((p2 && p4) && p6))
                        {
                            if (!((p4 && p6) && p8))
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        private static int NumberOfZeroToOneTransitionFromP9(int x, int y, bool[][] s)
        {
            bool p2 = s[x][y - 1];
            bool p3 = s[x + 1][y - 1];
            bool p4 = s[x + 1][y];
            bool p5 = s[x + 1][y + 1];
            bool p6 = s[x][y + 1];
            bool p7 = s[x - 1][y + 1];
            bool p8 = s[x - 1][y];
            bool p9 = s[x - 1][y - 1];

            int A = Convert.ToInt32((!p2 && p3)) + Convert.ToInt32((!p3 && p4)) +
                    Convert.ToInt32((!p4 && p5)) + Convert.ToInt32((!p5 && p6)) +
                    Convert.ToInt32((!p6 && p7)) + Convert.ToInt32((!p7 && p8)) +
                    Convert.ToInt32((!p8 && p9)) + Convert.ToInt32((!p9 && p2));
            return A;
        }
         private static int NumberOfNonZeroNeighbors(int x, int y, bool[][] s)
        {
            int count = 0;
            if (s[x - 1][y]) count++;
            if (s[x - 1][y + 1]) count++;
            if (s[x - 1][y - 1]) count++;
            if (s[x][y + 1]) count++;
            if (s[x][y - 1]) count++;
            if (s[x + 1][y]) count++;
            if (s[x + 1][y + 1]) count++;
            if (s[x + 1][y - 1]) count++;
            return count;
        }

        private static T[][] ArrayClone<T>(T[][] A)
        { return A.Select(a => a.ToArray()).ToArray(); }


        //Построение скелета с помощью библиотеки Emgu
        public static Bitmap SkelatanizeEmgu(Bitmap image)
        {
            Image<Gray, Byte> imgOld = image.ToImage<Gray, byte>();
            Image<Gray, byte> img2 = (new Image<Gray, byte>(imgOld.Width, imgOld.Height, new Gray(255))).Sub(imgOld);
            Image<Gray, byte> eroded = new Image<Gray, byte>(img2.Size);
            Image<Gray, byte> temp = new Image<Gray, byte>(img2.Size);
            Image<Gray, byte> skel = new Image<Gray, byte>(img2.Size);
            skel.SetValue(0);
            CvInvoke.Threshold(img2, img2, 127, 256, 0);
            var element = CvInvoke.GetStructuringElement(ElementShape.Cross, new Size(3, 3), new System.Drawing.Point(-1, -1));
            bool done = false;

            while (!done)
            {
                CvInvoke.Erode(img2, eroded, element, new System.Drawing.Point(-1, -1), 1, BorderType.Reflect, default(MCvScalar));
                CvInvoke.Dilate(eroded, temp, element, new System.Drawing.Point(-1, -1), 1, BorderType.Reflect, default(MCvScalar));
                CvInvoke.Subtract(img2, temp, temp);
                CvInvoke.BitwiseOr(skel, temp, skel);
                eroded.CopyTo(img2);
                if (CvInvoke.CountNonZero(img2) == 0) done = true;
            }
            return skel.ToBitmap();
        }
    }
}
