using W3_test.Data.Entities;

namespace W3_test.Repositories
{
	public interface ICartRepository
	{
		Task<CartEntity> GetByIdAsync(Guid id);
		Task<CartEntity> GetByUserIdAsync(Guid userId);
		Task AddAsync(CartEntity cart);
		Task UpdateAsync(CartEntity cart);
		Task DeleteAsync(Guid id);

		Task<CartEntity> GetByUserIdFirstAsync(Guid userId);
	}
}
