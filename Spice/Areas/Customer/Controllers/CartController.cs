using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Spice.Constant_Utility;
using Spice.Data;
using Spice.Models;
using Spice.Models.ViewModels;
using Stripe;
using System.Security.Claims;

namespace Spice.Areas.Customer.Controllers
{
	[Area("Customer")]
	public class CartController : Controller
	{
		private readonly ApplicationDbContext _db;
		[BindProperty]
		public OrderDetailsCartViewModel detailsCart { get; set; }
		public CartController(ApplicationDbContext db)
		{
			_db = db;
		}
		public async Task<IActionResult> Index()
		{
			detailsCart = new OrderDetailsCartViewModel()
			{
				OrderHeader = new Models.OrderHeader(),
			};
			detailsCart.OrderHeader.OriginalTotal = 0;

			var claimIdentity = (ClaimsIdentity)User.Identity;
			var claim = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);
			detailsCart.ListShoppingCarts = _db.ShoppingCart.Where(s => s.ApplicationUserId == claim.Value).ToList();
			foreach (var list in detailsCart.ListShoppingCarts)
			{
				list.MenuItem = _db.MenuItem.FirstOrDefault(s => s.Id == list.MenuItemId);
				detailsCart.OrderHeader.OriginalTotal = detailsCart.OrderHeader.OriginalTotal + (list.MenuItem.Price * list.count);
				list.MenuItem.description = SD.ConvertToRawHtml(list.MenuItem.description);
				if (list.MenuItem.description.Length > 100)
				{
					list.MenuItem.description = list.MenuItem.description.Substring(0, 99) + "...";
				}
			}
			detailsCart.OrderHeader.FinalTotal = detailsCart.OrderHeader.OriginalTotal;
			if (HttpContext.Session.GetString(SD.ssCouponCode) != null)
			{
				detailsCart.OrderHeader.CouponCode = HttpContext.Session.GetString(SD.ssCouponCode);
				var coupon = _db.Coupon.Where(s => s.Name.ToLower() == detailsCart.OrderHeader.CouponCode.ToLower()).FirstOrDefault();
				detailsCart.OrderHeader.FinalTotal = SD.DiscountPrice(coupon, detailsCart.OrderHeader.FinalTotal);
			}
			return View(detailsCart);
		}

		public async Task<IActionResult> Summary()
		{
			detailsCart = new OrderDetailsCartViewModel()
			{
				OrderHeader = new Models.OrderHeader(),
			};
			detailsCart.OrderHeader.OriginalTotal = 0;

			var claimIdentity = (ClaimsIdentity)User.Identity;
			var claim = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);

			ApplicationUser appUser = _db.ApplicationUser.FirstOrDefault(s => s.Id == claim.Value);
			detailsCart.ListShoppingCarts = _db.ShoppingCart.Where(s => s.ApplicationUserId == claim.Value).ToList();
			foreach (var list in detailsCart.ListShoppingCarts)
			{
				list.MenuItem = _db.MenuItem.FirstOrDefault(s => s.Id == list.MenuItemId);
				detailsCart.OrderHeader.OriginalTotal = detailsCart.OrderHeader.OriginalTotal + (list.MenuItem.Price * list.count);

			}
			detailsCart.OrderHeader.FinalTotal = detailsCart.OrderHeader.OriginalTotal;
			if (HttpContext.Session.GetString(SD.ssCouponCode) != null)
			{
				detailsCart.OrderHeader.CouponCode = HttpContext.Session.GetString(SD.ssCouponCode);
				var coupon = _db.Coupon.Where(s => s.Name.ToLower() == detailsCart.OrderHeader.CouponCode.ToLower()).FirstOrDefault();
				detailsCart.OrderHeader.FinalTotal = SD.DiscountPrice(coupon, detailsCart.OrderHeader.FinalTotal);
			}

