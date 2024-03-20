
using C_C_Proj_WebStore.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace C_C_Proj_WebStore.DataAccess.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<ApplicationUser> ApplicationUsers {  get; set; }

        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Run", DisplayOrder = 1 },
            new Category { Id = 2, Name = "Chill", DisplayOrder = 2 },
            new Category { Id = 3, Name = "New", DisplayOrder = 3 }
            );

            modelBuilder.Entity<Product>().HasData(
            new Product
            {
                Id = 1,
                ShoeModel = "Pegasus",
                Brand = "Nike",
                ListPrice = 100,
                Price = 95,
                Price50 = 90,
                Price100 = 80,
                Description = "Description1",
                Size = 41.5,
                Color = "White",
                Gender = "M",
                CategoryId = 1,
                ImageUrl = ""
            },
            new Product
            {
                Id = 2,
                ShoeModel = "Easy",
                Brand = "Adidas",
                ListPrice = 100,
                Price = 95,
                Price50 = 90,
                Price100 = 80,
                Description = "Description2",
                Size = 45.5,
                Color = "Black",
                Gender = "M",
                CategoryId = 2,
                ImageUrl = ""
            },
            new Product
            {
                Id = 3,
                ShoeModel = "Wild Horse",
                Brand = "Reebock",
                ListPrice = 100,
                Price = 95,
                Price50 = 90,
                Price100 = 80,
                Description = "Description3",
                Size = 38,
                Color = "Blue",
                Gender = "F",
                CategoryId = 4,
                ImageUrl = ""
            }
            );
        }
    }
}
