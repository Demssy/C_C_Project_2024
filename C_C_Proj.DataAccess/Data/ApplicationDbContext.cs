
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
        public DbSet<UserCreditCard> UserCreditCards { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Sneakers", DisplayOrder = 1 },
            new Category { Id = 2, Name = "Boots", DisplayOrder = 2 },
            new Category { Id = 3, Name = "Heels", DisplayOrder = 3 },
            new Category { Id = 4, Name = "Flats", DisplayOrder = 4 },
            new Category { Id = 5, Name = "Oxfords", DisplayOrder = 5 },
            new Category { Id = 6, Name = "Slippers", DisplayOrder = 6 },
            new Category { Id = 7, Name = "Boat Shoes", DisplayOrder = 7 },
            new Category { Id = 8, Name = "Sandals", DisplayOrder = 8 },
            new Category { Id = 9, Name = "Athletic Shoes", DisplayOrder = 9 }
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
                StockCount = 100,
                StockStatus = "AvailableInStock",
                PurchasesCount = 0,
                Discount = 0
                
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
                StockCount = 200,
                StockStatus = "AvailableInStock",
                PurchasesCount = 0,
                Discount = 0

            },
            new Product
            {
                Id = 3,
                ShoeModel = "Wild Horse",
                Brand = "Reebok",
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
                StockCount = 300,
                StockStatus = "AvailableInStock",
                PurchasesCount = 0,
                Discount = 0

            },
            new Product
            {
                Id = 4,
                ShoeModel = "Cons",
                Brand = "Converse",
                ListPrice = 120,
                Price = 33,
                Price50 = 22,
                Price100 = 11,
                Description = "Description3",
                Size = 38,
                Color = "White",
                Gender = "Unisex",
                CategoryId = 5,
                AgeGroup = "Adult",
                StockCount = 20,
                StockStatus = "AvailableInStock",
                PurchasesCount = 0,
                Discount = 0

            },
            new Product
            {
                Id = 5,
                ShoeModel = "Air Force",
                Brand = "Jordan",
                ListPrice = 500,
                Price = 299,
                Price50 = 199,
                Price100 = 99,
                Description = "Description3",
                Size = 43,
                Color = "Blue",
                Gender = "Female",
                CategoryId = 8,
                AgeGroup = "Kid",
                StockCount = 300,
                StockStatus = "AvailableInStock",
                PurchasesCount = 0,
                Discount = 0

            },
            new Product
            {
                Id = 6,
                ShoeModel = "Vans x Harry Potter",
                Brand = "Vans",
                ListPrice = 999,
                Price = 900,
                Price50 = 559,
                Price100 = 399,
                Description = "Description3",
                Size = 38,
                Color = "Black",
                Gender = "Male",
                CategoryId = 4,
                AgeGroup = "Adult",
                StockCount = 300,
                StockStatus = "AvailableInStock",
                PurchasesCount = 0,
                Discount = 0

            },
            new Product
            {
                Id = 7,
                ShoeModel = "Classic Clog",
                Brand = "Crocs",
                ListPrice = 22,
                Price = 21,
                Price50 = 15,
                Price100 = 11,
                Description = "Description3",
                Size = 38,
                Color = "Red",
                Gender = "Female",
                CategoryId = 7,
                AgeGroup = "Adult",
                StockCount = 300,
                StockStatus = "AvailableInStock",
                PurchasesCount = 0,
                Discount = 0
            },
            new Product
            {
                Id = 8,
                ShoeModel = "x Aimé Leon Dore",
                Brand = "New Balance",
                ListPrice = 356,
                Price = 299,
                Price50 = 199,
                Price100 = 99,
                Description = "Description3",
                Size = 46,
                Color = "Purple",
                Gender = "Female",
                CategoryId = 2,
                AgeGroup = "Adult",
                StockCount = 300,
                StockStatus = "AvailableInStock",
                PurchasesCount = 0,
                Discount = 0
            }
            );
        }
    }
}
