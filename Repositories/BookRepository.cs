using W3_test.Data;
using W3_test.Data.Entities;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using W3_test.Domain.DTOs;
namespace W3_test.Repositories
{
	public class BookRepository : IBookRepository
	{
		private readonly AppDbContext _context;

		public BookRepository(AppDbContext context)
		{
			_context = context;
		}

		public async Task<BookEntity> GetByIdAsync(Guid id)
		{
			return await _context.Books.FindAsync(id);
		}

		public async Task<IEnumerable<BookEntity>> GetAllAsync()
		{
			return await _context.Books.ToListAsync();
		}

		public async Task AddAsync(BookEntity book)
		{
			await _context.Books.AddAsync(book);
			await _context.SaveChangesAsync();
		}

		public async Task UpdateAsync(BookEntity book)
		{
			var existingBook = await _context.Books.FindAsync(book.Id);
			if (existingBook == null)
			{
				throw new InvalidOperationException($"Book with ID {book.Id} not found.");
			}
			_context.Books.Update(book);
			await _context.SaveChangesAsync();
		}

		public async Task<bool> DeleteAsync(Guid id)
		{
			var book = await GetByIdAsync(id);
			if (book != null)
			{
				_context.Books.Remove(book);
				await _context.SaveChangesAsync();
				return true;
			}
			return false;
		}
	}
}
