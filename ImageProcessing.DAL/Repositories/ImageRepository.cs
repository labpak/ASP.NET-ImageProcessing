using ImageProcessing.DAL.Interfaces;
using ImageProcessing.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageProcessing.DAL.Repositories
{
    public class ImageRepository : IImageRepository
    {
        private readonly ApplicationDbContext _db;

        public ImageRepository(ApplicationDbContext db) 
        { 
            _db = db;
        }
        public async Task<bool> Create(ImageP entity)
        {
            await _db.ImageP.AddAsync(entity);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> Delete(ImageP entity)
        {
            _db.ImageP.Remove(entity);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<ImageP> Get(int id)
        {
            return await _db.ImageP.FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<List<ImageP>> GetAll()
        {
            return await _db.ImageP.ToListAsync();//асинхронный, т.к. из-за запроса к бд подвиснет сайт и чтобы сайт не лежал нужно не синхронизировать
        }

        public async Task<ImageP> GetByName(string name)
        {
            return await _db.ImageP.FirstOrDefaultAsync(p => p.Name == name);
        }
    }
}
