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

		protected override void OnModelCreating(ModelBuilder builder)
		{
			base.OnModelCreating(builder);

			builder.Entity<BindChannelUser>().HasKey(ac => new { ac.ChannelId, ac.UserId });

			builder.Entity<BindChannelUser>().HasOne(ac => ac.Channel).WithMany(ac => ac.BindChannelUser).HasForeignKey(ac => ac.ChannelId);
			builder.Entity<BindChannelUser>().HasOne(ac => ac.User).WithMany(ac => ac.BindChannelUser).HasForeignKey(ac => ac.UserId);

			builder.Entity<BindChannelCategory>().HasKey(ac => new { ac.ChannelId, ac.CategoryId });

			builder.Entity<BindChannelCategory>().HasOne(ac => ac.Channel).WithMany(ac => ac.BindChannelCategory).HasForeignKey(ac => ac.ChannelId);
			builder.Entity<BindChannelCategory>().HasOne(ac => ac.Category).WithMany(ac => ac.BindChannelCategory).HasForeignKey(ac => ac.CategoryId);
		}

	}
}
