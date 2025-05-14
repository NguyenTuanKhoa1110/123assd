using System.ComponentModel.DataAnnotations;

namespace W3_test.Domain.Models
{
	public class RegisterViewModel
	{
		[Required]
		[EmailAddress]
		public string Email { get; set; }

		[Required]
		[DataType(DataType.Password)]
		public string Password { get; set; }

		[DataType(DataType.Password)]
		[Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
		public string ConfirmPassword { get; set; }

		[Required]
		public string FirstName { get; set; }

		[Required]
		public string LastName { get; set; }

		[Required]
		[Range(1, 120, ErrorMessage = "Age must be between 1 and 120.")]
		public int Age { get; set; }
		public string Address { get; set; }
	}
}

