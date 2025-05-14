using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using W3_test.Data.Entities;
using W3_test.Repositories;
using W3_test.Domain.DTOs;
using System.Linq;
using Microsoft.AspNetCore.Antiforgery;

namespace W3_test.Controllers
{
	[Authorize]
	public class CartController : Controller
	{
		private readonly ICartRepository _cartRepository;
		private readonly IBookRepository _bookRepository;

		public CartController(ICartRepository cartRepository, IBookRepository bookRepository)
		{
			_cartRepository = cartRepository;
			_bookRepository = bookRepository;
		}

		public async Task<IActionResult> Index()
		{
			var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
			var cart = await _cartRepository.GetByUserIdAsync(userId);
			if (cart == null)
			{
				cart = new CartEntity { Id = Guid.NewGuid(), UserId = userId };
				await _cartRepository.AddAsync(cart);
			}

			var cartDTO = new CartDTO
			{
				Id = cart.Id,
				UserId = cart.UserId,
				Items = cart.Items?.Select(item => new CartItemDTO
				{
					Id = item.Id,
					BookId = item.BookId,
					BookTitle = item.Book?.Title ?? "",
					Quantity = item.Quantity,
					Price = item.Price
				}).ToList() ?? new List<CartItemDTO>()
			};

			return View(cartDTO);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> AddToCart(Guid id, int quantity = 1)
		{
			var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
			var cart = await _cartRepository.GetByUserIdAsync(userId);
			var book = await _bookRepository.GetByIdAsync(id);

			if (book == null || quantity <= 0 || book.Stock < quantity)
			{
				TempData["ErrorMessage"] = "Invalid book or quantity.";
				return RedirectToAction("Index", "Book");
			}

			if (cart == null)
			{
				cart = new CartEntity { Id = Guid.NewGuid(), UserId = userId };
				await _cartRepository.AddAsync(cart);
			}

			var cartItem = cart.Items.FirstOrDefault(ci => ci.BookId == id);
			if (cartItem == null)
			{
				cartItem = new CartItemEntity
				{
					//Id = Guid.NewGuid(),
					CartId = cart.Id,
					BookId = id,
					Quantity = quantity,
					Price = book.Price
				};
				cart.Items.Add(cartItem);
			}
			else
			{
				cartItem.Quantity += quantity;
			}

			await _cartRepository.UpdateAsync(cart);
			TempData["SuccessMessage"] = "Book added to cart successfully!";
			return RedirectToAction("Index", "Cart");
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> RemoveFromCart(Guid cartItemId)
		{
			var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
			var cart = await _cartRepository.GetByUserIdAsync(userId);
			if (cart == null) return NotFound();

			var cartItem = cart.Items.FirstOrDefault(ci => ci.Id == cartItemId);
			if (cartItem != null)
			{
				cart.Items.Remove(cartItem);
				await _cartRepository.UpdateAsync(cart);
			}
			return RedirectToAction("Index", "Book");
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> UpdateQuantity(Guid cartItemId, int quantity)
		{
			var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
			var cart = await _cartRepository.GetByUserIdAsync(userId);
			if (cart == null) return NotFound();

			var cartItem = cart.Items.FirstOrDefault(ci => ci.Id == cartItemId);
			if (cartItem != null && quantity > 0)
			{
				cartItem.Quantity = quantity;
				await _cartRepository.UpdateAsync(cart);
			}
			return RedirectToAction("Index", "Book");
		}
		[HttpGet]
		public async Task<IActionResult> Checkout()
		{
			var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
			var cart = await _cartRepository.GetByUserIdAsync(userId);

			if (cart == null || cart.Items == null || !cart.Items.Any())
			{
				TempData["ErrorMessage"] = "Your cart is empty.";
				return RedirectToAction("Index");
			}

			var cartDTO = new CartDTO
			{
				Id = cart.Id,
				UserId = cart.UserId,
				Items = cart.Items.Select(item => new CartItemDTO
				{
					Id = item.Id,
					BookId = item.BookId,
					Quantity = item.Quantity,
					Price = item.Price,
					Book = new BookDTO
					{
						Id = item.Book.Id,
						Title = item.Book.Title,
						Price = item.Book.Price
					}
				}).ToList()
			};

			return View(cartDTO);
		}
	}
}