			detailsCart.OrderHeader.PickUpName = appUser.Name;
			detailsCart.OrderHeader.PickUpNumber = appUser.PhoneNumber;
			detailsCart.OrderHeader.PickUpTime = DateTime.Now;
			return View(detailsCart);
		}
		public IActionResult AddCoup()
		{
			if (detailsCart.OrderHeader.CouponCode == null)
			{
				detailsCart.OrderHeader.CouponCode = "";
			}
			HttpContext.Session.SetString(SD.ssCouponCode, detailsCart.OrderHeader.CouponCode);
			return RedirectToAction(nameof(Index));
		}


		public IActionResult RemoveCoup()
		{

			HttpContext.Session.SetString(SD.ssCouponCode, string.Empty);
			return RedirectToAction(nameof(Index));
		}

		public async Task<IActionResult> Plus(int cartId)
		{
			var cart = await _db.ShoppingCart.FirstOrDefaultAsync(s => s.Id == cartId);
			cart.count += 1;
			await _db.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}
		public async Task<IActionResult> Minus(int cartId)
		{
			var cart = await _db.ShoppingCart.FirstOrDefaultAsync(s => s.Id == cartId);

			if (cart.count == 1)
			{
				_db.ShoppingCart.Remove(cart);
				await _db.SaveChangesAsync();
			}
			else
			{
				cart.count -= 1;
				await _db.SaveChangesAsync();
			}

			return RedirectToAction(nameof(Index));
		}
		public async Task<IActionResult> Delete(int cartId)
		{
			var cart = await _db.ShoppingCart.FirstOrDefaultAsync(s => s.Id == cartId);
			_db.ShoppingCart.Remove(cart);
			await _db.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[ActionName("Summary")]
		public async Task<IActionResult> Place_Order(string stripeToken)
		{
			var claimIdentity = (ClaimsIdentity)User.Identity;
			var claim = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);
			detailsCart.ListShoppingCarts = await _db.ShoppingCart.Where(s => s.ApplicationUserId == claim.Value).ToListAsync();


			detailsCart.OrderHeader.Status = SD.PaymentPending;
			detailsCart.OrderHeader.PaymentStatus = SD.PaymentPending;
			detailsCart.OrderHeader.PickUpTime = Convert.ToDateTime(detailsCart.OrderHeader.PickUpDate.ToShortDateString() + " " + detailsCart.OrderHeader.PickUpTime.ToShortTimeString());
			detailsCart.OrderHeader.ApplicationUserId = claim.Value;
			detailsCart.OrderHeader.OrderDate = DateTime.Now;
			detailsCart.OrderHeader.PickUpDate = detailsCart.OrderHeader.PickUpDate;
			detailsCart.OrderHeader.OriginalTotal = 0;
			detailsCart.OrderHeader.FinalTotal = 0;
			if (detailsCart.OrderHeader.Comments != null)
			{
				detailsCart.OrderHeader.Comments = detailsCart.OrderHeader.Comments;
			}
			else {
				detailsCart.OrderHeader.Comments = "";
			}
			detailsCart.OrderHeader.CouponCode = "";
			detailsCart.OrderHeader.CouponCodeDiscount = "";
			detailsCart.OrderHeader.TransactionId = "";
			detailsCart.OrderHeader.PaymentStatus = SD.PaymentPending;


			_db.OrderHeader.Add(detailsCart.OrderHeader);
			await _db.SaveChangesAsync();
			foreach (var list in detailsCart.ListShoppingCarts)
			{
				list.MenuItem = _db.MenuItem.FirstOrDefault(s => s.Id == list.MenuItemId);
				OrderDetails orderDetails = new OrderDetails()
				{
					MenuItemId = list.MenuItemId,
					OrderId = detailsCart.OrderHeader.Id,
					count = list.count,
					Description = list.MenuItem.description,
					Name = list.MenuItem.Name,
					Price = Convert.ToInt32(list.MenuItem.Price)
				};
				detailsCart.OrderHeader.OriginalTotal += orderDetails.count * orderDetails.Price;
				_db.OrderDetails.Add(orderDetails);
			}
			detailsCart.OrderHeader.FinalTotal = detailsCart.OrderHeader.OriginalTotal;
			if (HttpContext.Session.GetString(SD.ssCouponCode) != null)
			{
				detailsCart.OrderHeader.CouponCode = HttpContext.Session.GetString(SD.ssCouponCode);
				var coupon = _db.Coupon.Where(s => s.Name.ToLower() == detailsCart.OrderHeader.CouponCode.ToLower()).FirstOrDefault();
				detailsCart.OrderHeader.FinalTotal = SD.DiscountPrice(coupon, detailsCart.OrderHeader.FinalTotal);
			}
			detailsCart.OrderHeader.CouponCodeDiscount = (detailsCart.OrderHeader.OriginalTotal - detailsCart.OrderHeader.FinalTotal).ToString();
			await _db.SaveChangesAsync();
			_db.ShoppingCart.RemoveRange(detailsCart.ListShoppingCarts);
			HttpContext.Session.SetInt32(SD.ssShoppingCart,0);
			await _db.SaveChangesAsync();
			var options = new ChargeCreateOptions()
			{
				Amount = Convert.ToInt64(detailsCart.OrderHeader.FinalTotal), // Multiply by 100 to convert to cents
				Currency = "usd",
				Description = "Order ID: " + detailsCart.OrderHeader.Id, // Added a space after "ID"
				Source = stripeToken
			};
			var service = new ChargeService();
			Charge charge = service.Create(options);
			if (charge.BalanceTransactionId != null)
			{
				detailsCart.OrderHeader.PaymentStatus = SD.PaymentRejected;
			}
			else
			{
				detailsCart.OrderHeader.TransactionId = charge.BalanceTransactionId;
			}

			if (charge.Status.ToLower() == "succeeded")
			{
				detailsCart.OrderHeader.PaymentStatus = SD.PaymentApproved;
				detailsCart.OrderHeader.Status = SD.statusSubmitted;
			}
			else
			{
				detailsCart.OrderHeader.PaymentStatus = SD.PaymentRejected;
			}
			await _db.SaveChangesAsync();
			return RedirectToAction("Confirm", "Order" , new { id = detailsCart.OrderHeader.Id });

		}

	}
}
