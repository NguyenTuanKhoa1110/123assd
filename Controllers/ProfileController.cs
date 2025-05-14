using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using W3_test.Domain.Models;

namespace W3_test.Controllers
{
	[Authorize]
	public class ProfileController : Controller
	{
		private readonly UserManager<AppUser> _userManager;

		public ProfileController(UserManager<AppUser> userManager)
		{
			_userManager = userManager;
		}

		public async Task<IActionResult> Index()
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null)
			{
				return NotFound("User not found.");
			}

			ViewData["FirstName"] = user.FirstName;
			ViewData["LastName"] = user.LastName;
			ViewData["Age"] = user.Age;
			ViewData["PhoneNumber"] = user.PhoneNumber;
			ViewData["Address"] = user.Address;

			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> UpdateProfile(string firstName, string lastName, int age, string phoneNumber, string address)
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null)
			{
				return NotFound("User not found.");
			}

			user.FirstName = firstName;
			user.LastName = lastName;
			user.Age = age;
			user.PhoneNumber = phoneNumber;
			user.Address = address;

			var result = await _userManager.UpdateAsync(user);
			if (result.Succeeded)
			{
				TempData["SuccessMessage"] = "Profile updated successfully!";
			}
			else
			{
				TempData["ErrorMessage"] = "Failed to update profile: " + string.Join(", ", result.Errors.Select(e => e.Description));
			}

			return RedirectToAction("Index");
		}
	}
}
