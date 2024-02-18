using BMCA.Data;
using BMCA.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

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
			ViewBag.Users = MyDataBase.AppUsers;

			if (TempData.ContainsKey("Message"))
			{
				ViewBag.TempMsg = TempData["Message"];
			}

			return View();
		}

		[Authorize(Roles = "User,Moderator,Admin")]
		public IActionResult Show(string _ID, string? _Search)
		{
			List<BindChannelUser> _BindChannelUser = MyDataBase.BindChannelUserEntries
				.Where(_BindChannelUser => _BindChannelUser.UserId == _ID)
				.ToList();

			List<int> _ChannelIds = new List<int>();
			foreach (BindChannelUser _Bind in _BindChannelUser)
			{
				_ChannelIds.Add(_Bind.ChannelId);
			}

			if (_Search == null && (User.IsInRole("Admin") || User.IsInRole("Moderator")))
            {
                ViewBag.UserChannels = MyDataBase.Channels;
			}
			else if (_Search == null)
			{
				ViewBag.UserChannels = MyDataBase.Channels
					.Where(_Channel => _ChannelIds.Contains(_Channel.ID))
					.ToList();
			}
			else
			{
				ViewBag.UserChannels = MyDataBase.Channels
					.Where(_Channel => _ChannelIds.Contains(_Channel.ID))
					.Where(_Channel => _Channel.Name.ToUpper().Contains(_Search.ToUpper()))
					.ToList();
			}

			try
			{
				return View(MyDataBase.AppUsers.Find(_ID));
			}
			catch
			{
				return View("Error", new ErrorViewModel { RequestId = "Could not find the user that you are looking for!" });
			}
		}

		[Authorize(Roles = "User,Moderator,Admin")]
		[HttpPost]
		public IActionResult Delete(string _ID)
		{
			ApplicationUser? _DeleteUser = MyDataBase.AppUsers.Find(_ID);

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
				MyDataBase.AppUsers.Remove(_DeleteUser);

				MyDataBase.SaveChanges();

				return RedirectToAction("List");
			}
			catch
			{
				return View("Error", new ErrorViewModel { RequestId = "An error occured while trying to delete the user. Please contact the dev team in order to resolve this issue." } );
			}
		}

		[Authorize(Roles = "Admin")]
		[HttpPost]
		public IActionResult Promote(string _ID)
		{
            ApplicationUser? _PromoteUser = MyDataBase.AppUsers.Find(_ID);

            if (_PromoteUser == null)
			{
                return View("Error", new ErrorViewModel { RequestId = "Promote attempt on non existing user!" });
            }

			if (_PromoteUser.Id != MyUserManager.GetUserId(User) && !User.IsInRole("Admin"))
			{
				return View("Error", new ErrorViewModel { RequestId = "Permission denied!" });
			}

			try
			{

                return RedirectToAction("List");
            }
			catch
			{
				return View("Error", new ErrorViewModel { RequestId = "An error occured while trying to promote a user. Please contact the dev team in order to resolve this issue." } );
            }
        }
	}

}
