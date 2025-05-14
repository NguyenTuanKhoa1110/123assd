using Microsoft.AspNetCore.Identity;

namespace W3_test.Domain.Models
{
	public class AppUser : IdentityUser<Guid>
	{

		public string FirstName { get; set; }
		public string LastName { get; set; }
		
		public int Age { get; set; }
		public string Address { get; set; }
		

	}
}
