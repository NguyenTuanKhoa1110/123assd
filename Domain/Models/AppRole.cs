using Microsoft.AspNetCore.Identity;

namespace W3_test.Domain.Models
{
	public class AppRole : IdentityRole<Guid>
	{
		public string Description { get; set; }
		public AppRole() : base() { }
		public AppRole(string roleName) : base(roleName) { }
	}
}
