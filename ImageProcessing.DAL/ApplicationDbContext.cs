using ImageProcessing.Models.Entity;
using ImageProcessing.Models.Helpers;
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

        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(builder =>
            {
                builder.HasData(new User
                {
                    Id = 1,
                    Name = "admin",
                    Password = HashPasswordHelper.HashPassowrd("admin"),
                    Role = Models.Enum.Role.Admin
                });

                builder.ToTable("Users").HasKey(x => x.Id);
                builder.Property(x => x.Id).ValueGeneratedOnAdd();
                builder.Property(x => x.Password).IsRequired();
                builder.Property(x => x.Name).HasMaxLength(128).IsRequired();
            });
            
            modelBuilder.Entity<ImageP>(builder =>
            {
                builder.ToTable("ImageP").HasKey(x => x.Id);
                builder.Property(x => x.Id).ValueGeneratedOnAdd();
            });
        }
    }   
}
