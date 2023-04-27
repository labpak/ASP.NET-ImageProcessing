using ImageProcessing.Models.Enum;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageProcessing.Models.ViewModels
{
    public class ImageViewModel//класс для отображения, не для работы
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        //public string TypeImage { get; set; }
        public DateTime DateCreate { get; set; }
        public byte[] Image { get; set; }
        public IFormFile formFile { get; set; }
    }
}
