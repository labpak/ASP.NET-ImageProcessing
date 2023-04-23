using ImageProcessing.Models.Entity;
using ImageProcessing.Models.Response;
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
    }
}
