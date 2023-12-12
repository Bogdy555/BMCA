using BMCA.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BMCA.Data
{
	public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
	{
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
			: base(options)
		{
		}

		public DbSet<ApplicationUser> Users { get; set; }
		public DbSet<BindChannelCategory> BindChannelCategorieEntries { get; set; }
		public DbSet<BindChannelUser> BindChannelUserEntries { get; set; }
		public DbSet<Category> Categories { get; set; }
		public DbSet<Channel> Channels { get; set; }
		public DbSet<Message> Messages { get; set; }

	}
}
