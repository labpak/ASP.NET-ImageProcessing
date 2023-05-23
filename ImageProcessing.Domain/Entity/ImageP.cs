using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageProcessing.Models.Enum;
using System.Drawing;

namespace ImageProcessing.Models.Entity
{
    public class ImageP
    {
        public int Id { get; set; }
        public int UserImageId { get; set; }//изменить на userId
        public string Name { get; set; }
        public string Description { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string TypeImage { get; set; }
        public DateTime DateCreate { get; set; }
        public byte[] Image { get; set; }
    }
}
