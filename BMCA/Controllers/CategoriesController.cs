using BMCA.Data;
using BMCA.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;



namespace BMCA.Controllers
{

	public class CategoriesController : Controller
	{

		private readonly ApplicationDbContext MyDataBase;
		private readonly UserManager<ApplicationUser> MyUserManager;
		private readonly RoleManager<IdentityRole> MyRoleManager;
		private readonly SignInManager<ApplicationUser> MySignInManager;

		public CategoriesController(ApplicationDbContext _MyDataBase, UserManager<ApplicationUser> _MyUserManager, RoleManager<IdentityRole> _MyRoleManager, SignInManager<ApplicationUser> _MySignInManager)
		{
			MyDataBase = _MyDataBase;
			MyUserManager = _MyUserManager;
			MyRoleManager = _MyRoleManager;
			MySignInManager = _MySignInManager;
		}

		[Authorize(Roles = "Admin")]
		public IActionResult Index()
		{
			ViewBag.Categories = MyDataBase.Categories;

			if (TempData.ContainsKey("Message"))
			{
				ViewBag.TempMsg = TempData["Message"];
			}

			return View();
		}

	}

}
