using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Spice.Constant_Utility;
using Spice.Data;
using Spice.Models;

namespace Spice.Areas.Admin.Controllers
{
    [Authorize(Roles = SD.ManagerRole)]
    [Area("Admin")]
    public class CategoryController : Controller
    {

        private readonly ApplicationDbContext _db;

        public CategoryController(ApplicationDbContext db)
        {
            _db = db;
        }

        //GET
        public async Task<IActionResult> Index()
        {
            return View(await _db.Category.ToListAsync());
        }

        //Create Navbar button view
        public IActionResult Create()
        {
            return View();
        }

        //Post the Cateogry which is created 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category obj)
        {
            if (ModelState.IsValid)
            {
                _db.Category.Add(obj);
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));

            }

            return View(obj);
        }


        //Find the Category to send to edit View
        public async Task<IActionResult> Edit(int? Id)
        {
            if (Id == null)
            {
                return NotFound();
            }
            var category = await _db.Category.FindAsync(Id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        //Post Edit Update function which is execute on the input type submit control
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Category obj)
        {
            if (ModelState.IsValid)
            {
                _db.Category.Update(obj);
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(obj);
        }

        //Get the object for the view to further delete it 
        public async Task<IActionResult> Delete(int? Id)
        {
            if (Id == null || Id == 0)
            {
                return NotFound();
            }
            var category = await _db.Category.FindAsync(Id);
            if (category == null) { return NotFound(); }

            return View(category);
        }

        //Post Delete function which is execute on the input type submit control
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Category obj)
        {
            if (ModelState.IsValid)
            {
                _db.Category.Remove(obj);
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(obj);
        }
        
        //Find the Category to send to edit View
        public async Task<IActionResult> Details(int? Id)
        {
            if (Id == null)
            {
                return NotFound();
            }
            var category = await _db.Category.FindAsync(Id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }
    }
}
