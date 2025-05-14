using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using W3_test.Data.Entities;
using W3_test.Repositories;
using W3_test.Domain.DTOs;
using Microsoft.EntityFrameworkCore;
using W3_test.Data.Entities;
using System.Linq;

namespace W3_test.Controllers
{
	[Authorize]
	public class OrderController : Controller
	{
		private readonly IOrderRepository _orderRepository;
		private readonly ICartRepository _cartRepository;
		private readonly IUserRepository _userRepository;
		private readonly IBookRepository _bookRepository;

		public OrderController(
			IOrderRepository orderRepository,
			ICartRepository cartRepository,
			IUserRepository userRepository,
			IBookRepository bookRepository)
		{
			_orderRepository = orderRepository;
			_cartRepository = cartRepository;
			_userRepository = userRepository;
			_bookRepository = bookRepository;
		}

		public async Task<IActionResult> Index()
		{
			var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
			var orders = await _orderRepository.GetByUserIdAsync(userId);

			// Map from OrderEntity to OrderDTO
			var orderDtos = new List<OrderDTO>();
			foreach (var order in orders)
			{
				var user = await _userRepository.GetByIdAsync(order.UserId);
				var orderDto = new OrderDTO
				{
					Id = order.Id,
					User = user,
					OrderDate = order.OrderDate,
					TotalAmount = order.TotalAmount,
					Status = "Pending",
					OrderItems = order.Items?.Select(item => new OrderItemDTO
					{
						Id = item.Id,
						OrderId = item.OrderId,
						BookId =item.BookId,
						Book = null,
						Quantity = item.Quantity,
						Price = item.Price
					}).ToList()
				};

				// Nhận thông tin mỗi order
				if (orderDto.OrderItems != null)
				{
					foreach (var item in orderDto.OrderItems)
					{
						var book = await _bookRepository.GetByIdAsync(item.BookId);
						item.Book = book != null ? new BookDTO
						{
							Id = book.Id,
							Title = book.Title,
							Author = book.Author,
							Price = book.Price,
							Category = book.Category,
							Description = book.Description
						} : null;
					}
				}

				orderDtos.Add(orderDto);
			}

			return View(orderDtos);
		}

		[HttpPost]
		public async Task<IActionResult> Checkout(string address, string paymentMethod)
		{
			var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
			var cart = await _cartRepository.GetByUserIdAsync(userId);
			if (cart == null || !cart.Items.Any())
			{
				return BadRequest("Cart is empty.");
			}

			var order = new OrderEntity
			{
				Id = Guid.NewGuid(),  
				UserId = userId,
				OrderDate = DateTime.UtcNow,
				TotalAmount = cart.Items.Sum(ci => ci.Price * ci.Quantity),
				Status = "Pending",
				ShippingAddress = address,
				PaymentMethod = paymentMethod,
				Items = cart.Items.Select(ci => new OrderItemEntity
				{
					Id = Guid.NewGuid(),  
					OrderId = Guid.NewGuid(),  
					BookId = ci.BookId,  
					Quantity = ci.Quantity,
					Price = ci.Price
				}).ToList()
			};

			await _orderRepository.AddAsync(order);

			// xóa gio hàng sau khi đặt hàng thành công
			await _cartRepository.DeleteAsync(cart.Id);

			return RedirectToAction("ConfirmOrder", new { id = order.Id });
		}

		public async Task<IActionResult> Details(Guid id)
		{
			var order = await _orderRepository.GetByIdAsync(id);
			if (order == null) return NotFound();

			// Map từ OrderEntity đến OrderDTO
			var user = await _userRepository.GetByIdAsync(order.UserId);
			var orderDto = new OrderDTO
			{
				Id = order.Id,
				User = user,
				OrderDate = order.OrderDate,
				TotalAmount = order.TotalAmount,
				Status = order.Status,
				ShippingAddress = order.ShippingAddress,
				PaymentMethod = order.PaymentMethod,
				OrderItems = order.Items?.Select(item => new OrderItemDTO
				{
					Id = item.Id,
					OrderId = item.OrderId,
					BookId = item.BookId,
					Book = null,  // update sau
					Quantity = item.Quantity,
					Price = item.Price
				}).ToList()
			};
				
			//
			if (orderDto.OrderItems != null)
			{
				foreach (var item in orderDto.OrderItems)
				{
					var book = await _bookRepository.GetByIdAsync(item.BookId);
					item.Book = book != null ? new BookDTO
					{
						Id = book.Id,
						Title = book.Title,
						Author = book.Author,
						Price = book.Price,
						Category = book.Category,
						Description = book.Description
					} : null;
				}
			}

			return View(orderDto);
		}

		public async Task<IActionResult> ConfirmOrder(Guid id)
		{
			var order = await _orderRepository.GetByIdAsync(id);
			if (order == null) return NotFound();

			
			var user = await _userRepository.GetByIdAsync(order.UserId);
			var orderDto = new OrderDTO
			{
				Id = order.Id,
				User = user,
				OrderDate = order.OrderDate,
				TotalAmount = order.TotalAmount,
				Status = order.Status,
				ShippingAddress = order.ShippingAddress,
				PaymentMethod = order.PaymentMethod,
				OrderItems = order.Items?.Select(item => new OrderItemDTO
				{
					Id = item.Id,
					OrderId = item.OrderId,
					BookId = item.BookId,
					Book = null,
					Quantity = item.Quantity,
					Price = item.Price
				}).ToList()
			};

			return View("ConfirmOrder", orderDto);
		}
	}
}
