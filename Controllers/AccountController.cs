using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using W3_test.Domain.Models;

namespace W3_test.Controllers
{

	public class AccountController : Controller
	{
		
		private readonly UserManager<AppUser> _userManager;
		private readonly SignInManager<AppUser> _signInManager;
		private readonly RoleManager<AppRole> _roleManager;
		private readonly IConfiguration _configuration;
        private readonly IMemoryCache _memoryCache;
        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, RoleManager<AppRole> roleManager, IMemoryCache memoryCache)
		{
			_userManager = userManager;
			_signInManager = signInManager;
			_roleManager = roleManager;
            _memoryCache = memoryCache;
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
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View(model);
            }

            // Kiểm tra mật khẩu tạm trong cache
            string cacheKey = $"TempPassword_{model.Email}";
            if (_memoryCache.TryGetValue(cacheKey, out string tempPassword))
            {
                var expiryKey = $"TempPasswordExpires_{model.Email}";
                if (_memoryCache.TryGetValue(expiryKey, out DateTime expiresAt) && DateTime.UtcNow <= expiresAt)
                {
                    if (model.Password == tempPassword)
                    {
                        // Reset mật khẩu thực tế
                        var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
                        var resetResult = await _userManager.ResetPasswordAsync(user, resetToken, model.Password);
                        if (resetResult.Succeeded)
                        {
                            // Xóa cache mật khẩu tạm
                            _memoryCache.Remove(cacheKey);
                            _memoryCache.Remove(expiryKey);

                            await _signInManager.SignInAsync(user, model.RememberMe);
                            TempData["SuccessMessage"] = "Logged in successfully with temporary password and password has been updated.";

                            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                                return Redirect(returnUrl);

                            return RedirectToAction("Index", "Home");
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, "Cannot update password. Please try again.");
                            return View(model);
                        }
                    }
                }
                else
                {
                    // Mật khẩu tạm hết hạn => xóa cache
                    _memoryCache.Remove(cacheKey);
                    _memoryCache.Remove(expiryKey);
                }
            }

            // Đăng nhập bình thường
            var signInResult = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: false);
            if (signInResult.Succeeded)
            {
                TempData["SuccessMessage"] = "Logged in successfully!";
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(model);
        }



        [HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Logout()
		{
			await _signInManager.SignOutAsync();
			TempData["SuccessMessage"] = "You have been logged out.";
			return RedirectToAction("Login", "Account");
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
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                ModelState.AddModelError("", "Please enter your email.");
                return View();
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                // Không tiết lộ user không tồn tại vì lý do bảo mật
                ViewBag.Message = "If this email exists in our system, a temporary password has been sent.";
                return View();
            }

            // Tạo mật khẩu tạm thời ngẫu nhiên
            var tempPassword = GenerateRandomPassword(8);

            // Reset mật khẩu bằng cách tạo token và dùng ResetPasswordAsync
            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetPassResult = await _userManager.ResetPasswordAsync(user, resetToken, tempPassword);

            if (!resetPassResult.Succeeded)
            {
                ModelState.AddModelError("", "Cannot reset password now, please try later.");
                return View();
            }

            // Gửi email mật khẩu tạm thời
            try
            {
                var smtpSettings = _configuration.GetSection("SmtpSettings").Get<SmtpSettings>();
                var fromAddress = new MailAddress(smtpSettings.FromAddress, smtpSettings.FromName);
                var toAddress = new MailAddress(email, user.UserName);

                string subject = "Your Temporary Password";
                string body = $"Dear {user.UserName},\n\nYour temporary password is: {tempPassword}\nPlease log in within 5 minutes and change your password.\n\nBest regards,\nYour Team";

                using (var smtp = new SmtpClient
                {
                    Host = smtpSettings.Host,
                    Port = smtpSettings.Port,
                    EnableSsl = smtpSettings.EnableSsl,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(smtpSettings.Username, smtpSettings.Password)
                })
                using (var message = new MailMessage(fromAddress, toAddress) { Subject = subject, Body = body })
                {
                    smtp.Send(message);
                }

                // Lưu mật khẩu tạm thời và thời gian hiện tại vào cache (dùng email làm key)
                _memoryCache.Set($"TempPassword_{email}", tempPassword, TimeSpan.FromMinutes(5));

                ViewBag.Message = "A temporary password has been sent to your email.";
                return View();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error sending email: {ex.Message}");
                return View();
            }
        }

        private string GenerateRandomPassword(int length)
        {
            const string validChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*?";
            var random = new Random();
            return new string(Enumerable.Repeat(validChars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }


        [HttpGet]
        [Authorize]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login");

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user);
                TempData["Success"] = "Password changed successfully.";
                return RedirectToAction("Profile");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(model);
        }
        [HttpGet]
        public IActionResult ResetPassword(string token, string email)
        {
            if (token == null || email == null)
            {
                return RedirectToAction("Login");
            }

            var model = new ResetPasswordViewModel { Token = token, Email = email };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null) return RedirectToAction("ResetPasswordConfirmation");

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
            if (result.Succeeded) return RedirectToAction("ResetPasswordConfirmation");

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View();
        }

        public IActionResult ResetPasswordConfirmation()
        {
            return View(); 
        }

    }
}
	

