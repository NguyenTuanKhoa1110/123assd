using Microsoft.AspNetCore.Identity;
using System.Security;

namespace W3_test.Domain.Models
{
    public class RolePermission
    {
        public Guid RoleId { get; set; }           
        public AppRole Role { get; set; } = null!;

        public Guid PermissionId { get; set; }
        public Permission Permission { get; set; } = null!;
    }
}
