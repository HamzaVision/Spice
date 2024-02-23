using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
    public class SubCategoryController : Controller
    {
        private readonly ApplicationDbContext _db;

        public SubCategoryController(ApplicationDbContext db)
        {
            _db = db;
        }
        public async Task<IActionResult> Index()
        {
            var subcategory = await _db.SubCategory.Include(s => s.Category).ToListAsync();
            return View(subcategory);
        }

        public async Task<IActionResult> Create()
        {
            SubCategoryAndCategoryViewModel model = new SubCategoryAndCategoryViewModel()
            {
                Categories = await _db.Category.ToListAsync(),
                subCategory = new Models.SubCategory(),
                Subcategories = await _db.SubCategory.OrderBy(p => p.Name).Select(p => p.Name).Distinct().ToListAsync()

            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SubCategoryAndCategoryViewModel model)
        {
            if (model.subCategory != null)
            {
                var doescategoryexists = _db.SubCategory.Include(p => p.Category).Where(s => s.Name == model.subCategory.Name && s.Category.Id == model.subCategory.CategoryId);


                if (doescategoryexists.Count() > 0)
                {
                    //Error
                    TempData["DangerMessage"] = "Error: This Sub Category already exists. Try another Name";

                }
                else
                {
                    _db.SubCategory.Add(model.subCategory);
                    await _db.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            SubCategoryAndCategoryViewModel modelVM = new SubCategoryAndCategoryViewModel()
            {
                Categories = await _db.Category.ToListAsync(),
                subCategory = new Models.SubCategory(),
                Subcategories = await _db.SubCategory.OrderBy(p => p.Name).Select(p => p.Name).Distinct().ToListAsync()
            };
            return View(modelVM);
        }

        [ActionName("GetSubCategory")]
        public async Task<IActionResult> GetSubCategory(int id)
        {
            List<SubCategory> list = new List<SubCategory>();

            list = await (from SubCategory in _db.SubCategory where SubCategory.CategoryId == id select SubCategory).ToListAsync();
            return Json(new SelectList(list, "Id", "Name"));
        }

        //Edit for the Sub Category with the view Model
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var sub = _db.SubCategory.Find(id);
            if (sub == null)
            {
                return NotFound();
            }
            SubCategoryAndCategoryViewModel model = new SubCategoryAndCategoryViewModel()
            {
                Categories = await _db.Category.ToListAsync(),
                subCategory = sub,
                Subcategories = await _db.SubCategory.OrderBy(p => p.Name).Select(p => p.Name).Distinct().ToListAsync()

            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? id, SubCategoryAndCategoryViewModel model)
        {
            
                var doescategoryexists = _db.SubCategory.Include(p => p.Category).Where(s => s.Name == model.subCategory.Name && s.Category.Id == model.subCategory.CategoryId);

                if (doescategoryexists.Count()>0)
                {
                    TempData["DangerMessage"] = "Error: This Sub Category already exists. Try another Name";
                }
                else
                {
                    try
                    {
                        var sub = await _db.SubCategory.FindAsync(id);
                        sub.Name = model.subCategory.Name;
                        _db.SubCategory.Update(sub);
                        await _db.SaveChangesAsync();
                        return RedirectToAction(nameof(Index));
                    }
                    catch (Exception ex)
                    { 
                        return RedirectToAction(nameof(Index));
                    }
                }
            

            SubCategoryAndCategoryViewModel modelVM = new SubCategoryAndCategoryViewModel()
            {
                Categories = await _db.Category.ToListAsync(),
                subCategory = new Models.SubCategory(),
                Subcategories = await _db.SubCategory.OrderBy(p => p.Name).Select(p => p.Name).Distinct().ToListAsync()
            };
            return View(modelVM);
        }

        //Find the Sub Category to send to edit View
        public async Task<IActionResult> Details(int? Id)
        {
            if (Id == null)
            {
                return NotFound();
            }
            var Subcategory = await _db.SubCategory.FindAsync(Id);
            if (Subcategory == null)
            {
                return NotFound();
            }
            SubCategoryAndCategoryViewModel modelVM = new SubCategoryAndCategoryViewModel()
            {
                Categories = await _db.Category.ToListAsync(),
                subCategory = Subcategory,
                Subcategories = await _db.SubCategory.OrderBy(p => p.Name).Select(p => p.Name).Distinct().ToListAsync()
            };
            return View(modelVM);
        }

        //Edit for the Sub Category with the view Model
        public async Task<IActionResult> Delete(int? Id)
        {
            if (Id == null)
            {
                return NotFound();
            }
            var Subcategory = await _db.SubCategory.FindAsync(Id);
            if (Subcategory == null)
            {
                return NotFound();
            }
            SubCategoryAndCategoryViewModel modelVM = new SubCategoryAndCategoryViewModel()
            {
                Categories = await _db.Category.ToListAsync(),
                subCategory = Subcategory,
                Subcategories = await _db.SubCategory.OrderBy(p => p.Name).Select(p => p.Name).Distinct().ToListAsync()
            };
            return View(modelVM);
        }

        //Post Delete function which is execute on the input type submit control
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(SubCategory obj)
        {
            if (obj != null)
            {
                _db.SubCategory.Remove(obj);
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            SubCategoryAndCategoryViewModel modelVM = new SubCategoryAndCategoryViewModel()
            {
                Categories = await _db.Category.ToListAsync(),
                subCategory = obj,
                Subcategories = await _db.SubCategory.OrderBy(p => p.Name).Select(p => p.Name).Distinct().ToListAsync()
            };
            return View(modelVM);
        }
    }
}
