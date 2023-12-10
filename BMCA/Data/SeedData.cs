using BMCA.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BMCA.Data
{

    public class SeedData
    {

        public static void Initialize(IServiceProvider _ServiceProvider)
        {
            var _Context = new ApplicationDbContext(_ServiceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>());

            if (_Context.Roles.Any())
            {
                return;
            }

            string _GUID_Admin_Role = Guid.NewGuid().ToString();

            string _GUID_Admin = Guid.NewGuid().ToString();

            _Context.Roles.AddRange
            (
                new IdentityRole { Id = _GUID_Admin_Role, Name = "Admin", NormalizedName = "ADMIN" }
            );

            var _Hasher = new PasswordHasher<ApplicationUser>();

            _Context.Users.AddRange
            (
                new ApplicationUser
                {
                    Id = _GUID_Admin,
                    UserName = "admin@test.com",
                    EmailConfirmed = true,
                    NormalizedEmail = "ADMIN@TEST.COM",
                    Email = "admin@test.com",
                    NormalizedUserName = "ADMIN@TEST.COM",
                    PasswordHash = _Hasher.HashPassword(null, "WAdhiawu_!$$#224$daksno")
                }
            );

            _Context.UserRoles.AddRange
            (
                new IdentityUserRole<string>
                {
                    RoleId = _GUID_Admin_Role,
                    UserId = _GUID_Admin
                }
            );

            _Context.SaveChanges();
        }

    }

}
