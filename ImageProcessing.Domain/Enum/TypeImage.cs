using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageProcessing.Domain.Enum
{
    public enum TypeImage
    {
        [Display(Name = ".jpg")]
        jpg = 0,
        [Display(Name = ".png")]
        png = 1,
        [Display(Name = ".tif")]
        tif =2,
        [Display(Name = ".svg")]
        svg = 3,
        [Display(Name = ".raw")]
        raw = 4,
        [Display(Name = ".bmp")]
        bmp = 5
    }
}
    