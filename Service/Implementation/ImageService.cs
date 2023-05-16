using ImageProcessing.DAL;
using ImageProcessing.Models.Entity;
using ImageProcessing.Models.Enum;
using ImageProcessing.Models.Response;
using ImageProcessing.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using Service.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace Service.Implementation
{
    public class ImageService : IImageService
    {
        private readonly ApplicationDbContext _db;
        public ImageService(ApplicationDbContext db)
        { 
            _db = db;
        }
        public async Task<IBaseResponse<List<ImageP>>> GetImages()
        {
            try
            {
                var images = await _db.ImageP.ToListAsync();
                if (images.Count == 0)
                {

                    return new BaseResponse<List<ImageP>>()
                    {
                        Description = "Изображения отсутствуют",
                        StatusCode = StatusCode.OK
                    };
                }

                return new BaseResponse<List<ImageP>>()
                {
                    Data = images,
                    StatusCode = StatusCode.OK
                };
            }
            catch (Exception ex)
            {
                return new BaseResponse<List<ImageP>>()
                {
                    Description = $"[GetImages] : {ex.Message}",
                    StatusCode = StatusCode.InternalServerError
                };

            }
        }

        public async Task<IBaseResponse<ImageP>> GetImage(int id)
        {
            var baseResponse = new BaseResponse<ImageP>();
            try 
            {
                var image = await _db.ImageP.FirstOrDefaultAsync(p => p.Id == id);
                if (image == null)
                {
                    baseResponse.Description = "Изображение отсутствует";
                    baseResponse.StatusCode = StatusCode.ImageNotFound;
                    return baseResponse;
                }
                baseResponse.Data = image;
                baseResponse.StatusCode = StatusCode.OK;
                return baseResponse;
            }
            catch (Exception ex) 
            {
                return new BaseResponse<ImageP> ()
                {
                    Description = $"[GetImages] : {ex.Message}",
                    StatusCode = StatusCode.InternalServerError
                };
            }
        }

        public async Task<IBaseResponse<ImageP>> GetByName(string name)
        {            
            var baseResponse = new BaseResponse<ImageP>();
            try
            {
                //var image = await _imageRepository.GetByName(name);
                var image = await _db.ImageP.FirstOrDefaultAsync(p => p.Name == name);
                if (image == null)
                {
                    baseResponse.Description = "Изображение отсутствует";
                    baseResponse.StatusCode = StatusCode.ImageNotFound;
                    return baseResponse;
                }
                baseResponse.Data = image;
                baseResponse.StatusCode = StatusCode.OK;
                return baseResponse;
            }
            catch (Exception ex)
            {
                return new BaseResponse<ImageP>()
                {
                    Description = $"[GetImages] : {ex.Message}",
                    StatusCode = StatusCode.InternalServerError
                };
            }
        }
        public async Task<IBaseResponse<bool>> DeleteImage(int id)
        {
            var baseResponse = new BaseResponse<bool>();
            var entity = await _db.ImageP.FirstOrDefaultAsync(p => p.Id == id);
            try
            {
                if (entity == null)
                {
                    baseResponse.Description = "Изображение отсутствует";
                    baseResponse.StatusCode = StatusCode.ImageNotFound;
                    baseResponse.Data = false;
                    return baseResponse;             
                }
                else
                {
                    _db.ImageP.Remove(entity);
                    await _db.SaveChangesAsync();
                    baseResponse.StatusCode = StatusCode.OK;
                    baseResponse.Data = true;
                    return baseResponse;
                }
            }
            catch (Exception ex)
            {
                return new BaseResponse<bool>()
                {
                    Data = false,
                    Description = $"[GetImages] : {ex.Message}",
                    StatusCode = StatusCode.InternalServerError
                };
            }
        }
        public async Task<IBaseResponse<bool>> CreateImage(ImageViewModel model, byte[] imageData)
        {
           //byte[] ewq = { 1,2,3};
            var baseResponse = new BaseResponse<bool>();
            try
            {
                var image = new ImageP()
                {
                    Id = model.Id,
                    DateCreate = DateTime.Now,
                    Description = model.Description,
                    Name = model.Name,
                    //TypeImage = (TypeImage)Convert.ToInt32(model.TypeImage),
                    //Width = model.Width,
                    //Height = model.Height,
                    Image = imageData
                };

                await _db.ImageP.AddAsync(image);
                await _db.SaveChangesAsync();

                baseResponse.Data = true;
                baseResponse.StatusCode = StatusCode.OK;
                return baseResponse;
            }
            catch (Exception ex)
            {
                return new BaseResponse<bool>()
                {
                    Data = false,
                    Description = $"[GetImages] : {ex.Message}",
                    StatusCode = StatusCode.InternalServerError
                };
            }
        }

        public async Task<IBaseResponse<ImageP>> Edit(int id, ImageViewModel model)
        {
            var baseResponse = new BaseResponse<ImageP>();
            try
            {
                var image = await _db.ImageP.FirstOrDefaultAsync(p => p.Id == id);
                if (image == null)
                {
                    baseResponse.StatusCode = StatusCode.ImageNotFound;
                    baseResponse.Description = "Image not found";
                    return baseResponse;
                }

                image.DateCreate = DateTime.Now;
                image.Description = model.Description;
                image.Name = model.Name;
                //image.TypeImage = (TypeImage)Convert.ToInt32(model.TypeImage);
                //image.Width = model.Width;
                //image.Height = model.Height;

                _db.ImageP.Update(image);
                await _db.SaveChangesAsync();

                baseResponse.Data = image;
                return baseResponse;
            }
            catch (Exception ex) 
            {
                return new BaseResponse<ImageP>()
                {
                    Description = $"[GetImages] : {ex.Message}",
                    StatusCode = StatusCode.InternalServerError
                };
            }
        }
    }
}
