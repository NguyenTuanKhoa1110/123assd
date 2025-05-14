using Microsoft.AspNetCore.Mvc;
using W3_test.Domain.Models;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Net.Mail;
using System.Net;

namespace W3_test.Controllers
{

	public class AccountController : Controller
	{
		
		private readonly UserManager<AppUser> _userManager;
		private readonly SignInManager<AppUser> _signInManager;
		private readonly RoleManager<AppRole> _roleManager;
		private readonly IConfiguration _configuration;

		public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, RoleManager<AppRole> roleManager)
		{
			_userManager = userManager;
			_signInManager = signInManager;
			_roleManager = roleManager;
		}

		[HttpGet]
		public IActionResult Register()
		{
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Register(RegisterViewModel model)
		{
			if (ModelState.IsValid)
			{
				var user = new AppUser
				{
					UserName = model.Email,
					Email = model.Email,
					FirstName = model.FirstName,
					LastName = model.LastName,
					Age = model.Age,
					Address = model.Address
				};

				var result = await _userManager.CreateAsync(user, model.Password);
				if (result.Succeeded)
				{
					
					if (user.Email == " admin@example.com")
					{
						await _userManager.AddToRoleAsync(user, "Admin");
					}
					else
					{
						await _userManager.AddToRoleAsync(user, "Staff");
					}

					TempData["SuccessMessage"] = "Registration successful! Please log in.";
					return RedirectToAction("Login", "Account");
				}

				foreach (var error in result.Errors)
				{
					ModelState.AddModelError(string.Empty, error.Description);
				}
			}
			return View(model);
		}


		[HttpGet]
		public IActionResult Login()
		{
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
		{
			if (ModelState.IsValid)
			{
				var user = await _userManager.FindByEmailAsync(model.Email);
				if (user != null)
				{
					var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, false);
					if (result.Succeeded)
					{
						TempData["SuccessMessage"] = "Logged in successfully!";
						if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
							return Redirect(returnUrl);

						return RedirectToAction("Index", "Home");
					}
				}
				ModelState.AddModelError(string.Empty, "Invalid login attempt.");
			}
			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Logout()
		{
			await _signInManager.SignOutAsync();
			TempData["SuccessMessage"] = "You have been logged out.";
			return RedirectToAction("Index", "Home");
		}

		public async Task<IActionResult> CreateRoles()
		{
			var roleNames = new[] { "Admin", "User", "Staff" };
			foreach (var roleName in roleNames)
			{
				if (!await _roleManager.RoleExistsAsync(roleName))
					await _roleManager.CreateAsync(new AppRole(roleName));
			}
			return Ok("Roles created successfully.");
		}
		public IActionResult ForgotPassword()
		{
			return View();
		}

		[HttpPost]
		public IActionResult ForgotPassword(string email)
		{
			try
			{
				var smtpSettings = _configuration.GetSection("SmtpSettings").Get<SmtpSettings>();
				var fromAddress = new MailAddress(smtpSettings.FromAddress, smtpSettings.FromName);
				var toAddress = new MailAddress(email, "Recipient Name");
				string subject = "Password Reset Request";
				string body = "Dear User,\n\nClick the link to reset your password: https://yourdomain.com/reset-password\n\nBest regards,\nYour Team";

				var smtp = new SmtpClient
				{
					Host = smtpSettings.Host,
					Port = smtpSettings.Port,
					EnableSsl = smtpSettings.EnableSsl,
					DeliveryMethod = SmtpDeliveryMethod.Network,
					UseDefaultCredentials = false,
					Credentials = new NetworkCredential(smtpSettings.Username, smtpSettings.Password)
				};

				using (var message = new MailMessage(fromAddress, toAddress)
				{
					Subject = subject,
					Body = body
				})
				{
					smtp.Send(message);
				}

				ViewBag.Message = "Password reset link has been sent to your email.";
				return View();
			}
			catch (Exception ex)
			{
				ViewBag.Error = "An error occurred: " + ex.Message;
				return View();
			}
		}
	}
}
	

