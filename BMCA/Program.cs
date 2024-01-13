using BMCA.Data;
using BMCA.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
	options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
	.AddRoles<IdentityRole>()
	.AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();

var app = builder.Build();

SeedData.Initialize(app.Services.CreateScope().ServiceProvider);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseMigrationsEndPoint();
}
else
{
	app.UseExceptionHandler("/Home/Error");
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(name: "HomePage", pattern: "", defaults: new { controller = "Home", action = "Index" });

app.MapControllerRoute(name: "CategoriesList", pattern: "Categories/List", defaults: new { controller = "Categories", action = "List" });
app.MapControllerRoute(name: "CategoriesNew", pattern: "Categories/New", defaults: new { controller = "Categories", action = "New" });
app.MapControllerRoute(name: "CategoriesEdit", pattern: "Categories/Edit/{_ID}", defaults: new { controller = "Categories", action = "Edit" });
app.MapControllerRoute(name: "CategoriesDelete", pattern: "Categories/Delete/{_ID}", defaults: new { controller = "Categories", action = "Delete" });

app.MapControllerRoute(name: "ChannelsShow", pattern: "Channels/Show/{_ID}", defaults: new { controller = "Channels", action = "Show" });
app.MapControllerRoute(name: "ChannelsNew", pattern: "Channels/New", defaults: new { controller = "Channels", action = "New" });
app.MapControllerRoute(name: "ChannelsEdit", pattern: "Channels/Edit/{_ID}", defaults: new { controller = "Channels", action = "Edit" });
app.MapControllerRoute(name: "ChannelsDelete", pattern: "Channels/Delete/{_ID}", defaults: new { controller = "Channels", action = "Delete" });

app.MapControllerRoute(name: "UsersList", pattern: "Users/List", defaults: new { controller = "Users", action = "List" });
app.MapControllerRoute(name: "UsersShow", pattern: "Users/Show/{_ID}", defaults: new { controller = "Users", action = "Show" });
app.MapControllerRoute(name: "UsersDelete", pattern: "Users/Delete/{_ID}", defaults: new { controller = "Users", action = "Delete" });

app.MapControllerRoute(name: "MessagesNew", pattern: "Messages/New", defaults: new { controller = "Messages", action = "New" });
app.MapControllerRoute(name: "MessagesEdit", pattern: "Messages/Edit/{_ID}", defaults: new { controller = "Messages", action = "Edit" });
app.MapControllerRoute(name: "MessagesDelete", pattern: "Messages/Delete/{_ID}", defaults: new { controller = "Messages", action = "Delete" });

app.MapRazorPages();

app.Run();
