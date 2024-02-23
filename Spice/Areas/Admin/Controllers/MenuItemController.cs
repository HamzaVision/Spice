using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    public class MenuItemController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _hostingEnvironment;
        [BindProperty]
        public MenuItemViewModel ModelVM { get; set; }

        public MenuItemController(ApplicationDbContext db,IWebHostEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
            _db = db;
            ModelVM = new MenuItemViewModel()
            {
                CategoryList = _db.Category,
                MenuItem = new Models.MenuItem()
            };
        }
        public async Task<IActionResult> Index()
        {
            var menuItems = await _db.MenuItem
           .Include(s => s.Category)
           .Include(s => s.SubCategory)
           .Select(s => new MenuItem
           {
               Id =s.Id,
               Name = s.Name,
               Price = s.Price,
               Category = s.Category,
               SubCategory = s.SubCategory
           })
           .ToListAsync();
            return View(menuItems);
        }

        public IActionResult Create()
        {
            
            return View(ModelVM);
        }

        [HttpPost,ActionName("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateMenuItem(MenuItemViewModel modelVM)
        {
            if (modelVM.MenuItem != null)
            {
                modelVM.MenuItem.SubCategoryId = Convert.ToInt32(Request.Form["SubCategoryList"].ToString());
                //modelVM.MenuItem.Id = 0;
                _db.MenuItem.Add(modelVM.MenuItem);
                await _db.SaveChangesAsync();

                string webroot = _hostingEnvironment.WebRootPath;
                var files = HttpContext.Request.Form.Files;
                var menuItem = await _db.MenuItem.FindAsync(modelVM.MenuItem.Id);

                if (files.Count > 0)
                {
                    // Files are uploaded
                    var uploads = Path.Combine(webroot, "images");
                    var extension = Path.GetExtension(files[0].FileName);
                    var filePath = Path.Combine(uploads, modelVM.MenuItem.Id + extension);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        files[0].CopyTo(fileStream);
                        menuItem.Image = @"\images\" + modelVM.MenuItem.Id + extension;
                    }
                }
                else
                {
                    var defaultImagePath = Path.Combine(webroot, "images", SD.DefaultPath);
                    var destinationPath = Path.Combine(webroot, "images", modelVM.MenuItem.Id + ".png");

                    System.IO.File.Copy(defaultImagePath, destinationPath);
                    menuItem.Image = @"\images\" + modelVM.MenuItem.Id + ".png";
                }

                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(modelVM);
        }


        public async Task<IActionResult> Edit(int? Id)
        {
            if (Id == null)
            {
                return NotFound();
            }

            try
            {
                ModelVM.MenuItem = await _db.MenuItem.Include(s => s.Category).Include(s => s.SubCategory).SingleOrDefaultAsync(m => m.Id == Id);

                if (ModelVM.MenuItem == null)
                {
                    return NotFound();
                }

                ModelVM.SubCategoryList = await _db.SubCategory
                    .Where(s => s.CategoryId == ModelVM.MenuItem.CategoryId)
                    .ToListAsync();

                return View(ModelVM);
            }
            catch (Exception)
            {
                // Handle the exception
                //Console.WriteLine("An error occurred: " + ex.Message);

                // Load the MenuItem without navigation properties
                ModelVM.MenuItem = await _db.MenuItem
                    .Select(s => new MenuItem
                    {
                        Id = s.Id,
                        Name = s.Name,
                        Price = s.Price,
                        CategoryId = s.CategoryId,
                        SubCategoryId = s.SubCategoryId,
                        Image = s.Image                        
                    })
                    .SingleOrDefaultAsync(s => s.Id == Id);

                if (ModelVM.MenuItem == null)
                {
                    return NotFound();
                }

                // Load the SubCategoryList based on CategoryId
                ModelVM.SubCategoryList = await _db.SubCategory
                    .Where(s => s.CategoryId == ModelVM.MenuItem.CategoryId)
                    .ToListAsync();

                return View(ModelVM);
            }
        }


        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditMenuItem(int? Id , MenuItemViewModel modelVM)
        {
            if (modelVM.MenuItem != null)
            {
                modelVM.MenuItem.SubCategoryId = Convert.ToInt32(Request.Form["SubCategoryList"].ToString());
                
                var sub = await _db.MenuItem
                    .Select(s => new MenuItem
                    {
                        Id = s.Id,
                        Name = s.Name,
                        Price = s.Price,
                        CategoryId = s.CategoryId,
                        SubCategoryId = s.SubCategoryId,                        
                    })
                    .SingleOrDefaultAsync(s => s.Id == Id);
                sub.Name = modelVM.MenuItem.Name; 
                sub.Price = modelVM.MenuItem.Price;
                sub.CategoryId = modelVM.MenuItem.CategoryId;
                sub.SubCategoryId = modelVM.MenuItem.SubCategoryId;
                sub.description = modelVM.MenuItem.description;
                sub.spicyness = modelVM.MenuItem.spicyness;


                _db.MenuItem.Update(sub);
                await _db.SaveChangesAsync();

                string webroot = _hostingEnvironment.WebRootPath;
                var files = HttpContext.Request.Form.Files;
                var menuItem = await _db.MenuItem.FindAsync(modelVM.MenuItem.Id);

                if (files.Count > 0)
                {
                    // Files are uploaded
                    var uploads = Path.Combine(webroot, "images");
                    var extension = Path.GetExtension(files[0].FileName);
                    var filePath = Path.Combine(uploads, modelVM.MenuItem.Id + extension);

                    var imagePath = Path.Combine(webroot,menuItem.Image.TrimStart('\\'));

                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        files[0].CopyTo(fileStream);
                        menuItem.Image = @"\images\" + modelVM.MenuItem.Id + extension;
                    }
                }
                else
                {
                    var defaultImagePath = Path.Combine(webroot, "images", SD.DefaultPath);
                    var destinationPath = Path.Combine(webroot, "images", modelVM.MenuItem.Id + ".png");

                    //System.IO.File.Copy(defaultImagePath, destinationPath);
                    menuItem.Image = @"\images\" + modelVM.MenuItem.Id + ".png";
                }

                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(modelVM);
        }


        public async Task<IActionResult> Details(int? Id)
        {
            if (Id == null)
            {
                return NotFound();
            }

            try
            {
                ModelVM.MenuItem = await _db.MenuItem.Include(s => s.Category).Include(s => s.SubCategory).SingleOrDefaultAsync(m => m.Id == Id);

                if (ModelVM.MenuItem == null)
                {
                    return NotFound();
                }

                ModelVM.SubCategoryList = await _db.SubCategory
                    .Where(s => s.CategoryId == ModelVM.MenuItem.CategoryId)
                    .ToListAsync();

                return View(ModelVM);
            }
            catch (Exception)
            {
                // Handle the exception
                //Console.WriteLine("An error occurred: " + ex.Message);

                // Load the MenuItem without navigation properties
                ModelVM.MenuItem = await _db.MenuItem
                                    .Select(s => new MenuItem
                                    {
                                        Id = s.Id,
                                        Name = s.Name,
                                        Price = s.Price,
                                        CategoryId = s.CategoryId,
                                        SubCategoryId = s.SubCategoryId,
                                        Image = s.Image
                                    })
                                    .SingleOrDefaultAsync(s => s.Id == Id);

                if (ModelVM.MenuItem == null)
                {
                    return NotFound();
                }

                // Load the SubCategoryList based on CategoryId
                ModelVM.SubCategoryList = await _db.SubCategory
                    .Where(s => s.CategoryId == ModelVM.MenuItem.CategoryId)
                    .ToListAsync();

                return View(ModelVM);
            }
        }

        public async Task<IActionResult> Delete(int? Id)
        {
            if (Id == null)
            {
                return NotFound();
            }

            try
            {
                ModelVM.MenuItem = await _db.MenuItem.Include(s => s.Category).Include(s => s.SubCategory).SingleOrDefaultAsync(m => m.Id == Id);

                if (ModelVM.MenuItem == null)
                {
                    return NotFound();
                }

                ModelVM.SubCategoryList = await _db.SubCategory
                    .Where(s => s.CategoryId == ModelVM.MenuItem.CategoryId)
                    .ToListAsync();

                return View(ModelVM);
            }
            catch (Exception ex)
            {
                // Handle the exception
                //Console.WriteLine("An error occurred: " + ex.Message);

                // Load the MenuItem without navigation properties
                ModelVM.MenuItem = await _db.MenuItem
                                    .Select(s => new MenuItem
                                    {
                                        Id = s.Id,
                                        Name = s.Name,
                                        Price = s.Price,
                                        CategoryId = s.CategoryId,
                                        SubCategoryId = s.SubCategoryId,
                                        Image = s.Image
                                    })
                                    .SingleOrDefaultAsync(s => s.Id == Id);

                if (ModelVM.MenuItem == null)
                {
                    return NotFound();
                }

                // Load the SubCategoryList based on CategoryId
                ModelVM.SubCategoryList = await _db.SubCategory
                    .Where(s => s.CategoryId == ModelVM.MenuItem.CategoryId)
                    .ToListAsync();

                return View(ModelVM);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(MenuItem obj)
        {
            if (obj != null)
            {
                _db.MenuItem.Remove(obj);
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            MenuItemViewModel modelVM = new MenuItemViewModel()
            {
                CategoryList = await _db.Category.ToListAsync(),
                MenuItem = obj,
                SubCategoryList = await _db.SubCategory.Where(s => s.CategoryId == ModelVM.MenuItem.CategoryId).ToListAsync()
            };
            return View(modelVM);
        }

    }
}
