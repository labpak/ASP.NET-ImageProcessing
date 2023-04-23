using ImageProcessing.Models.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageProcessing.DAL.Interfaces
{
    public interface IImageRepository: IBaseRepository<ImageP>
    {
        Task<ImageP> GetByName(string name);
    }
}
