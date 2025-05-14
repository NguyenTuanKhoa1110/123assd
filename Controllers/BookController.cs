using Microsoft.AspNetCore.Mvc;
using W3_test.Repositories;
using W3_test.Domain.DTOs;
using W3_test.Data.Entities;
namespace W3_test.Controllers
{
	public class BookController : Controller
	{
		private readonly IBookRepository _bookRepository;

		public BookController(IBookRepository bookRepository)
		{
			_bookRepository = bookRepository;
		}

		public async Task<IActionResult> Index()
		{
			var books = await _bookRepository.GetAllAsync();
			var bookDTOs = books.Select(b => new BookDTO
			{
				Id = b.Id,
				Title = b.Title,
				Author = b.Author,
				Description = b.Description,
				
			}).ToList();
			return View(bookDTOs);
		}
		// GET: Book/Create
		public IActionResult Create()
		{
			return View();
		}

		// POST: Book/Create
		[HttpPost]
		//[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(BookDTO dto)
		{
			if (!ModelState.IsValid)
				return View(dto);

			var book = new BookEntity
			{
				Id = Guid.NewGuid(),
				Title = dto.Title,
				Author = dto.Author,
				Description = dto.Description,
				Category = dto.Category,
				Price = dto.Price,
				ImageUrl = dto.ImageUrl
			};

			await _bookRepository.AddAsync(book);
			return RedirectToAction(nameof(Index));
		}

		public async Task<IActionResult> Details(Guid id)
		{
			var book = await _bookRepository.GetByIdAsync(id);
			if (book == null) return NotFound();

			var bookDTO = new BookDTO
			{
				Id = book.Id,
				Title = book.Title,
				Author = book.Author,
				Description = book.Description,
				Category = book.Category,
				Price = book.Price,
				ImageUrl = book.ImageUrl
			};

			return View(bookDTO);
		}
	}
}
