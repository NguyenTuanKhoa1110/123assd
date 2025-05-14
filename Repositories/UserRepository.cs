using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using W3_test.Data;
using W3_test.Domain.DTOs;
using W3_test.Domain.Models; 

namespace W3_test.Repositories
{
	public class UserRepository : IUserRepository
	{
		private readonly UserManager<AppUser> _userManager;
		private readonly AppDbContext _context;

		public UserRepository(UserManager<AppUser> userManager, AppDbContext context)
		{
			_userManager = userManager;
			_context = context;
		}

		public async Task<IEnumerable<AppUserDTO>> GetAllAsync()
		{
			var users = await _userManager.Users.ToListAsync();
			return users.Select(user => new AppUserDTO
			{
				Id = user.Id,
				Username = user.UserName,
				Email = user.Email
			});
		}

		public async Task<AppUserDTO> GetByIdAsync(Guid id)
		{
			var user = await _userManager.FindByIdAsync(id.ToString());
			if (user == null) return null;

			return new AppUserDTO
			{
				Id = user.Id,
				Username = user.UserName,
				Email = user.Email
			};
		}

		public async Task<AppUserDTO> GetByUsernameAsync(string username)
		{
			var user = await _userManager.FindByNameAsync(username);
			if (user == null) return null;

			return new AppUserDTO
			{
				Id = user.Id,
				Username = user.UserName,
				Email = user.Email
			};
		}

		public async Task AddAsync(AppUserDTO userDto)
		{
			var user = new AppUser
			{
				UserName = userDto.Username,
				Email = userDto.Email
				
			};

			
			await _userManager.CreateAsync(user, "DefaultPassword123!");
		}

		public async Task UpdateAsync(AppUserDTO userDto)
		{
			var user = await _userManager.FindByIdAsync(userDto.Id.ToString());
			if (user != null)
			{
				user.UserName = userDto.Username;
				user.Email = userDto.Email;
				await _userManager.UpdateAsync(user);
			}
		}

		public async Task<bool> DeleteAsync(Guid id)
		{
			var user = await _userManager.FindByIdAsync(id.ToString());
			if (user != null)
			{
				var result = await _userManager.DeleteAsync(user);
				return result.Succeeded;
			}
			return false;
		}
	}
}
