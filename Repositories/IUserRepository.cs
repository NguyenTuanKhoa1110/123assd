using W3_test.Domain.DTOs;

namespace W3_test.Repositories
{
	public interface IUserRepository
	{
		Task<IEnumerable<AppUserDTO>> GetAllAsync();
		Task<AppUserDTO> GetByIdAsync(Guid id);
		Task<AppUserDTO> GetByUsernameAsync(string username);
		Task AddAsync(AppUserDTO user);
		Task UpdateAsync(AppUserDTO user);
		Task<bool> DeleteAsync(Guid id);
		
	}
}
