using AForge.Imaging;
using AForge.Imaging.Filters;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Service.Interfaces;
using System.Drawing;
using ImageProcessing.ImageProcessingMethods;

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
}
