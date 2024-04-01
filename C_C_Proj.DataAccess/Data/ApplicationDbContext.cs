
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
        public DbSet<ShoppingCard> ShoppingCards { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<OrderHeader> OrderHeaders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Run", DisplayOrder = 1 },
            new Category { Id = 2, Name = "Chill", DisplayOrder = 2 },
            new Category { Id = 3, Name = "New", DisplayOrder = 3 }
            );

            modelBuilder.Entity<Company>().HasData(
                new Company
                {
                Id = 1,
                Name = "Nike",
                StreetAddress = "123 Main St",
                City = "New York",
                Country = "USA",
                PostalCode = "10001",
                PhoneNumber = "1234567890",
                Email = "nike.com"
                },
                new Company
                {
                    Id = 2,
                    Name = "Adidas",
                    StreetAddress = "456 Main St",
                    City = "Berlin",
                    Country = "Germany",
                    PostalCode = "10001123",
                    PhoneNumber = "23451324",
                    Email = "abibas.com"
                },
                new Company
                {
                    Id = 3,
                    Name = "Boss",
                    StreetAddress = "789 Main St",
                    City = "Milan",
                    Country = "Itali",
                    PostalCode = "5671123",
                    PhoneNumber = "29999222224",
                    Email = "biboss.com"
                }

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
                Gender = "Male",
                CategoryId = 1,
                AgeGroup = "Adult",
                StockCount = 100
                
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
                Gender = "Male",
                CategoryId = 2,
                AgeGroup = "Kid",
                StockCount = 200
                
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
                Gender = "Female",
                CategoryId = 3,
                AgeGroup = "Adult",
                StockCount = 300
                
            }
            );
        }
    }
}
