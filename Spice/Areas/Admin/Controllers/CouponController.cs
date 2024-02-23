using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using Spice.Constant_Utility;
using Spice.Data;
using Spice.Models;
using Spice.Models.ViewModels;
using System.Data;

namespace Spice.Areas.Admin.Controllers
{
	[Authorize(Roles = SD.ManagerRole)]
	[Area("Admin")]
    public class CouponController : Controller
    {
        private readonly ApplicationDbContext _db;
        public CouponController(ApplicationDbContext db) {
            _db = db;
        }
        public async Task<IActionResult> Index()
        {
            return View(await _db.Coupon.ToListAsync());
        }
        public IActionResult Create() 
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Coupon coupon)
        {
            if (coupon != null)
            {
                    var files = HttpContext.Request.Form.Files;
                    if (files.Count() > 0)
                    {
                        var picture = new byte[files[0].Length];
                        using (var fileStream = files[0].OpenReadStream())
                        {
                            await fileStream.ReadAsync(picture, 0, picture.Length);
                        }
                        coupon.Picture = picture;
                    }
                    _db.Coupon.Add(coupon);
                    await _db.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
            }
           
            return View(coupon);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var sub =await _db.Coupon.FindAsync(id);
            if (sub == null)
            {
                return NotFound();
            }
            
            return View(sub);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Coupon coupon)
        {
            if (coupon != null)
            { 
                var sub = _db.Coupon.Find(coupon.Id);
                sub.Name = coupon.Name.ToString();
                sub.discount = coupon.discount;
                sub.MinimumAmount = coupon.MinimumAmount;
                sub.IsActive = coupon.IsActive;
                sub.CouponType = coupon.CouponType;
                _db.Coupon.Update(sub);
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(coupon);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var sub =await _db.Coupon.FindAsync(id);
            if (sub == null)
            {
                return NotFound();
            }
            
            return View(sub);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var sub = await _db.Coupon.FindAsync(id);
            if (sub == null)
            {
                return NotFound();
            }

            return View(sub);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Coupon obj)
        {
            if (obj != null)
            {
                _db.Coupon.Remove(obj);
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(obj);
        }
    }
}
