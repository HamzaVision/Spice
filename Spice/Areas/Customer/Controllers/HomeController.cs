using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Spice.Constant_Utility;
using Spice.Data;
using Spice.Models;
using Spice.Models.ViewModels;
using System.Diagnostics;
using System.Security.Claims;

namespace Spice.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        //private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _db;

        public HomeController(ApplicationDbContext db)
        {
            _db = db;

        }
        //public HomeController(ILogger<HomeController> logger)
        //{
        //    _logger = logger;
        //}

        public async Task<IActionResult> Index()
        {
            IndexViewModel indexVM = new IndexViewModel
            {
                MenuItems = await _db.MenuItem.Include(s => s.Category).Include(s => s.SubCategory).ToListAsync(),
                categories = await _db.Category.ToListAsync(),
                coupons = await _db.Coupon.ToListAsync()
            };
            var claimIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if (claim != null)
            {
				var count = _db.ShoppingCart.Where(s => s.ApplicationUserId == claim.Value).ToList().Count;
				HttpContext.Session.SetInt32(SD.ssShoppingCart, count); 
			}
            
            return View(indexVM);
        }

        public IActionResult Privacy()
        {
            return View();
        }
        [Authorize]
        public async Task<IActionResult> Details(int id)
        {
            
            var menuitem = await _db.MenuItem.Include(s => s.Category).Include(s => s.SubCategory).Where(s => s.Id == id).FirstOrDefaultAsync();
            ShoppingCart shopcart = new ShoppingCart()
            {
                MenuItem = menuitem,
                MenuItemId = menuitem.Id
            };
            return View(shopcart);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Details(ShoppingCart obj)
        {
            if (obj != null)
            {
                var claimIdentity = (ClaimsIdentity)this.User.Identity;
                var claim = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);

                obj.ApplicationUserId = claim.Value;

                //object s = null;
                ShoppingCart cartfromdb = await _db.ShoppingCart.Where(s => s.ApplicationUserId == obj.ApplicationUserId && s.MenuItemId == obj.Id).FirstOrDefaultAsync();
                obj.MenuItemId = obj.Id;
                obj.Id = 0;
                if (cartfromdb == null)
                {
                    await _db.ShoppingCart.AddAsync(obj);
                }
                else {
                    cartfromdb.count = cartfromdb.count + obj.count;
                }
                
                await _db.SaveChangesAsync();
                var count = _db.ShoppingCart.Where(s=>s.ApplicationUserId == obj.ApplicationUserId).ToList().Count();
                HttpContext.Session.SetInt32(SD.ssShoppingCart,count);
                return RedirectToAction(nameof(Index));
            }
            else
            {
                var menuitem = await _db.MenuItem.Include(s => s.Category).Include(s => s.SubCategory).Where(s => s.Id == obj.Id).FirstOrDefaultAsync();
                ShoppingCart shopcart = new ShoppingCart()
                {
                    MenuItem = menuitem,
                    MenuItemId = menuitem.Id
                };
                return View(shopcart);
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}