using AForge.Imaging;
using AForge.Imaging.Filters;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Service.Interfaces;
using System.Drawing;

namespace Service.Implementation
{
    public class ImageProcessingService: IImageProcessingService
    {
        public ImageProcessingService() { }

        public System.Drawing.Bitmap FinalImage(System.Drawing.Bitmap image)
        {
            Invert InvertFilter;
            InvertFilter = new AForge.Imaging.Filters.Invert();

            Bitmap OrigImage = (Bitmap)image;

            //ФИЛЬТР МЕДИАННЫЙ
            Bitmap mfiltrImage = MedianFilter.MFilter(OrigImage);
            Bitmap binImage = ConvertHelp.Binarization(mfiltrImage);


            //СКЕЛЕТ Зонг-Сунь
            bool[][] zst = SkeletBuild.ZhangSuenThinning(ConvertHelp.Image2Bool(binImage));
            Bitmap thinImage = (Bitmap)ConvertHelp.Bool2Image(zst);
            binImage = ConvertHelp.Binarization(thinImage);


            ////AFORGE СКЕЛЕТ
            //InvertFilter.ApplyInPlace(binImage);
            //SimpleSkeletonization tfilter = new SimpleSkeletonization();
            //// apply the filter
            //tfilter.ApplyInPlace(binImage);
            //InvertFilter.ApplyInPlace(binImage);
            //binImage = ConvertHelp.Binarization(binImage);


            ////СКЕЛЕТ EMGUCV           
            //Bitmap skImage = SkeletBuild.SkelatanizeEmgu(binImage);
            //binImage = ConvertHelp.Binarization(skImage);
            //InvertFilter.ApplyInPlace(binImage);


            //УДАЛЕНИЕ ШУМОВ
            Bitmap rn = RemoveArtifact.RemoveNoiseAforge(binImage, 20, 10);
            binImage = ConvertHelp.Binarization(rn);

            Bitmap ni = ConvertHelp.CreateNonIndexedImage(binImage);
            Bitmap withoutlines = HoughTransform.Hough(ni, 5);//0 to 500
            binImage = ConvertHelp.Binarization(withoutlines);

            //УДАЛЕНИЕ ШУМОВ
            rn = RemoveArtifact.RemoveNoiseAforge(binImage, 3, 3);
            binImage = ConvertHelp.Binarization(rn);

            /*Bitmap sS = Segment.SegmentString(ni);
            binImage = Binarization(sS);
            Bitmap sW = Segment.SegmentWords(binImage);*/

            image = binImage;
            return binImage;
        }
    }

    internal class ConvertHelp
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

    internal class MedianFilter
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

    internal class RemoveArtifact
    {
        public static Bitmap RemoveNoiseWithAforge(Bitmap src, int width, int height)
        {
            Invert InvertFilter;
            InvertFilter = new AForge.Imaging.Filters.Invert();
            InvertFilter.ApplyInPlace(src);
            Image<Gray, Byte> imageCV = src.ToImage<Gray, byte>();//Image<Bgr, Byte> imageCV = bmp.ToImage<Bgr, byte>();
            Mat srcM = imageCV.Mat;
            BlobCounter bc = new BlobCounter();
            bc.ProcessImage(src);
            Blob[] blobs = bc.GetObjectsInformation();

            foreach (var b in blobs)
            {
                if (b.Rectangle.Width < width || b.Rectangle.Height < height)
                    CvInvoke.Rectangle(srcM, b.Rectangle, new MCvScalar(0, 0, 0), thickness: -1, lineType: LineType.EightConnected);
            }
            Bitmap res = srcM.ToBitmap();
            InvertFilter.ApplyInPlace(res);
            return res;
        }

        public static Bitmap RemoveNoiseAforge(Bitmap bmp, int w, int h)
        {
            Invert InvertFilter;
            InvertFilter = new AForge.Imaging.Filters.Invert();
            InvertFilter.ApplyInPlace(bmp);
            // create filter
            BlobsFiltering filter = new BlobsFiltering();
            // configure filter
            filter.CoupledSizeFiltering = true;
            filter.MinWidth = w;
            filter.MinHeight = h;
            // apply the filter
            filter.ApplyInPlace(bmp);
            InvertFilter.ApplyInPlace(bmp);
            return bmp;
        }

