using W3_test.Data;
using W3_test.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace W3_test.Repositories
{
	public class OrderRepository : IOrderRepository
	{
		private readonly AppDbContext _context;

		public OrderRepository(AppDbContext context)
		{
			_context = context;
		}

		public async Task<OrderEntity> GetByIdAsync(Guid id)
		{
			return await _context.Orders
				.Include(o => o.Items)
				.ThenInclude(oi => oi.Book)
				.FirstOrDefaultAsync(o => o.Id == id);
		}

		public async Task<IEnumerable<OrderEntity>> GetByUserIdAsync(Guid userId)
		{
			return await _context.Orders
				.Include(o => o.Items)
				.ThenInclude(oi => oi.Book)
				.Where(o => o.UserId == userId)
				.ToListAsync();
		}

		public async Task<IEnumerable<OrderEntity>> GetAllAsync()
		{
			return await _context.Orders
				.Include(o => o.Items)
				.ThenInclude(oi => oi.Book)
				.ToListAsync();
		}

		public async Task AddAsync(OrderEntity order)
		{
			await _context.Orders.AddAsync(order);
			await _context.SaveChangesAsync();
		}

		public async Task UpdateAsync(OrderEntity order)
		{
			_context.Orders.Update(order);
			await _context.SaveChangesAsync();
		}

		public async Task<bool> DeleteAsync(Guid id)
		{
			var order = await GetByIdAsync(id);
			if (order != null)
			{
				_context.Orders.Remove(order);
				await _context.SaveChangesAsync();
				return true;
			}
			return false;
		}
	}
}
