using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageProcessing.DAL.Interfaces
{
    public interface IBaseRepository<T>//generic(<T>) т.к. разные типылолочевидно
    {
        Task<bool> Create (T entity);//создано 1, inache 0
        Task<T> Get(int id);
        Task<List<T>> GetAll();

        Task<bool> Delete(T entity);
    }
}
