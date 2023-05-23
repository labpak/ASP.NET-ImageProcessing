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
    public class RemoveArtifact
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
}
