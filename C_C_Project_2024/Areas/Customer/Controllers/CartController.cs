using C_C_Proj_WebStore.DataAccess.Repository.IRepository;
using C_C_Proj_WebStore.Models;
using C_C_Proj_WebStore.Models.ViewModels;
using C_C_Proj_WebStore.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using System.Security.Claims;

namespace C_C_Proj_WebStore.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        [BindProperty]
        public ShoppingCartVM ShoppingCartVM { get; set; }
        public CartController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            ShoppingCartVM = new ShoppingCartVM()
            {
                ShoppingCartList = _unitOfWork.ShoppingCard.GetAll(u => u.ApplicationUserId == claim, includeProperties: "Product"),
                OrderHeader = new OrderHeader()
            };

            IEnumerable<ProductImage> productImages = _unitOfWork.ProductImage.GetAll();

            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.Product.ProductImages = productImages.Where(u=>u.ProductId == cart.Product.Id).ToList();
                cart.Price = CalculateOrderTotal(cart);
                ShoppingCartVM.OrderHeader.OrderTotal += cart.Price * cart.Count;
            }
            return View(ShoppingCartVM);
        }

        public IActionResult Summary()
        {
            if (TempData["productId"] == null)
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
                ShoppingCartVM = new ShoppingCartVM()
                {
                    ShoppingCartList = _unitOfWork.ShoppingCard.GetAll(u => u.ApplicationUserId == claim, includeProperties: "Product"),
                    OrderHeader = new OrderHeader()
                };

                ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == claim);
                ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.ApplicationUser.Name;
                ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
                ShoppingCartVM.OrderHeader.StreetAddress = ShoppingCartVM.OrderHeader.ApplicationUser.StreetAdress;
                ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.ApplicationUser.City;
                ShoppingCartVM.OrderHeader.Country = ShoppingCartVM.OrderHeader.ApplicationUser.State;
                ShoppingCartVM.OrderHeader.PostalCode = ShoppingCartVM.OrderHeader.ApplicationUser.PostalAdress;

                foreach (var cart in ShoppingCartVM.ShoppingCartList)
                {
                    cart.Price = CalculateOrderTotal(cart);
                    ShoppingCartVM.OrderHeader.OrderTotal += cart.Price * cart.Count;
                }
                return View(ShoppingCartVM);
            }
            else
            {
                var productId = Convert.ToInt32(TempData["productId"]);
                var productCount = Convert.ToInt32(TempData["productCount"]);
                ShoppingCard Card = new()
                {
                    Product = _unitOfWork.Product.Get(u => u.Id == productId, includeProperties: "Category,ProductImages"),
                    ProductId = productId,
                    Count = productCount
                };
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
                ShoppingCartVM = new ShoppingCartVM()
                {
                    ShoppingCartList = new List<ShoppingCard>(),
                    OrderHeader = new OrderHeader()
                };
                ShoppingCartVM.ShoppingCartList.Append(Card);
                ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == claim);
                ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.ApplicationUser.Name;
                ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
                ShoppingCartVM.OrderHeader.StreetAddress = ShoppingCartVM.OrderHeader.ApplicationUser.StreetAdress;
                ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.ApplicationUser.City;
                ShoppingCartVM.OrderHeader.Country = ShoppingCartVM.OrderHeader.ApplicationUser.State;
                ShoppingCartVM.OrderHeader.PostalCode = ShoppingCartVM.OrderHeader.ApplicationUser.PostalAdress;
                Card.Price = CalculateOrderTotal(Card);
                ShoppingCartVM.OrderHeader.OrderTotal += Card.Price * Card.Count;
                return View(ShoppingCartVM);
            }
        }

        [HttpPost]
        [ActionName("Summary")]
		public IActionResult SummaryPOST()
		{
			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            ShoppingCartVM.ShoppingCartList = _unitOfWork.ShoppingCard.GetAll(u => u.ApplicationUserId == claim, includeProperties: "Product");
            ShoppingCartVM.OrderHeader.OrderDate = System.DateTime.Now;
            ShoppingCartVM.OrderHeader.ApplicationUserId = claim;
		
			ApplicationUser applicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == claim);
			
			foreach (var cart in ShoppingCartVM.ShoppingCartList)
			{
				cart.Price = CalculateOrderTotal(cart);
				ShoppingCartVM.OrderHeader.OrderTotal += cart.Price * cart.Count;
			}
            if(applicationUser.CompanyId.GetValueOrDefault() == 0)
            {
				ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
                ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusPending;
			}
			else
            {
				ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusDelayedPayment;
				ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusApproved;
			}
            _unitOfWork.OrderHeader.Add(ShoppingCartVM.OrderHeader);
            _unitOfWork.Save();
            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
				OrderDetail orderDetail = new OrderDetail
                {
					ProductId = cart.ProductId,
					OrderHeaderId = ShoppingCartVM.OrderHeader.Id,
					Price = cart.Price,
					Count = cart.Count
				};
				_unitOfWork.OrderDetail.Add(orderDetail);
                _unitOfWork.Save();
			}
			if (applicationUser.CompanyId.GetValueOrDefault() == 0)
			{
                var domain = "https://localhost:7162/";
				var options = new SessionCreateOptions
				{
					SuccessUrl = domain + $"customer/cart/OrderConfirmation?id={ShoppingCartVM.OrderHeader.Id}",
                    CancelUrl = domain + "customer/cart/Index",
					LineItems = new List<SessionLineItemOptions>(),
					Mode = "payment",
				};

                foreach (var item in ShoppingCartVM.ShoppingCartList)
                {
                    var sessionLineItemOptions = new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)item.Price * 100,
                            Currency = "ils",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = item.Product.ShoeModel
                            }
                        },
                        Quantity = item.Count
                    };
                    options.LineItems.Add(sessionLineItemOptions);
                }
				var service = new SessionService();
				Session session = service.Create(options);
                _unitOfWork.OrderHeader.UpdateStripePaymentId(ShoppingCartVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
               
                _unitOfWork.Save();
                Response.Headers.Add("Location", session.Url);
                return StatusCode(303);
			}
            return RedirectToAction(nameof(OrderConfirmation), new {id=ShoppingCartVM.OrderHeader.Id});
		}

        public IActionResult OrderConfirmation(int id)
        {
            
            OrderHeader orderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == id, includeProperties: "ApplicationUser");
            if(orderHeader == null)
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
            List<ShoppingCard> shoppingCardList = _unitOfWork.ShoppingCard.GetAll(u => u.ApplicationUserId == orderHeader.ApplicationUserId).ToList();
            _unitOfWork.ShoppingCard.RemoveRange(shoppingCardList);
            _unitOfWork.Save();
			return View(id);
		}

		public IActionResult Plus(int cartId)
        {
            var cartFromDb = _unitOfWork.ShoppingCard.Get(u => u.Id == cartId);
            cartFromDb.Product = _unitOfWork.Product.Get(u => u.Id == cartFromDb.ProductId);
            if (cartFromDb.Product.StockCount - 1 < 0)
            {
                cartFromDb.Product.StockStatus = SD.OutOfStock;
                TempData["Error"] = "Not enough in stock";
                _unitOfWork.ShoppingCard.Update(cartFromDb);
                _unitOfWork.Save();
                return RedirectToAction(nameof(Index));
            }
            else
            {
                cartFromDb.Product.StockCount = cartFromDb.Product.StockCount - 1;
                cartFromDb.Count += 1;
                _unitOfWork.ShoppingCard.Update(cartFromDb);
                _unitOfWork.Save();
                return RedirectToAction(nameof(Index));
            }
        }

        public IActionResult Minus(int cartId)
        {
            var cartFromDb = _unitOfWork.ShoppingCard.Get(u => u.Id == cartId);
            if (cartFromDb.Count <= 1)
            {
                cartFromDb.Product = _unitOfWork.Product.Get(u => u.Id == cartFromDb.ProductId);
                cartFromDb.Product.StockCount = cartFromDb.Product.StockCount + 1;
                if (cartFromDb.Product.StockCount > 0)
                {
                    cartFromDb.Product.StockStatus = SD.AvailableInStock;
                }
                HttpContext.Session.SetInt32(SD.SessionCart, _unitOfWork.ShoppingCard
                    .GetAll(u => u.ApplicationUserId == cartFromDb.ApplicationUserId).Count() - 1);
                _unitOfWork.ShoppingCard.Remove(cartFromDb);
            }
            else
            {
                cartFromDb.Count -= 1;
                cartFromDb.Product = _unitOfWork.Product.Get(u => u.Id == cartFromDb.ProductId);
                cartFromDb.Product.StockCount = cartFromDb.Product.StockCount + 1;
                if (cartFromDb.Product.StockCount > 0)
                {
                    cartFromDb.Product.StockStatus = SD.AvailableInStock;
                }
                _unitOfWork.ShoppingCard.Update(cartFromDb);
            }
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Remove(int cartId)
        {
            var cartFromDb = _unitOfWork.ShoppingCard.Get(u => u.Id == cartId);
            cartFromDb.Product = _unitOfWork.Product.Get(u => u.Id == cartFromDb.ProductId);
            cartFromDb.Product.StockCount = cartFromDb.Product.StockCount + cartFromDb.Count;
            HttpContext.Session.SetInt32(SD.SessionCart, _unitOfWork.ShoppingCard.GetAll(u => u.ApplicationUserId == cartFromDb.ApplicationUserId).Count() - 1);
            _unitOfWork.ShoppingCard.Remove(cartFromDb);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }
        private double CalculateOrderTotal(ShoppingCard shoppingCard)
        {
           if(shoppingCard.Count <= 50)
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
    }
}
