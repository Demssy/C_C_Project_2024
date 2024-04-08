using C_C_Proj_WebStore.DataAccess.Repository.IRepository;
using C_C_Proj_WebStore.Models;
using C_C_Proj_WebStore.Models.ViewModels;
using C_C_Proj_WebStore.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
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

        //public IActionResult NotyfyMe(int id)
        //{
        //    ShoppingCard shoppingCard = new()
        //    {
        //        Product = _unitOfWork.Product.Get(u => u.Id == id, includeProperties: "Category,ProductImages"),
        //        ProductId = id,
        //        Count = 1
        //    };
        //    return View();
        //}


        public IActionResult NotifyMe(ShoppingCard shoppingCard)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            ApplicationUser applicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == userId);
            OrderHeader orderHeader = new OrderHeader();
            orderHeader.ApplicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == userId);
            orderHeader.ApplicationUserId = userId;
            orderHeader.Name = applicationUser.Name;
            orderHeader.PhoneNumber = applicationUser.PhoneNumber;
            orderHeader.StreetAddress = applicationUser.StreetAdress;
            orderHeader.City = applicationUser.City;
            orderHeader.Country = applicationUser.State;
            orderHeader.PostalCode = applicationUser.PostalAdress;
            orderHeader.OrderDate = DateTime.Now;
            orderHeader.OrderStatus = SD.PendingForOrderToStock;
            orderHeader.PaymentStatus = SD.PaymentStatusPending;
            _unitOfWork.OrderHeader.Add(orderHeader);
            _unitOfWork.Save();
            OrderDetail orderDetail = new OrderDetail
            {
                ProductId = shoppingCard.ProductId,
                OrderHeaderId = orderHeader.Id,
                Price = shoppingCard.Price,
                Count = shoppingCard.Count
            };
            _unitOfWork.OrderDetail.Add(orderDetail);
            _unitOfWork.Save();
            TempData["Success"] = "Item added to orders successfully";

            return RedirectToAction(nameof(Index));
        }

        public IActionResult BuyNow(ShoppingCard shoppingCard)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            ShoppingCard Card = new()
            {
                Product = _unitOfWork.Product.Get(u => u.Id == shoppingCard.ProductId, includeProperties: "Category,ProductImages"),
                ProductId = shoppingCard.ProductId,
                Count = shoppingCard.Count,
                ApplicationUserId = claim,
                ApplicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == claim)
            };
            Card.Price = CalculateOrderTotal(Card);
            return View(Card);
        }


        [HttpPost]
        public IActionResult BuyNowPOST(ShoppingCard shoppingCard)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            OrderHeader orderHeader = new OrderHeader();

            orderHeader.ApplicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == claim);
            orderHeader.Name = orderHeader.ApplicationUser.Name;
            orderHeader.PhoneNumber = orderHeader.ApplicationUser.PhoneNumber;
            orderHeader.StreetAddress = orderHeader.ApplicationUser.StreetAdress;
            orderHeader.City = orderHeader.ApplicationUser.City;
            orderHeader.Country = orderHeader.ApplicationUser.State;
            orderHeader.PostalCode = orderHeader.ApplicationUser.PostalAdress;

            orderHeader.OrderDate = System.DateTime.Now;
            orderHeader.ApplicationUserId = claim;

            ApplicationUser applicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == claim);

            ShoppingCard Card = new()
            {
                Product = _unitOfWork.Product.Get(u => u.Id == shoppingCard.ProductId, includeProperties: "Category,ProductImages"),
                ProductId = shoppingCard.ProductId,
                Count = shoppingCard.Count,
                ApplicationUserId = claim,
                ApplicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == claim)
            };
            Card.Price = CalculateOrderTotal(Card);
            //ShoppingCard.Price = CalculateOrderTotal(shoppingCard);
            orderHeader.OrderTotal += Card.Price * Card.Count;

            if (applicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                orderHeader.PaymentStatus = SD.PaymentStatusPending;
                orderHeader.OrderStatus = SD.StatusPending;
            }
            else
            {
                orderHeader.PaymentStatus = SD.PaymentStatusDelayedPayment;
                orderHeader.OrderStatus = SD.StatusApproved;
            }
            _unitOfWork.OrderHeader.Add(orderHeader);
            _unitOfWork.Save();

            OrderDetail orderDetail = new OrderDetail
            {
                ProductId = Card.ProductId,
                OrderHeaderId = orderHeader.Id,
                Price = Card.Price,
                Count = Card.Count
            };
            _unitOfWork.OrderDetail.Add(orderDetail);
            _unitOfWork.Save();

            if (applicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                var domain = "https://localhost:7162/";
                var options = new SessionCreateOptions
                {
                    SuccessUrl = domain + $"customer/home/OrderConfirmationn?id={orderHeader.Id}",
                    CancelUrl = domain + "customer/cart/Index",
                    LineItems = new List<SessionLineItemOptions>(),
                    Mode = "payment",
                };


                var sessionLineItemOptions = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)Card.Price * 100,
                        Currency = "ils",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = Card.Product.ShoeModel
                        }
                    },
                    Quantity = Card.Count
                };
                options.LineItems.Add(sessionLineItemOptions);

                var service = new SessionService();
                Session session = service.Create(options);
                _unitOfWork.OrderHeader.UpdateStripePaymentId(orderHeader.Id, session.Id, session.PaymentIntentId);

                _unitOfWork.Save();
                Response.Headers.Add("Location", session.Url);
                return StatusCode(303);
            }
			return RedirectToAction("OrderConfirmationn", new { id = orderHeader.Id });
		}

		public IActionResult OrderConfirmationn(int id)
		{

			OrderHeader orderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == id, includeProperties: "ApplicationUser");
			if (orderHeader == null)
			{
				HttpContext.Session.Clear();
				return View();

			}
			if (orderHeader.PaymentStatus != SD.PaymentStatusDelayedPayment)
			{
				var service = new SessionService();
				Session session = service.Get(orderHeader.SessionId);

				if (session.PaymentStatus.ToLower() == "paid")
				{
					_unitOfWork.OrderHeader.UpdateStripePaymentId(id, session.Id, session.PaymentIntentId);
					_unitOfWork.OrderHeader.UpdateStatus(id, SD.StatusApproved, SD.PaymentStatusApproved);
					_unitOfWork.Save();
				}
				HttpContext.Session.Clear();
				//else
				//            {
				//	orderHeader.PaymentStatus = SD.PaymentStatusRejected;
				//}
			}
			
			return View(id);
		}

		private double CalculateOrderTotal(ShoppingCard shoppingCard)
        {
            if (shoppingCard.Count <= 50)
            {
                return shoppingCard.Product.Price;
            }
            else
            {
                if (shoppingCard.Count <= 100)
                {
                    return shoppingCard.Product.Price50;
                }
                else
                {
                    return shoppingCard.Product.Price100;
                }
            }
        }

        public IActionResult Details(int id)
        {
            if (TempData["productId"] == null)
            {
                ShoppingCard shoppingCard = new()
                {
                    Product = _unitOfWork.Product.Get(u => u.Id == id, includeProperties: "Category,ProductImages"),
                    ProductId = id,
                    Count = 1
                };
                return View(shoppingCard);
            }
            else
            {
                int productId = (int)TempData["productId"];
                ShoppingCard shoppingCard = new()
                {
                    Product = _unitOfWork.Product.Get(u => u.Id == productId, includeProperties: "Category,ProductImages"),
                    ProductId = productId,
                    Count = 1
                };
                return View(shoppingCard);
            }
        }


        [HttpPost]
        [Authorize]
        public IActionResult Details(ShoppingCard shoppingCard)
        {
            shoppingCard.Id = 0;
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            shoppingCard.ApplicationUserId = userId;
            ShoppingCard cardFromDb = _unitOfWork.ShoppingCard.Get(u => u.ApplicationUserId == userId && u.ProductId == shoppingCard.ProductId);
            if (cardFromDb == null)
            {
                shoppingCard.Product = _unitOfWork.Product.Get(u => u.Id == shoppingCard.ProductId, includeProperties: "Category,ProductImages");
                if (shoppingCard.Count > shoppingCard.Product.StockCount)
                {
                    TempData["Error"] = "Not enough in stock";
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
                cardFromDb.Product = _unitOfWork.Product.Get(u => u.Id == cardFromDb.ProductId, includeProperties: "Category,ProductImages");
                if (shoppingCard.Count > cardFromDb.Product.StockCount)
                {
                    TempData["Error"] = "Not enough in stock";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    cardFromDb.Product.StockCount -= shoppingCard.Count;
                    cardFromDb.Count += shoppingCard.Count;
                    if (cardFromDb.Product.StockCount <= 0)
                    {
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
        public IActionResult GetFilteredProducts(string[] brands, double[] sizes,int price,string[] color,string category,string gender, int sort)
        {
            IEnumerable<Product> productList = _unitOfWork.Product.GetAll(includeProperties: "Category,ProductImages");


            if (brands != null && brands.Length > 0)
            {
                productList = productList.Where(p => brands.Contains(p.Brand));
            }

            if (color != null && color.Length > 0)
            {
                productList = productList.Where(p => color.Contains(p.Color));
            }

            if (sizes != null && sizes.Length > 0)
            {
                productList = productList.Where(p => sizes.Contains(p.Size));
            }

            if (price ==50 || price == 100 || price==200)
            {
                productList = productList.Where(p => ((int)p.Price)<=price);
            }

            if(category != null && category.Length > 0)
            {
                productList = productList.Where(p => p.Category.Name == category);
            }
            if (gender != null && gender.Length > 0)
            {
                productList = productList.Where(p => p.Gender == gender);
            }

            if (sort == 1)
                productList = productList.OrderBy(p => p.Price);
            if (sort == 2)
                productList = productList.OrderByDescending(p => p.Price);
            if (sort == 0)
                productList = productList.OrderBy(p => p.Brand).ThenBy(p => p.Size);


            return PartialView("_ProductList", productList);
        }

        public IActionResult GetCategories(string[] brands, double[] sizes)
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

            return PartialView("_ProductList", productList);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
