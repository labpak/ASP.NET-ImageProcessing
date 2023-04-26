using ImageProcessing.Models.Entity;
using ImageProcessing.Models.Response;
using ImageProcessing.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Interfaces
{
    public interface IImageService
    {
        Task<IBaseResponse<IEnumerable<ImageP>>> GetImages();
        Task<IBaseResponse<ImageP>> GetImage(int id);
        Task<IBaseResponse<ImageP>> GetByName(string name);
        Task<IBaseResponse<bool>> DeleteImage(int id);
        Task<IBaseResponse<bool>> CreateImage(ImageViewModel model, byte[] imageData);
        Task<IBaseResponse<ImageP>> Edit(int id, ImageViewModel model);
    }
}
