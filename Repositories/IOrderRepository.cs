using W3_test.Data.Entities;

namespace W3_test.Repositories
{
	public interface IOrderRepository
	{
		Task<OrderEntity> GetByIdAsync(Guid id);
		Task<IEnumerable<OrderEntity>> GetByUserIdAsync(Guid userId);
		Task<IEnumerable<OrderEntity>> GetAllAsync(); 

		Task AddAsync(OrderEntity order);
		Task UpdateAsync(OrderEntity order);
		Task<bool> DeleteAsync(Guid id);
	}
}
