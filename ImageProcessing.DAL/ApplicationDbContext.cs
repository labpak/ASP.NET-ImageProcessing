using ImageProcessing.Models.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageProcessing.DAL
{
    public class ApplicationDbContext: DbContext //содержит все компоненты для работы с бд
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
         {
            //Database.EnsureDeleted();
            Database.EnsureCreated();//создаст бд если ее нет
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {          
            optionsBuilder.UseSqlServer("Server=MANUL\\SQLEXPRESS;Database=ImageProc;Trusted_Connection=True;TrustServerCertificate=True");
        }
        public DbSet<ImageP> ImageP { get; set; } // для получения даных из бд
    }   
}
