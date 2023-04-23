using ImageProcessing.Models.Entity;
using ImageProcessing.Models.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageProcessing.Models.Response
{
    public interface IBaseResponse<T>
    {
        StatusCode StatusCode { get; }
        public T Data { get; }
    }
    public class BaseResponse<T>: IBaseResponse<T>
    {
        public string Description { get; set; }//сюда ошибки будут записываться или предупреждения
        public StatusCode StatusCode { get; set; }
        public T Data { get; set; }
    }
}
