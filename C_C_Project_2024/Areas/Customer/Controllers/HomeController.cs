using C_C_Proj_WebStore.DataAccess.Repository.IRepository;
using C_C_Proj_WebStore.Models;
using C_C_Proj_WebStore.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace C_C_Proj_WebStore.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            IEnumerable<Product> productList = _unitOfWork.Product.GetAll(includeProperties: "Category,ProductImages");
            return View(productList);
        }

        public IActionResult Details(int id)
        {
            ShoppingCard shoppingCard = new()
            {
                Product = _unitOfWork.Product.Get(u => u.Id == id, includeProperties: "Category,ProductImages"),
                ProductId = id,
                Count = 1
            };
            return View(shoppingCard);
        }

        [HttpPost]
        [Authorize]
        public IActionResult Details(ShoppingCard shoppingCard)
        {
            shoppingCard.Id = 0;
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            shoppingCard.ApplicationUserId = userId;
            ShoppingCard cardFromDb = _unitOfWork.ShoppingCard.Get(u => u.ApplicationUserId == userId && u.ProductId == shoppingCard.ProductId, tracked:true);
            if (cardFromDb == null)
            {
                shoppingCard.Product = _unitOfWork.Product.Get(u => u.Id == shoppingCard.ProductId, includeProperties: "Category,ProductImages", tracked: true);
                if (shoppingCard.Count > shoppingCard.Product.StockCount)
                {
                    TempData["Error"] = "Error: Not enough in stock";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    _unitOfWork.ShoppingCard.Add(shoppingCard);
                    shoppingCard.Product.StockCount -= shoppingCard.Count;
                    if (shoppingCard.Product.StockCount <= 0)
                    {
                        shoppingCard.Product.StockStatus = SD.OutOfStock;
                    }
                    _unitOfWork.Save();
                    HttpContext.Session.SetInt32(SD.SessionCart, _unitOfWork.ShoppingCard.GetAll(u => u.ApplicationUserId == userId).Count());
                }
            }
            else
            {
                cardFromDb.Product = _unitOfWork.Product.Get(u => u.Id == cardFromDb.ProductId, includeProperties: "Category,ProductImages", tracked: true);
                if (shoppingCard.Count > cardFromDb.Product.StockCount)
                {
                    TempData["Error"] = "Error: Not enough in stock";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    cardFromDb.Product.StockCount -= shoppingCard.Count;
                    cardFromDb.Count += shoppingCard.Count;
                    if (cardFromDb.Product.StockCount <= 0) { 
                        cardFromDb.Product.StockStatus = SD.OutOfStock;
                    }
                    _unitOfWork.ShoppingCard.Update(cardFromDb);
                    _unitOfWork.Save();
                }
            }
            TempData["Success"] = "Item added to cart successfully";
            
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpPost]
        public IActionResult GetFilteredProducts(string[] brands, double[] sizes)
        {
            IEnumerable<Product> productList = _unitOfWork.Product.GetAll(includeProperties: "Category,ProductImages");

            if (brands != null && brands.Length > 0)
            {
                productList = productList.Where(p => brands.Contains(p.Brand));
            }

            if (sizes != null && sizes.Length > 0)
            {
                productList = productList.Where(p => sizes.Contains(p.Size));
            }

            productList = productList.OrderBy(p => p.Brand).ThenBy(p => p.Size);

            return PartialView("_Copy", productList);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
