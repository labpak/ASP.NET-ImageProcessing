using ImageProcessing.DAL.Interfaces;
using ImageProcessing.Models.Entity;
using ImageProcessing.Models.Enum;
using ImageProcessing.Models.Response;
using Service.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Implementation
{
    public class ImageService : IImageService
    {
        private readonly IImageRepository _imageRepository;

        public ImageService(IImageRepository imageRepository)
        { 
            _imageRepository= imageRepository;
        }
        public async Task<IBaseResponse<IEnumerable<ImageP>>> GetImages()
        {
            var baseResponse = new BaseResponse<IEnumerable<ImageP>>();
            try 
            {
                var images = await _imageRepository.GetAll();
                if (images.Count == 0)
                {
                    baseResponse.Description = "Изображения отсутствуют";
                    baseResponse.StatusCode = StatusCode.OK; 
                    return baseResponse;
                }
                baseResponse.Data = images; 
                baseResponse.StatusCode = StatusCode.OK; 
                return baseResponse;
            }
            catch(Exception ex) 
            {
                return new BaseResponse<IEnumerable<ImageP>>()
                {
                    Description = $"[GetImages] : {ex.Message}"
                };
              
            }
        }
    }
}
