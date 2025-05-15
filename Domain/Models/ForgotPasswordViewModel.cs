using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace W3_test.Domain.Models
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [NotNull]
        public string Email { get; set; }
    }

}
