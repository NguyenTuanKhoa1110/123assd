using W3_test.Data.Entities;

namespace W3_test.Repositories
{
	public interface IBookRepository
	{
		Task<BookEntity> GetByIdAsync(Guid id);
		Task<IEnumerable<BookEntity>> GetAllAsync();
		Task AddAsync(BookEntity book);
		Task UpdateAsync(BookEntity book);
		Task<bool> DeleteAsync(Guid id);
		
	}
}
