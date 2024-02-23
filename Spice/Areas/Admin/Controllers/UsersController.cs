using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Spice.Constant_Utility;
using Spice.Data;
using System.Data;
using System.Security.Claims;

namespace Spice.Areas.Admin.Controllers
{
	[Authorize(Roles = SD.ManagerRole)]
	[Area("Admin")]
	public class UsersController : Controller
	{
		private readonly ApplicationDbContext _db;

		public UsersController(ApplicationDbContext db)
		{
			_db = db;
		}
		public async Task<IActionResult> Index()
		{
			var claimIdentity = (ClaimsIdentity)this.User.Identity;
			var claim = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);
			return View(await _db.ApplicationUser.Where(s => s.Id != claim.Value).ToListAsync());
		}

		public async Task<IActionResult> Lock(string id)
		{
			if (id == null)
			{
				return NotFound();
			}
			var applciationuser = await _db.ApplicationUser.FirstOrDefaultAsync(s => s.Id == id);
			if (applciationuser == null)
			{
				return NotFound();
			}
			applciationuser.LockoutEnd = DateTime.Now.AddYears(100);
			await _db.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}

		public async Task<IActionResult> UnLock(string id)
		{
			if (id == null)
			{
				return NotFound();
			}
			var applciationuser = await _db.ApplicationUser.FirstOrDefaultAsync(s => s.Id == id);
			if (applciationuser == null)
			{
				return NotFound();
			}
			applciationuser.LockoutEnd = DateTime.Now;
			await _db.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}
	}
}
