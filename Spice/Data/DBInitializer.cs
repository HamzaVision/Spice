using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Spice.Constant_Utility;
using Spice.Models;

namespace Spice.Data
{
    public class DBInitializer : IDBInitialize
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public DBInitializer(IServiceScopeFactory serviceScopeFactory,ApplicationDbContext db, UserManager<IdentityUser> userManager , RoleManager<IdentityRole> roleManager)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _db = db;
            _userManager= userManager;
            _roleManager= roleManager;
        }
        public async void Initialize()
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var serviceProvider = scope.ServiceProvider;
                try
                {
                    var dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();
                    if (dbContext.Database.GetPendingMigrations().Any())
                    {
                        dbContext.Database.Migrate();
                    }
                }
                catch (Exception ex)
                {
                    // Handle the exception appropriately
                }

                if (serviceProvider.GetService(typeof(RoleManager<IdentityRole>)) is RoleManager<IdentityRole> roleManager)
                {
                    if (!await roleManager.RoleExistsAsync(SD.ManagerRole))
                    {
                        await roleManager.CreateAsync(new IdentityRole(SD.ManagerRole));
                    }

                    if (!await roleManager.RoleExistsAsync(SD.FrontDeskRole))
                    {
                        await roleManager.CreateAsync(new IdentityRole(SD.FrontDeskRole));
                    }

                    if (!await roleManager.RoleExistsAsync(SD.KitchenRole))
                    {
                        await roleManager.CreateAsync(new IdentityRole(SD.KitchenRole));
                    }

                    if (!await roleManager.RoleExistsAsync(SD.CustomerRole))
                    {
                        await roleManager.CreateAsync(new IdentityRole(SD.CustomerRole));
                    }
                }

                if (serviceProvider.GetService(typeof(UserManager<IdentityUser>)) is UserManager<IdentityUser> userManager)
                {
                    var email = "hs249384@gmail.com";
                    var user = await userManager.FindByEmailAsync(email);
                    if (user == null)
                    {
                        var newUser = new IdentityUser
                        {
                            UserName = email,
                            Email = email,
                            EmailConfirmed = true
                        };
                        var result = await userManager.CreateAsync(newUser, "Hs@124");

                        if (result.Succeeded)
                        {
                            await userManager.AddToRoleAsync(newUser, SD.ManagerRole);
                        }
                    }
                    else
                    {
                        var isInRole = await userManager.IsInRoleAsync(user, SD.ManagerRole);
                        if (!isInRole)
                        {
                            await userManager.AddToRoleAsync(user, SD.ManagerRole);
                        }
                    }
                }
            }
        }


    }
}
