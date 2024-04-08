using C_C_Proj_WebStore.DataAccess.Data;
using C_C_Proj_WebStore.DataAccess.Repository.IRepository;
using C_C_Proj_WebStore.Models;
using C_C_Proj_WebStore.Models.ViewModels;
using C_C_Proj_WebStore.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using System.Collections.Generic;
using System.Security.Claims;

namespace C_C_Proj_WebStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            List<Product> objProductList = _unitOfWork.Product.GetAll(includeProperties: "Category").ToList();

            return View(objProductList);
        }


        public IActionResult Warehouse(int? id)
        {
            Product? productToBeOrdered = _unitOfWork.Product.Get(u => u.Id == id, includeProperties: "Category");
            if (productToBeOrdered == null)
            {
                return NotFound();
            }
            productToBeOrdered.ProductImages = _unitOfWork.ProductImage.GetAll(u => u.ProductId == id).ToList();
            return View(productToBeOrdered);
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult UpdateProductStock2(Product product)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            Product productFromDb = _unitOfWork.Product.Get(u => u.Id == product.Id, includeProperties: "Category");
            productFromDb.StockStatus = SD.AvailableInStock;
            productFromDb.StockCount += product.StockCount;
            _unitOfWork.Product.Update(productFromDb);
            
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index), new { id = productFromDb.Id });
        }

        public IActionResult Upsert(int? id)
        {
            ProductVM productVM = new()
            {
                Product = new Product(),

                AgeGroupList = new List<SelectListItem>
                {
                    new SelectListItem { Text = "Adult", Value = "Adult" },
                    new SelectListItem { Text = "Kid", Value = "Kid" }
                },
                GenderList = new List<SelectListItem>
                {
                    new SelectListItem {Text = "Male", Value ="Male"},
                    new SelectListItem {Text = "Female", Value = "Female"},
                    new SelectListItem {Text = "Unisex", Value = "Unisex"}
                },
                CategoryList = _unitOfWork.Category.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                })
            };
            if (id == null || id == 0)
            {
                //create
                return View(productVM);
            }
            else
            {
                //update
                productVM.Product = _unitOfWork.Product.Get(u => u.Id == id, includeProperties: "ProductImages");
                return View(productVM);
            }
        }
        [HttpPost]
        public IActionResult Upsert(ProductVM productVM, List<IFormFile> files)
        {
            if (ModelState.IsValid)
            {

                if (productVM.Product.Id != 0)
                {
                    if(productVM.Product.StockCount <= 0)
                    {
                        productVM.Product.StockStatus = SD.OutOfStock;
                    }
                    else
                    {
                        productVM.Product.StockStatus = SD.AvailableInStock;
                    }
                    _unitOfWork.Product.Update(productVM.Product);
                }
                else
                {
                    if (productVM.Product.StockCount <= 0)
                    {
                        productVM.Product.StockStatus = SD.OutOfStock;
                    }
                    else
                    {
                        productVM.Product.StockStatus = SD.AvailableInStock;
                    }
                    _unitOfWork.Product.Add(productVM.Product);
                }
                _unitOfWork.Save();


                string wwwRootPath = _webHostEnvironment.WebRootPath;
                if (files != null)
                {

                    foreach (IFormFile file in files)
                    {
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        string productPath = @"image\products\product-" + productVM.Product.Id;
                        string finalPath = Path.Combine(wwwRootPath, productPath);

                        if (!Directory.Exists(finalPath))
                            Directory.CreateDirectory(finalPath);

                        using (var fileStream = new FileStream(Path.Combine(finalPath, fileName), FileMode.Create))
                        {
                            file.CopyTo(fileStream);
                        }

                        ProductImage productImage = new()
                        {
                            ImageUrl = @"\" + productPath + @"\" + fileName,
                            ProductId = productVM.Product.Id
                        };

                        if (productVM.Product.ProductImages == null)
                            productVM.Product.ProductImages = new List<ProductImage>();

                        productVM.Product.ProductImages.Add(productImage);
                        _unitOfWork.ProductImage.Add(productImage);
                        _unitOfWork.Save();
                    }

                    //_unitOfWork.Product.Update(productVM.Product);
                    //_unitOfWork.Save();
                }

                TempData["success"] = "Product has been created/updated successfully";
                return RedirectToAction("Index");
            }
            else
            {
                productVM.CategoryList = _unitOfWork.Category.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                });
                return View(productVM);
            }
        }







        #region API CALLS


        [HttpGet]
        public IActionResult GetAll()
        {
            List<Product> objProductList = _unitOfWork.Product.GetAll(includeProperties: "Category").ToList();
            return Json(new { data = objProductList });
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var productToBeDeleted = _unitOfWork.Product.Get(u => u.Id == id);
            if (productToBeDeleted == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }
            /* var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, productToBeDeleted.ImageUrl.TrimStart('\\'));*/
            /* if (System.IO.File.Exists(oldImagePath))
             {
                 System.IO.File.Delete(oldImagePath);
             }*/

            string productPath = @"image\products\product-" + id;
            string finalPath = Path.Combine(_webHostEnvironment.WebRootPath, productPath);

            if (!Directory.Exists(finalPath))
            {
                string[] filePaths = Directory.GetFiles(finalPath);
                foreach (string filePath in filePaths)
                {
                    System.IO.File.Delete(filePath);
                }

                Directory.CreateDirectory(finalPath);
            }



            _unitOfWork.Product.Remove(productToBeDeleted);
            _unitOfWork.Save();

            return Json(new { success = true, message = "Delete successful" });
        }

        public IActionResult DeleteImage(int imageId)
        {
            var ImageTeBeDeleted = _unitOfWork.ProductImage.Get(u => u.Id == imageId);
            int productId = ImageTeBeDeleted.ProductId;

            if (ImageTeBeDeleted != null)
            {
                if (!string.IsNullOrEmpty(ImageTeBeDeleted.ImageUrl))
                {
                    var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, ImageTeBeDeleted.ImageUrl.TrimStart('\\'));

                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                _unitOfWork.ProductImage.Remove(ImageTeBeDeleted);
                _unitOfWork.Save();

                TempData["success"] = "Deleted successfully";
            }

            return RedirectToAction(nameof(Upsert), new { id = productId });
        }

        #endregion
    }
}
