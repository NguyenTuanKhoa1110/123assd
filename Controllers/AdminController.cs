using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using W3_test.Data.Entities;
using W3_test.Domain.Models;
using W3_test.Repositories;
using AutoMapper;
using W3_test.Domain.DTOs;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace W3_test.Controllers
{
	[Authorize(Roles = "Admin,Staff ")]
	public class AdminController : Controller
	{
		private readonly IBookRepository _bookRepository;
		private readonly IOrderRepository _orderRepository;
		private readonly IUserRepository _userRepository;
		private readonly IMapper _mapper;
		private readonly UserManager<AppUser> _userManager;
		private readonly RoleManager<AppRole> _roleManager;

		public AdminController(
			IBookRepository bookRepository,
			IOrderRepository orderRepository,
			IUserRepository userRepository,
			IMapper mapper,
			UserManager<AppUser> userManager,
			RoleManager<AppRole> roleManager)
		{
			_bookRepository = bookRepository;
			_orderRepository = orderRepository;
			_userRepository = userRepository;
			_mapper = mapper;
			_userManager = userManager;
			_roleManager = roleManager;
		}

		public IActionResult Index() => View();

		// BOOKS
		public async Task<IActionResult> ManageBooks()
		{
			var books = await _bookRepository.GetAllAsync();
			var bookModels = _mapper.Map<IEnumerable<BookDTO>>(books);
			return View(bookModels);
		}

		[HttpGet]
		public IActionResult CreateBook() => View();

		[HttpPost]
		public async Task<IActionResult> CreateBook(Book model, IFormFile ImageFile)
		{
			if (!ModelState.IsValid)
				return View(model);

			try
			{
				if (ImageFile != null && ImageFile.Length > 0)
				{
					var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
					var fileExtension = Path.GetExtension(ImageFile.FileName).ToLower();
					if (!allowedExtensions.Contains(fileExtension))
					{
						ModelState.AddModelError("ImageFile", "Only image files (.jpg, .jpeg, .png, .gif) are allowed.");
						return View(model);
					}

					var imagesDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
					if (!Directory.Exists(imagesDir))
						Directory.CreateDirectory(imagesDir);

					var filePath = Path.Combine(imagesDir, ImageFile.FileName);
					using (var stream = new FileStream(filePath, FileMode.Create))
					{
						await ImageFile.CopyToAsync(stream);
					}

					model.ImageUrl = "/images/" + ImageFile.FileName;
				}

				var bookEntity = _mapper.Map<BookEntity>(model);
				await _bookRepository.AddAsync(bookEntity);
				return RedirectToAction("ManageBooks");
			}
			catch (Exception ex)
			{
				ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
				return View(model);
			}
		}

		[HttpGet]
		public async Task<IActionResult> EditBook(Guid id)
		{
			var bookEntity = await _bookRepository.GetByIdAsync(id);
			if (bookEntity == null) return NotFound();

			var bookDto = _mapper.Map<BookDTO>(bookEntity);
			return View(bookDto);
		}

		[HttpPost]
		public async Task<IActionResult> EditBook(Book model)
		{
			if (!ModelState.IsValid) return View(model);

			var bookEntity = await _bookRepository.GetByIdAsync(model.Id);
			if (bookEntity == null) return NotFound();

			_mapper.Map(model, bookEntity);
			await _bookRepository.UpdateAsync(bookEntity);
			return RedirectToAction("ManageBooks");
		}

		[HttpPost]
		public async Task<IActionResult> DeleteBook(Guid id)
		{
			var success = await _bookRepository.DeleteAsync(id);
			return success ? RedirectToAction("ManageBooks") : NotFound();
		}

		// ORDERS
		public async Task<IActionResult> ManageOrders(string status)
		{
			var orders = await _orderRepository.GetAllAsync();
			if (!string.IsNullOrEmpty(status))
				orders = orders.Where(o => o.Status == status);

			var orderModels = _mapper.Map<IEnumerable<OrderDTO>>(orders);
			var statuses = new List<string> { "Pending", "Shipped", "Delivered", "Cancelled" };
			ViewBag.statuses = new SelectList(statuses, status);
			return View(orderModels);
		}

		[HttpGet]
		public async Task<IActionResult> EditOrder(Guid id)
		{
			var orderEntity = await _orderRepository.GetByIdAsync(id);
			if (orderEntity == null) return NotFound();

			var orderModel = _mapper.Map<OrderDTO>(orderEntity);
			return View(orderModel);
		}

		[HttpPost]
		public async Task<IActionResult> EditOrder(OrderDTO model)
		{
			if (!ModelState.IsValid) return View(model);

			var orderEntity = await _orderRepository.GetByIdAsync(model.Id);
			if (orderEntity == null) return NotFound();

			_mapper.Map(model, orderEntity);
			await _orderRepository.UpdateAsync(orderEntity);
			return RedirectToAction("ManageOrders");
		}

		[HttpPost]
		public async Task<IActionResult> DeleteOrder(Guid id)
		{
			var success = await _orderRepository.DeleteAsync(id);
			return success ? RedirectToAction("ManageOrders") : NotFound();
		}

        // USERS
        public async Task<IActionResult> ManageUsers(string query)
        {
            var users = await _userRepository.GetAllAsync();

            string originalQuery = query; // Giữ lại chuỗi gốc để truyền vào ViewData

            if (!string.IsNullOrEmpty(query))
            {
                string loweredQuery = query.ToLower();
                users = users.Where(u =>
                    u.Username?.ToLower().Contains(loweredQuery) == true ||
                    u.Email?.ToLower().Contains(loweredQuery) == true
                );
            }

            var userModels = _mapper.Map<IEnumerable<AppUserDTO>>(users);

            // Load tất cả roles
            var roles = await _roleManager.Roles.ToListAsync();
            ViewBag.Roles = roles;

            // Load role hiện tại của từng user
            var userRolesDict = new Dictionary<Guid, string>();
            foreach (var user in users)
            {
                var appUser = await _userManager.FindByIdAsync(user.Id.ToString());
                if (appUser != null)
                {
                    var rolesList = await _userManager.GetRolesAsync(appUser);
                    userRolesDict[user.Id] = rolesList.FirstOrDefault() ?? "No Role";
                }
            }
            ViewBag.UserRoles = userRolesDict;

            ViewData["SearchQuery"] = originalQuery; 
            return View(userModels);
        }


        [HttpGet]
        public async Task<IActionResult> EditUser(Guid id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null) return NotFound();

            var appUser = await _userManager.FindByIdAsync(user.Id.ToString());
            if (appUser == null) return NotFound();

            var userRoles = await _userManager.GetRolesAsync(appUser);
            var allRoles = await _roleManager.Roles
                .Where(r => r.Name == "Admin" || r.Name == "Staff" || r.Name == "Khách hàng")
                .ToListAsync();

            var model = new EditUserViewModel
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                SelectedRole = userRoles.FirstOrDefault(), // chỉ lấy 1
                AllRoles = allRoles.Select(r => new SelectListItem
                {
                    Value = r.Name,
                    Text = r.Name,
                    Selected = userRoles.Contains(r.Name)
                }).ToList()
            };

            return View(model);
        }

        public async Task<IActionResult> EditUser(EditUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var allRoles = await _roleManager.Roles
                    .Where(r => r.Name == "Admin" || r.Name == "Staff" || r.Name == "Khách hàng")
                    .ToListAsync();

                model.AllRoles = allRoles.Select(r => new SelectListItem
                {
                    Value = r.Name,
                    Text = r.Name,
                    Selected = r.Name == model.SelectedRole
                }).ToList();

                return View(model);
            }

            var user = await _userManager.FindByIdAsync(model.Id.ToString());
            if (user == null) return NotFound();

            user.UserName = model.Username;
            user.Email = model.Email;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                foreach (var error in updateResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                
                var allRoles = await _roleManager.Roles
                    .Where(r => r.Name == "Admin" || r.Name == "Staff" || r.Name == "Khách hàng")
                    .ToListAsync();

                model.AllRoles = allRoles.Select(r => new SelectListItem
                {
                    Value = r.Name,
                    Text = r.Name,
                    Selected = r.Name == model.SelectedRole
                }).ToList();

                return View(model);
            }

            
            var currentRoles = await _userManager.GetRolesAsync(user);

            
            if (!currentRoles.Contains(model.SelectedRole) || currentRoles.Count != 1)
            {
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
                await _userManager.AddToRoleAsync(user, model.SelectedRole);
            }

            TempData["Message"] = "User updated successfully.";
            return RedirectToAction("ManageUsers");
        }



        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null) return NotFound();

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                TempData["Error"] = "Failed to delete user.";
                return RedirectToAction("EditUser", new { id });
            }

            TempData["Message"] = "User deleted successfully.";
            return RedirectToAction("ManageUsers");
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignRole(Guid userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction(nameof(ManageUsers));
            }

            // Không cho tự gán role cho chính mình nếu muốn
            if (user.Id.ToString() == _userManager.GetUserId(User))
            {
                TempData["Error"] = "You cannot change your own role.";
                return RedirectToAction(nameof(ManageUsers));
            }

            if (string.IsNullOrWhiteSpace(roleName) || !await _roleManager.RoleExistsAsync(roleName))
            {
                TempData["Error"] = "Invalid or missing role.";
                return RedirectToAction(nameof(ManageUsers));
            }

            var currentRoles = await _userManager.GetRolesAsync(user);

            if (currentRoles.Contains(roleName) && currentRoles.Count == 1)
            {
                TempData["Message"] = "User already has the selected role.";
                return RedirectToAction(nameof(ManageUsers));
            }

            // Xóa toàn bộ role cũ
            var resultRemove = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            var resultAdd = await _userManager.AddToRoleAsync(user, roleName);

            if (resultRemove.Succeeded && resultAdd.Succeeded)
            {
                TempData["Message"] = $"Successfully assigned role '{roleName}' to {user.Email}.";
            }
            else
            {
                TempData["Error"] = "Failed to update role.";
            }

            return RedirectToAction(nameof(ManageUsers));
        }

    }


}

