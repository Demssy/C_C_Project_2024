﻿using C_C_Proj_WebStore.DataAccess.Repository.IRepository;
using C_C_Proj_WebStore.Models;
using C_C_Proj_WebStore.Models.ViewModels;
using C_C_Proj_WebStore.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using System.Security.Claims;
using System.Security.Cryptography;

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
                cart.Product.ProductImages = productImages.Where(u => u.ProductId == cart.Product.Id).ToList();
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
                    OrderHeader = new OrderHeader(),
                    UserCreditCard = _unitOfWork.UserCreditCard.Get(u => u.ApplicationUserId == claim) ?? new UserCreditCard()

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
                    OrderHeader = new OrderHeader(),
                    UserCreditCard = _unitOfWork.UserCreditCard.Get(u => u.ApplicationUserId == claim) ?? new UserCreditCard()
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
            UserCreditCard userCreditCardFromDb = _unitOfWork.UserCreditCard.Get(u => u.ApplicationUserId == claim);
            if (userCreditCardFromDb == null)
            {
                ShoppingCartVM.UserCreditCard.key = new byte[16];
                ShoppingCartVM.UserCreditCard.iv = new byte[16];
                using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(ShoppingCartVM.UserCreditCard.key);
                    rng.GetBytes(ShoppingCartVM.UserCreditCard.iv);
                }
                ShoppingCartVM.UserCreditCard.ApplicationUserId = applicationUser.Id;
                ShoppingCartVM.UserCreditCard.EncryptedCardNumber = Encrypt(plainText: ShoppingCartVM.UserCreditCard.CardNumber, ShoppingCartVM.UserCreditCard.key, ShoppingCartVM.UserCreditCard.iv);
                ShoppingCartVM.UserCreditCard.EncryptedCVV = Encrypt(ShoppingCartVM.UserCreditCard.CVV, ShoppingCartVM.UserCreditCard.key, ShoppingCartVM.UserCreditCard.iv);
                _unitOfWork.UserCreditCard.Add(ShoppingCartVM.UserCreditCard);
                _unitOfWork.Save();
            }


            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.Price = CalculateOrderTotal(cart);
                ShoppingCartVM.OrderHeader.OrderTotal += cart.Price * cart.Count;
            }
            if (applicationUser.CompanyId.GetValueOrDefault() == 0)
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
                var domain = Request.Scheme + "://" + Request.Host.Value+"/";
                var options = new SessionCreateOptions
                {
                    PaymentMethodTypes = new List<string>
                    {
                    "card"
                    },
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
                        Quantity = item.Count,
                    };
                    options.LineItems.Add(sessionLineItemOptions);
                }
                var service = new SessionService();
                Session session = service.Create(options);
                _unitOfWork.OrderHeader.UpdateStripePaymentId(ShoppingCartVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
                _unitOfWork.Save();
                Response.Headers.Add("Location", session.Url);
                foreach (var item in ShoppingCartVM.ShoppingCartList)
                {
                    item.Product.StockCount -= item.Count;
                    item.Product.PurchasesCount++;
                    if (item.Product.StockCount <= 0)
                    {
                        item.Product.StockStatus = SD.OutOfStock;
                        return RedirectToAction(nameof(Index));
                        TempData["Error"] = "Not enough in stock";
                    }
                    _unitOfWork.Product.Update(item.Product);
                    _unitOfWork.Save();
                }
                return StatusCode(303);
            }
            return RedirectToAction(nameof(OrderConfirmation), new { id = ShoppingCartVM.OrderHeader.Id });
        }

        public IActionResult OrderConfirmation(int id)
        {

            OrderHeader orderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == id, includeProperties: "ApplicationUser");
            OrderDetail orderDetail = _unitOfWork.OrderDetail.Get(u => u.OrderHeaderId == id);
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
            
            List<ShoppingCard> shoppingCardList = _unitOfWork.ShoppingCard.GetAll(u => u.ApplicationUserId == orderHeader.ApplicationUserId, includeProperties: "Product").ToList();
            _unitOfWork.ShoppingCard.RemoveRange(shoppingCardList);
            _unitOfWork.Save();
            shoppingCardList.ForEach(u => u.Product = _unitOfWork.Product.Get(u => u.Id == u.Id, includeProperties:"Category"));
            return View(shoppingCardList);
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
                HttpContext.Session.SetInt32(SD.SessionCart, _unitOfWork.ShoppingCard
                    .GetAll(u => u.ApplicationUserId == cartFromDb.ApplicationUserId).Count() - 1);
                _unitOfWork.ShoppingCard.Remove(cartFromDb);
            }
            else
            {
                cartFromDb.Count -= 1;
                _unitOfWork.ShoppingCard.Update(cartFromDb);
            }
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Remove(int cartId)
        {
            var cartFromDb = _unitOfWork.ShoppingCard.Get(u => u.Id == cartId);
            HttpContext.Session.SetInt32(SD.SessionCart, _unitOfWork.ShoppingCard.GetAll(u => u.ApplicationUserId == cartFromDb.ApplicationUserId).Count() - 1);
            _unitOfWork.ShoppingCard.Remove(cartFromDb);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }
        private double CalculateOrderTotal(ShoppingCard shoppingCard)
        {
            if (shoppingCard.Count <= 50)
            {
                if (shoppingCard.Product.Discount > 0)
                {
                    return shoppingCard.Product.Price - (shoppingCard.Product.Price * shoppingCard.Product.Discount);

                }
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

        public byte[] Encrypt(string plainText, byte[] key, byte[] iv)
        {
            byte[] cipheredtext;
            using (Aes aesAlg = Aes.Create())
            {
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(key, iv);
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                        cipheredtext = msEncrypt.ToArray();
                    }
                }
            }
            return cipheredtext;
        }


        public string Decrypt(byte[] cipherText, byte[] key, byte[] iv)
        {
            string plaintext = String.Empty;
            using (Aes aesAlg = Aes.Create())
            {
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(key, iv);
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
            return plaintext;
        }
    }
}
