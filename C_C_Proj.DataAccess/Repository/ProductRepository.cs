using C_C_Proj_WebStore.DataAccess.Data;
using C_C_Proj_WebStore.DataAccess.Repository.IRepository;
using C_C_Proj_WebStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace C_C_Proj_WebStore.DataAccess.Repository
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private ApplicationDbContext _db;
        public ProductRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Product product)
        {
            var objFromDb = _db.Products.FirstOrDefault(s => s.Id == product.Id);
            if (objFromDb != null)
            {
                /*if (product.ImageUrl != null)
                {
                    objFromDb.ImageUrl = product.ImageUrl;
                }*/
                objFromDb.ShoeModel = product.ShoeModel;
                objFromDb.Brand = product.Brand;
                objFromDb.Price = product.Price;
                objFromDb.Description = product.Description;
                objFromDb.Size = product.Size;
                objFromDb.Color = product.Color;
                objFromDb.ListPrice = product.ListPrice;
                objFromDb.Price50 = product.Price50;
                objFromDb.Price100 = product.Price100;
                objFromDb.ProductImages = product.ProductImages;
                objFromDb.CategoryId = product.CategoryId;
                objFromDb.Gender = product.Gender;
                objFromDb.AgeGroup = product.AgeGroup;
                
            }
        }
    }
}
