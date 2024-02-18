using BMCA.Data;
using BMCA.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BMCA.Controllers
{
	public class ChannelsController : Controller
	{
		private readonly ApplicationDbContext MyDataBase;
		private readonly UserManager<ApplicationUser> MyUserManager;
		private readonly RoleManager<IdentityRole> MyRoleManager;
		private readonly SignInManager<ApplicationUser> MySignInManager;

		public ChannelsController(ApplicationDbContext _MyDataBase, UserManager<ApplicationUser> _MyUserManager, RoleManager<IdentityRole> _MyRoleManager, SignInManager<ApplicationUser> _MySignInManager)
		{
			MyDataBase = _MyDataBase;
			MyUserManager = _MyUserManager;
			MyRoleManager = _MyRoleManager;
			MySignInManager = _MySignInManager;
		}

		[Authorize(Roles = "User,Moderator,Admin")]
		public IActionResult Show(int _ID, string? _Search)
		{
			Channel? _Channel = MyDataBase.Channels.Include("BindsChannelUser").Include("Messages").Where(m => m.ID == _ID).First();

			if (_Channel == null || _Channel.BindsChannelUser == null || _Channel.Messages == null)
			{
				return View("Error", new ErrorViewModel { RequestId = "Could not find the channel!" });
			}

			bool _Found = false;

			foreach (BindChannelUser _Bind in _Channel.BindsChannelUser)
			{
				if (_Bind.UserId == MyUserManager.GetUserId(User) || User.IsInRole("Admin") || User.IsInRole("Moderator"))
				{
					_Found = true;
					break;
				}
			}

			if (!_Found)
			{
				return Redirect("/Users/Show/" + MyUserManager.GetUserId(User));
			}

			List<BindChannelUser> _BindChannelUser = MyDataBase.BindChannelUserEntries
				.Where(_BindChannelUser => _BindChannelUser.UserId == MyUserManager.GetUserId(User))
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

			ViewBag.ChatMessages = _Channel.Messages.ToList().OrderBy(m => m.Date);

			return View(_Channel);
		}

		[Authorize(Roles = "User,Moderator,Admin")]
		public IActionResult Display(int _ID, string? _Search)
		{
			Channel? _Channel = MyDataBase.Channels.Include("BindsChannelUser").Include("Category").Where(m => m.ID == _ID).First();

			if (_Channel == null || _Channel.BindsChannelUser == null || _Channel.Category == null)
			{
				return View("Error", new ErrorViewModel { RequestId = "Could not find the channel!" });
			}

			bool _Found = false;

			foreach (BindChannelUser _Bind in _Channel.BindsChannelUser)
			{
				if (_Bind.UserId == MyUserManager.GetUserId(User) || User.IsInRole("Admin") || User.IsInRole("Moderator"))
				{
					_Found = true;
					ViewBag.Owner = _Bind.UserId;
				}
			}

			if (!_Found)
			{
				return Redirect("/Users/Show/" + MyUserManager.GetUserId(User));
			}

			List<BindChannelUser> _BindChannelUser = MyDataBase.BindChannelUserEntries
				.Where(_BindChannelUser => _BindChannelUser.UserId == MyUserManager.GetUserId(User))
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

			List<ApplicationUser> _AllMembers = new List<ApplicationUser>();

			foreach (BindChannelUser _Bind in _Channel.BindsChannelUser)
			{
				ApplicationUser? _Member = MyDataBase.AppUsers.Find(_Bind.UserId);

				if (_Member == null)
				{
					continue;
				}

				_AllMembers.Add(_Member);
			}

			ViewBag.AllMembers = _AllMembers;

			return View(_Channel);
		}

		[Authorize(Roles = "User,Moderator,Admin")]
		[HttpPost]
		public IActionResult RemoveMemer(int _ID, int? _UserId)
		{
			// TO DO

			if (_UserId == null)
			{
				return Redirect("/Channels/Display/" + _ID);
			}

			Channel? _Channel = MyDataBase.Channels.Include("BindsChannelUser").Where(m => m.ID == _ID).First();

			if (_Channel == null || _Channel.BindsChannelUser == null)
			{
				return View("Error", new ErrorViewModel { RequestId = "Can't remove a member from an unexisting channel!" });
			}

			foreach (BindChannelUser _Bind in _Channel.BindsChannelUser)
			{
				if (_Bind.IsOwner)
				{
					if (_Bind.UserId != MyUserManager.GetUserId(User))
					{
						return Redirect("/Channels/Display/" + _ID);
					}

					break;
				}
			}

			foreach (BindChannelUser _Bind in _Channel.BindsChannelUser)
			{
				if (_Bind.UserId != MyUserManager.GetUserId(User))
				{
					return Redirect("/Channels/Display/" + _ID);
				}
			}

			return Redirect("/Channels/Display/" + _ID);
		}

		[Authorize(Roles = "User,Moderator,Admin")]
		public IActionResult New()
		{
			ViewBag.CategoriesList = GetAllCategories();

			return View();
		}

		[HttpPost]
		[Authorize(Roles = "User,Moderator,Admin")]
		public IActionResult New(Channel _Channel)
		{
			if (!ModelState.IsValid)
			{
				ViewBag.CategoriesList = GetAllCategories();

				return View(_Channel);
			}

			try
			{
				MyDataBase.Channels.Add(_Channel);

				MyDataBase.SaveChanges();

				MyDataBase.BindChannelUserEntries.Add(new BindChannelUser { IsOwner = true, ChannelId = _Channel.ID, UserId = MyUserManager.GetUserId(User) });

				MyDataBase.SaveChanges();

				return Redirect("/Channels/Show/" + _Channel.ID);
			}
			catch
			{
				return View("Error", new ErrorViewModel { RequestId = "An error occured while trying to create a new channel. Please contact the dev team in order to resolve this issue." });
			}
		}

		[Authorize(Roles = "User,Moderator,Admin")]
		public IActionResult Edit(int _ID)
		{
			Channel? _Channel = MyDataBase.Channels.Include("BindsChannelUser").Where(m => m.ID == _ID).First();

			if (_Channel == null || _Channel.BindsChannelUser == null)
			{
				return View("Error", new ErrorViewModel { RequestId = "Edit attempt on non existing channel!" });
			}

			foreach (BindChannelUser _Bind in _Channel.BindsChannelUser)
			{
				if (_Bind.IsOwner)
				{
					if (_Bind.UserId != MyUserManager.GetUserId(User))
					{
						return Redirect("/Channels/Show/" + _ID);
					}

					break;
				}
			}

			ViewBag.CategoriesList = GetAllCategories();

			return View(_Channel);
		}

		[HttpPost]
		[Authorize(Roles = "User,Moderator,Admin")]
		public IActionResult Edit(int _ID, Channel _Channel)
		{
			_Channel.ID = _ID;

			Channel? _OriginalChannel = MyDataBase.Channels.Include("BindsChannelUser").Where(m => m.ID == _ID).First();

			if (_OriginalChannel == null || _OriginalChannel.BindsChannelUser == null)
			{
				return View("Error", new ErrorViewModel { RequestId = "Edit attempt on non existing channel!" });
			}

			foreach (BindChannelUser _Bind in _OriginalChannel.BindsChannelUser)
			{
				if (_Bind.IsOwner)
				{
					if (_Bind.UserId != MyUserManager.GetUserId(User))
					{
						return Redirect("/Channels/Show/" + _ID);
					}

					break;
				}
			}

			if (!ModelState.IsValid)
			{
				ViewBag.CategoriesList = GetAllCategories();

				return View(_Channel);
			}

			try
			{
				_OriginalChannel.Name = _Channel.Name;
				_OriginalChannel.Description = _Channel.Description;
				_OriginalChannel.CategoryId = _Channel.CategoryId;

				MyDataBase.SaveChanges();

				return Redirect("/Channels/Show/" + _ID);
			}
			catch
			{
				return View("Error", new ErrorViewModel { RequestId = "An error occured while trying to edit the channel. Please contact the dev team in order to resolve this issue." });
			}
			}

		[HttpPost]
		[Authorize(Roles = "User,Moderator,Admin")]
		public IActionResult Delete(int _ID)
		{
			Channel? _Channel = MyDataBase.Channels.Include("BindsChannelUser").Where(m => m.ID == _ID).First();

			if (_Channel == null || _Channel.BindsChannelUser == null)
			{
				return View("Error", new ErrorViewModel { RequestId = "Delete attempt on non existing channel!" });
			}

			foreach (BindChannelUser _Bind in _Channel.BindsChannelUser)
			{
				if (_Bind.IsOwner)
				{
					if (_Bind.UserId != MyUserManager.GetUserId(User))
					{
						return Redirect("/Channels/Show/" + _ID);
					}

					break;
				}
			}

			try
			{
				MyDataBase.Channels.Remove(_Channel);

				MyDataBase.SaveChanges();

				return Redirect("/Users/Show/" + MyUserManager.GetUserId(User));
			}
			catch
			{
				return View("Error", new ErrorViewModel { RequestId = "An error occured while trying to delete the channel. Please contact the dev team in order to resolve this issue." });
			}
		}

		[NonAction]
		public IEnumerable<SelectListItem> GetAllCategories()
		{
			List<SelectListItem> _SelectList = new List<SelectListItem>();

			List<Category> _Categories = MyDataBase.Categories.ToList();

			foreach (Category _Category in _Categories)
			{
				_SelectList.Add(new SelectListItem { Value = _Category.ID.ToString(), Text = _Category.Name });
			}

			return _SelectList;
		}
	}
}
