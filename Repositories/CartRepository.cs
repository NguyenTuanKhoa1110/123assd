using W3_test.Data;
using W3_test.Data.Entities;
using Microsoft.EntityFrameworkCore;
using W3_test.Domain.Models;
namespace W3_test.Repositories
{
	public class CartRepository : ICartRepository
	{
		private readonly AppDbContext _context;

		public CartRepository(AppDbContext context)
		{
			_context = context;
		}

		public async Task<CartEntity> GetByIdAsync(Guid id)
		{
			return await _context.Carts
				.Include(c => c.Items)
				.ThenInclude(ci => ci.Book)
				.FirstOrDefaultAsync(c => c.Id == id);
		}

		public async Task<CartEntity> GetByUserIdAsync(Guid userId)
		{
			return await _context.Carts
				.Include(c => c.Items)
				.ThenInclude(ci => ci.Book)
				.FirstOrDefaultAsync(c => c.UserId == userId);
		}

		public async Task<CartEntity> GetByUserIdFirstAsync(Guid userId)
		{
			return await _context.Carts
				.FirstOrDefaultAsync(c => c.UserId == userId);
		}

		public async Task AddAsync(CartEntity cart)
		{
			await _context.Carts.AddAsync(cart);
			await _context.SaveChangesAsync();
		}

		public async Task UpdateAsync(CartEntity cart)
		{
			//_context.Carts.Update(cart);
			//await _context.SaveChangesAsync();

			// new
			var existingEntity = _context.Set<CartEntity>().Find(cart.Id); // reload
			if (existingEntity == null)
			{
				throw new Exception("Entity not found");
			}
			_context.Entry(existingEntity).CurrentValues.SetValues(cart);
			_context.SaveChanges();
		}

		public async Task DeleteAsync(Guid id)
		{
			var cart = await GetByIdAsync(id);
			if (cart != null)
			{
				_context.Carts.Remove(cart);
				await _context.SaveChangesAsync();
			}
		}
	}
}