        private static Bitmap RemoveLines(Bitmap bmp)
        {
            Image<Gray, Byte> imageCV = bmp.ToImage<Gray, byte>();//Image<Bgr, Byte> imageCV = bmp.ToImage<Bgr, byte>();
            Mat src = imageCV.Mat;

            Mat gray = new Mat();
            if (imageCV.NumberOfChannels == 3)
                CvInvoke.CvtColor(src, gray, Emgu.CV.CvEnum.ColorConversion.Rgb2Gray);
            else
                gray = src;

            // Apply adaptiveThreshold at the bitwise_not of gray, notice the ~ symbol
            Mat bw = new Mat();
            CvInvoke.AdaptiveThreshold(~gray, bw, 255, Emgu.CV.CvEnum.AdaptiveThresholdType.MeanC, Emgu.CV.CvEnum.ThresholdType.Binary, 15, -2);
            Mat fin = bw.Clone();
            // Show binary image
            CvInvoke.Imshow("binary", bw);
            // Create the images that will use to extract the horizontal and vertical lines
            Mat horizontal = bw.Clone();
            Mat vertical = bw.Clone();
            fin = ~bw;
            // Specify size on horizontal axis
            int horizontalsize = horizontal.Cols / 30;
            // Create structure element for extracting horizontal lines through morphology operations
            Mat horizontalKernel = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(horizontalsize, 1), new System.Drawing.Point(-1, -1));
            // Apply morphology operations
            CvInvoke.Erode(horizontal, horizontal, horizontalKernel, new System.Drawing.Point(-1, -1), 1, BorderType.Default, new MCvScalar(255, 255, 255));
            //horizontal = eroseMat(horizontal.ToBitmap(), horizontalKernel);
            CvInvoke.Imshow("erode", horizontal);
            CvInvoke.Dilate(horizontal, horizontal, horizontalKernel, new System.Drawing.Point(-1, -1), 1, BorderType.Default, new MCvScalar(255, 255, 255));
            //horizontal = dilateMat(horizontal.ToBitmap(), horizontalKernel);
            CvInvoke.Imshow("horizontal_lines", horizontal);

            //Need to find horizontal contours, so as to not damage letters
            Mat hierarchy = new Mat();
            Emgu.CV.Util.VectorOfVectorOfPoint contours = new Emgu.CV.Util.VectorOfVectorOfPoint();
            CvInvoke.FindContours(horizontal, contours, hierarchy, RetrType.Tree, ChainApproxMethod.ChainApproxNone);
            int contCount = contours.Size;
            for (int i = 0; i < contCount; i++)
            {

                Rectangle r = CvInvoke.BoundingRectangle(contours[i]);
                float percentage_height = (float)r.Height / (float)src.Rows;
                float percentage_width = (float)r.Width / (float)src.Cols;

                //These exclude contours that probably are not dividing lines
                if (percentage_height > 0.10)
                    continue;

                if (percentage_width < 0.15)//если ширина меньше 5%, то не удаляем
                    continue;
                //fills in line with white rectange
                CvInvoke.Rectangle(fin, r, new MCvScalar(255, 255, 255), thickness: -1, lineType: LineType.EightConnected);
            }

            int verticalsize = vertical.Rows / 30;
            Mat verticalKernel = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(1, verticalsize), new System.Drawing.Point(-1, -1));
            CvInvoke.Erode(vertical, vertical, verticalKernel, new System.Drawing.Point(-1, -1), 1, BorderType.Default, new MCvScalar(255, 255, 255));
            //vertical = eroseMat(horizontal.ToBitmap(), horizontalKernel);
            CvInvoke.Dilate(vertical, vertical, verticalKernel, new System.Drawing.Point(-1, -1), 1, BorderType.Default, new MCvScalar(255, 255, 255));
            //vertical = dilateMat(horizontal.ToBitmap(), horizontalKernel);
            CvInvoke.Imshow("verticalal", vertical);

            CvInvoke.FindContours(vertical, contours, hierarchy, RetrType.Tree, ChainApproxMethod.ChainApproxNone);
            contCount = contours.Size;
            for (int i = 0; i < contCount; i++)
            {
                Rectangle r = CvInvoke.BoundingRectangle(contours[i]);

                float percentage_height = (float)r.Height / (float)src.Rows;
                float percentage_width = (float)r.Width / (float)src.Cols;

                //These exclude contours that probably are not dividing lines
                if (percentage_width > 0.10)
                    continue;

                if (percentage_height < 0.15)// больше лучше
                    continue;
                //fills in line with white rectange
                CvInvoke.Rectangle(fin, r, new MCvScalar(255, 255, 255), thickness: -1, lineType: LineType.EightConnected);
            }

            Bitmap res = fin.ToBitmap();
            return res;
        }

    }

    internal class Segment
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

    internal class SkeletBuild
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

    //трансформация Хафа
    internal class HoughTransform
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

    public class Pair<T, U>
    {
        public Pair()
        {
        }

        public Pair(T first, U second)
        {
            this.First = first;
            this.Second = second;
        }

        public T First { get; set; }
        public U Second { get; set; }
    }
}
