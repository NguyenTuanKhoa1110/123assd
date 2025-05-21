using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using W3_test.Data.Entities;
using W3_test.Domain.DTOs;
using W3_test.Domain.Models;
using W3_test.Repositories;

namespace W3_test.Controllers
{
    [Authorize(Roles = "Admin,Staff")]
    public class AdminController : Controller
    {
        private readonly IBookRepository _bookRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            IBookRepository bookRepository,
            IOrderRepository orderRepository,
            IUserRepository userRepository,
            IMapper mapper,
            UserManager<AppUser> userManager,
            RoleManager<AppRole> roleManager,
            ILogger<AdminController> logger)
        {
            _bookRepository = bookRepository;
            _orderRepository = orderRepository;
            _userRepository = userRepository;
            _mapper = mapper;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        public IActionResult Index() => View();

        // ----- BOOKS -----
        public async Task<IActionResult> ManageBooks()
        {
            var books = await _bookRepository.GetAllAsync();
            var bookDtos = _mapper.Map<IEnumerable<BookDTO>>(books);
            return View(bookDtos);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult CreateBook()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateBook(BookDTO model, IFormFile imageFile)
        {
            ModelState.Remove("ImageUrl");

            _logger.LogInformation("Received book creation request: Title={Title}, Author={Author}, ImageFile={ImageFile}", model?.Title, model?.Author, imageFile?.FileName);

            if (imageFile == null || imageFile.Length == 0)
            {
                ModelState.AddModelError("imageFile", "Vui lòng chọn một tệp hình ảnh.");
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                _logger.LogWarning("Model state invalid: {Errors}", string.Join(", ", errors));
                return View(model);
            }

            try
            {
                const int maxFileSize = 5 * 1024 * 1024;
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var allowedContentTypes = new[] { "image/jpeg", "image/png", "image/gif" };
                var fileExtension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(fileExtension) || !allowedContentTypes.Contains(imageFile.ContentType))
                {
                    ModelState.AddModelError("imageFile", "Chỉ cho phép tệp hình ảnh (.jpg, .jpeg, .png, .gif).");
                    _logger.LogWarning("Invalid image file extension or content type: {FileName}", imageFile.FileName);
                    return View(model);
                }

                if (imageFile.Length > maxFileSize)
                {
                    ModelState.AddModelError("imageFile", "Kích thước tệp không được vượt quá 5MB.");
                    _logger.LogWarning("Image file too large: {FileSize} bytes", imageFile.Length);
                    return View(model);
                }

                var imagesPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                if (!Directory.Exists(imagesPath))
                {
                    Directory.CreateDirectory(imagesPath);
                }

                var fileName = Guid.NewGuid() + fileExtension;
                var fullPath = Path.Combine(imagesPath, fileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                model.ImageUrl = "/images/" + fileName;
                _logger.LogInformation("Image saved at {ImageUrl}", model.ImageUrl);

                var bookEntity = _mapper.Map<BookEntity>(model);
                await _bookRepository.AddAsync(bookEntity);

                TempData["SuccessMessage"] = "Sách đã được tạo thành công!";
                return RedirectToAction(nameof(ManageBooks));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo sách");
                ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi khi tạo sách. Vui lòng thử lại.");
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
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditBook(Book model)
        {
            if (!ModelState.IsValid) return View(model);

            var bookEntity = await _bookRepository.GetByIdAsync(model.Id);
            if (bookEntity == null) return NotFound();

            _mapper.Map(model, bookEntity);
            await _bookRepository.UpdateAsync(bookEntity);
            return RedirectToAction(nameof(ManageBooks));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteBook(Guid id)
        {
            var success = await _bookRepository.DeleteAsync(id);
            return success ? RedirectToAction(nameof(ManageBooks)) : NotFound();
        }

        // ----- ORDERS -----
        public async Task<IActionResult> ManageOrders(string status)
        {
            var orders = await _orderRepository.GetAllAsync();
            if (!string.IsNullOrEmpty(status))
            {
                orders = orders.Where(o => o.Status == status);
            }

            var orderDtos = _mapper.Map<IEnumerable<OrderDTO>>(orders);

            var statuses = new List<string> { "Pending", "Shipped", "Delivered", "Cancelled" };
            ViewBag.Statuses = new SelectList(statuses, status);

            return View(orderDtos);
        }

        [HttpGet]
        public async Task<IActionResult> EditOrder(Guid id)
        {
            var orderEntity = await _orderRepository.GetByIdAsync(id);
            if (orderEntity == null) return NotFound();

            var orderDto = _mapper.Map<OrderDTO>(orderEntity);
            return View(orderDto);
        }

        [HttpPost]
        public async Task<IActionResult> EditOrder(OrderDTO model)
        {
            if (!ModelState.IsValid) return View(model);

            var orderEntity = await _orderRepository.GetByIdAsync(model.Id);
            if (orderEntity == null) return NotFound();

            _mapper.Map(model, orderEntity);
            await _orderRepository.UpdateAsync(orderEntity);
            return RedirectToAction(nameof(ManageOrders));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteOrder(Guid id)
        {
            var success = await _orderRepository.DeleteAsync(id);
            return success ? RedirectToAction(nameof(ManageOrders)) : NotFound();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult PlaceOrder(CheckoutViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }



            return RedirectToAction("OrderConfirmation");
        }

        // ----- USERS -----
        public async Task<IActionResult> ManageUsers(string query)
        {
            var users = await _userRepository.GetAllAsync();

            string originalQuery = query;

            if (!string.IsNullOrEmpty(query))
            {
                var loweredQuery = query.ToLowerInvariant();
                users = users.Where(u =>
                    (u.UserName?.ToLowerInvariant().Contains(loweredQuery) ?? false) ||
                    (u.Email?.ToLowerInvariant().Contains(loweredQuery) ?? false));
            }

            var userDtos = _mapper.Map<IEnumerable<AppUserDTO>>(users);

            // Load all roles
            var roles = await _roleManager.Roles.ToListAsync();
            ViewBag.Roles = roles;

            // Load current roles of each user
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
            return View(userDtos);
        }

        [HttpGet]
        public async Task<IActionResult> EditUser(Guid id)
        {
            var userEntity = await _userRepository.GetByIdAsync(id);
            if (userEntity == null) return NotFound();

            var appUser = await _userManager.FindByIdAsync(userEntity.Id.ToString());
            if (appUser == null) return NotFound();

            var userRoles = await _userManager.GetRolesAsync(appUser);
            var allRoles = await _roleManager.Roles
                .Where(r => r.Name == "Admin" || r.Name == "Staff")
                .ToListAsync();

            var userClaims = await _userManager.GetClaimsAsync(appUser);
            var allPermissions = GetAllPermissions();
            var isAdmin = userRoles.Contains("Admin");
            var model = new EditUserViewModel
            {
                Id = userEntity.Id,
                Username = userEntity.UserName,
                Email = userEntity.Email,
                SelectedRole = userRoles.FirstOrDefault(),
                AllRoles = allRoles.Select(r => new SelectListItem
                {
                    Value = r.Name,
                    Text = r.Name
                }).ToList(),
                AllPermissions = allPermissions.Select(p => new PermissionItemViewModel
                {
                    Name = p,
                    IsAssigned = userClaims.Any(c => c.Type == "Permission" && c.Value == p)
                }).ToList()
            };

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(EditUserViewModel model, string[] SelectedPermissions)
        {
            if (!ModelState.IsValid)
            {
                // Nạp lại dữ liệu nếu có lỗi nhập
                model.AllRoles = await _roleManager.Roles
                    .Where(r => r.Name == "Admin" || r.Name == "Staff")
                    .Select(r => new SelectListItem { Value = r.Name, Text = r.Name })
                    .ToListAsync();

                var allPermissions = GetAllPermissions();
                model.AllPermissions = allPermissions.Select(p => new PermissionItemViewModel
                {
                    Name = p,
                    IsAssigned = SelectedPermissions.Contains(p)
                }).ToList();

                return View(model);
            }

            var appUser = await _userManager.FindByIdAsync(model.Id.ToString());
            if (appUser == null) return NotFound();

            // Cập nhật role
            var currentRoles = await _userManager.GetRolesAsync(appUser);
            await _userManager.RemoveFromRolesAsync(appUser, currentRoles);
            await _userManager.AddToRoleAsync(appUser, model.SelectedRole);

            // Cập nhật claims
            var currentClaims = await _userManager.GetClaimsAsync(appUser);
            foreach (var claim in currentClaims.Where(c => c.Type == "Permission"))
            {
                await _userManager.RemoveClaimAsync(appUser, claim);
            }

            foreach (var permission in SelectedPermissions ?? Array.Empty<string>())
            {
                await _userManager.AddClaimAsync(appUser, new System.Security.Claims.Claim("Permission", permission));
            }

            TempData["SuccessMessage"] = "Cập nhật người dùng thành công.";
            return RedirectToAction(nameof(ManageUsers));
        }


        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var success = await _userRepository.DeleteAsync(id);
            return success ? RedirectToAction(nameof(ManageUsers)) : NotFound();
        }
    
         private List<string> GetAllPermissions()
        {
            return new List<string>
        {
            "Book.Create", "Book.Read", "Book.Update", "Book.Delete",
        "Order.View", "Order.Update",
        "User.Manage"

            };
        }
    }
}
