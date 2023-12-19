using BMCA.Data;
using BMCA.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace BMCA.Controllers
{

	public class UsersController : Controller
	{

		private readonly ApplicationDbContext MyDataBase;
		private readonly UserManager<ApplicationUser> MyUserManager;
		private readonly RoleManager<IdentityRole> MyRoleManager;
		private readonly SignInManager<ApplicationUser> MySignInManager;

		public UsersController(ApplicationDbContext _MyDataBase, UserManager<ApplicationUser> _MyUserManager, RoleManager<IdentityRole> _MyRoleManager, SignInManager<ApplicationUser> _MySignInManager)
		{
			MyDataBase = _MyDataBase;
			MyUserManager = _MyUserManager;
			MyRoleManager = _MyRoleManager;
			MySignInManager = _MySignInManager;
		}

		[Authorize(Roles = "Admin")]
		public IActionResult List()
		{
			ViewBag.Users = MyDataBase.Users;

			if (TempData.ContainsKey("Message"))
			{
				ViewBag.TempMsg = TempData["Message"];
			}

			return View();
		}

		[Authorize(Roles = "Guest, User, Moderator, Admin")]
		public IActionResult Show(string _ID)
		{
			try
			{
				return View(MyDataBase.Users.Find(_ID));
			}
			catch
			{
				return View("Error", new ErrorViewModel { RequestId = "Could not find the user that you are looking for!" });
			}
		}

		[Authorize(Roles = "Guest, User, Moderator, Admin")]
		[HttpPost]
		public IActionResult Delete(string _ID)
		{
			ApplicationUser? _DeleteUser = MyDataBase.Users.Find(_ID);

			if (_DeleteUser == null)
			{
				return View("Error", new ErrorViewModel { RequestId = "Delete attempt on non existing user!" });
			}

			if (_DeleteUser.Id != MyUserManager.GetUserId(User) && !User.IsInRole("Admin"))
			{
				return View("Error", new ErrorViewModel { RequestId = "Permission denied!" });
			}

			try
			{
				MyDataBase.Users.Remove(_DeleteUser);

				MyDataBase.SaveChanges();

				return RedirectToAction("List");
			}
			catch
			{
				return View("Error", new ErrorViewModel { RequestId = "An error occured while trying to delete the user. Please contact the dev team in order to resolve this issue." } );
			}
		}

	}

}
