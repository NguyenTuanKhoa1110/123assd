using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using W3_test.Domain.Models;

namespace W3_test.Data
{
    public static class SeedData
    {
        public static async Task SeedRolesAsync(RoleManager<AppRole> roleManager)
        {
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new AppRole
                {
                    Name = "Admin",
                    Description = "Admin role"
                });
            }

            if (!await roleManager.RoleExistsAsync("Staff"))
            {
                await roleManager.CreateAsync(new AppRole
                {
                    Name = "Staff",
                    Description = "Staff role"
                });
            }
        }

        public static async Task SeedAdminAsync(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager)
        {
            var adminEmail = "admin@example.com";
            var admin = await userManager.FindByEmailAsync(adminEmail);

            if (admin == null)
            {
                admin = new AppUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "Admin",
                    LastName = "User",
                    Age = 30,
                    Address = "Admin City",
                    EmailConfirmed = true
                };

                var createResult = await userManager.CreateAsync(admin, "Admin@123");
                if (!createResult.Succeeded) return;
            }

            if (!await userManager.IsInRoleAsync(admin, "Admin"))
            {
                await userManager.AddToRoleAsync(admin, "Admin");
            }

            // Gán quyền cho Admin
            await AddPermissionsToUser(userManager, admin, new List<string>
            {
            Permissions.Users.View,
            Permissions.Users.Edit,
            Permissions.Users.Delete,
            Permissions.Users.ManageRoles,   
            Permissions.Books.Create,
            Permissions.Books.View,
            Permissions.Books.Update,
            Permissions.Books.Delete
});
        }

        public static async Task SeedStaffAsync(UserManager<AppUser> userManager)
        {
            var staffEmail = "staff@example.com";
            var staff = await userManager.FindByEmailAsync(staffEmail);

            if (staff == null)
            {
                staff = new AppUser
                {
                    UserName = staffEmail,
                    Email = staffEmail,
                    FirstName = "Staff",
                    LastName = "User",
                    Age = 25,
                    Address = "Staff Town",
                    EmailConfirmed = true
                };

                var createResult = await userManager.CreateAsync(staff, "Staff@123");
                if (!createResult.Succeeded) return;
            }

            if (!await userManager.IsInRoleAsync(staff, "Staff"))
            {
                await userManager.AddToRoleAsync(staff, "Staff");
            }

            // Gán quyền cho Staff (không có quyền Delete)
            await AddPermissionsToUser(userManager, staff, new List<string>
            {
                Permissions.Books.Create,
                Permissions.Books.View,
                Permissions.Books.Update,
                Permissions.Books.Delete
            });
        }

        private static async Task AddPermissionsToUser(UserManager<AppUser> userManager, AppUser user, List<string> permissions)
        {
            var currentClaims = await userManager.GetClaimsAsync(user);

            foreach (var permission in permissions)
            {
                if (!currentClaims.Any(c => c.Type == "Permission" && c.Value == permission))
                {
                    await userManager.AddClaimAsync(user, new Claim("Permission", permission));
                }
            }
        }
    }
}
