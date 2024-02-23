// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Spice.Models;
using System.Security.Policy;
//using Microsoft.AspNetCore.Identity;
//using System.ComponentModel.DataAnnotations;
//using Spice.Models;
using Spice.Data;
using Microsoft.EntityFrameworkCore;
using Spice.Constant_Utility;

namespace Spice.Areas.Identity.Pages.Account
{
	public class LoginModel : PageModel
	{
		private readonly SignInManager<IdentityUser> _signInManager;
		private readonly ILogger<LoginModel> _logger;
		private readonly UserManager<IdentityUser> _userManager;
		private readonly ApplicationDbContext _db;
		public LoginModel(SignInManager<IdentityUser> signInManager, ILogger<LoginModel> logger, UserManager<IdentityUser> userManager, ApplicationDbContext db)
		{
			_signInManager = signInManager;
			_logger = logger;
			_userManager = userManager;
			_db = db;
		}

		/// <summary>
		///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
		///     directly from your code. This API may change or be removed in future releases.
		/// </summary>
		[BindProperty]
		public InputModel Input { get; set; }

		/// <summary>
		///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
		///     directly from your code. This API may change or be removed in future releases.
		/// </summary>
		public IList<AuthenticationScheme> ExternalLogins { get; set; }

		/// <summary>
		///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
		///     directly from your code. This API may change or be removed in future releases.
		/// </summary>
		public string ReturnUrl { get; set; }

		/// <summary>
		///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
		///     directly from your code. This API may change or be removed in future releases.
		/// </summary>
		[TempData]
		public string ErrorMessage { get; set; }

		/// <summary>
		///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
		///     directly from your code. This API may change or be removed in future releases.
		/// </summary>
		public class InputModel
		{
			/// <summary>
			///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
			///     directly from your code. This API may change or be removed in future releases.
			/// </summary>
			[Required]
			[EmailAddress]
			public string Email { get; set; }

			/// <summary>
			///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
			///     directly from your code. This API may change or be removed in future releases.
			/// </summary>
			[Required]
			[DataType(DataType.Password)]
			public string Password { get; set; }

			/// <summary>
			///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
			///     directly from your code. This API may change or be removed in future releases.
			/// </summary>
			[Display(Name = "Remember me?")]
			public bool RememberMe { get; set; }
		}

		public async Task OnGetAsync(string returnUrl = null)
		{
			if (!string.IsNullOrEmpty(ErrorMessage))
			{
				ModelState.AddModelError(string.Empty, ErrorMessage);
			}

			returnUrl ??= Url.Content("~/");

			// Clear the existing external cookie to ensure a clean login process
			await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

			ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

			ReturnUrl = returnUrl;
		}


		public async Task<IActionResult> OnPostAsync(string returnUrl = null)
		{
			returnUrl ??= Url.Content("~/");

			ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

			if (ModelState.IsValid)
			{
				var user = await _userManager.FindByEmailAsync(Input.Email);
				ApplicationUser user1 = new ApplicationUser();
				user1.Id = user.Id;
				user1.PasswordHash = user.PasswordHash;
				user1.Email = Input.Email;
				user1.Name = user.UserName;
				user1.EmailConfirmed = user.EmailConfirmed;
				if (user != null)
				{
					// Verify the provided password against the stored password hash
					var passwordHasher = new PasswordHasher<ApplicationUser>();
					var passwordVerificationResult = passwordHasher.VerifyHashedPassword(user1, user1.PasswordHash, Input.Password);

					if (passwordVerificationResult == PasswordVerificationResult.Success)
					{
						// Passwords match, proceed with sign in
						var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);
						if (result.Succeeded)
						{
							//var us = _db.Users.Where(s => s.Email == Input.Email).FirstOrDefaultAsync();
							var count =  _db.ShoppingCart.Where(s => s.ApplicationUserId == user1.Id).ToList().Count;
							HttpContext.Session.SetInt32(SD.ssShoppingCart, count);
							_logger.LogInformation("User logged in.");
							return LocalRedirect(returnUrl);
						}
						if (result.RequiresTwoFactor)
						{
							return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
						}
						if (result.IsLockedOut)
						{
							_logger.LogWarning("User account locked out.");
							return RedirectToPage("./Lockout");
						}
					}
				}

				ModelState.AddModelError(string.Empty, "Invalid login attempt.");
			}

			// If we got this far, something failed, redisplay form
			return Page();
		}

	}
}
