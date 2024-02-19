using BMCA.Data;
using BMCA.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Debugger.Contracts.EditAndContinue;

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
			Channel? _Channel = MyDataBase.Channels.Include("BindsChannelUser").Include("Messages").Include("Messages.User").Where(m => m.ID == _ID).First();

			if (_Channel == null || _Channel.BindsChannelUser == null || _Channel.Messages == null)
			{
				return View("Error", new ErrorViewModel { RequestId = "Could not find the channel!" });
			}

			if (!User.IsInRole("Admin"))
			{
				bool _Found = false;

				foreach (BindChannelUser _Bind in _Channel.BindsChannelUser)
				{
					if (_Bind.UserId == MyUserManager.GetUserId(User))
					{
						_Found = true;
						break;
					}
				}

				if (!_Found)
				{
					return View("Error", new ErrorViewModel { RequestId = "Access denied!" });
				}
			}

			List<BindChannelUser> _BindChannelUser = MyDataBase.BindChannelUserEntries
				.Where(_BindChannelUser => _BindChannelUser.UserId == MyUserManager.GetUserId(User))
				.ToList();

			List<int> _ChannelIds = new List<int>();
			foreach (BindChannelUser _Bind in _BindChannelUser)
			{
				_ChannelIds.Add(_Bind.ChannelId);
			}

			if (_Search == null && User.IsInRole("Admin"))
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

			if (!User.IsInRole("Admin"))
			{
				bool _Found = false;

				foreach (BindChannelUser _Bind in _Channel.BindsChannelUser)
				{
					if (_Bind.UserId == MyUserManager.GetUserId(User))
					{
						_Found = true;
						break;
					}
				}

				if (!_Found)
				{
					return View("Error", new ErrorViewModel { RequestId = "Access denied!" });
				}
			}

			foreach (BindChannelUser _Bind in _Channel.BindsChannelUser)
			{
				if (_Bind.IsOwner)
				{
					ViewBag.Owner = _Bind.UserId;
					break;
				}
			}

			List<BindChannelUser> _BindChannelUser = MyDataBase.BindChannelUserEntries
				.Where(_BindChannelUser => _BindChannelUser.UserId == MyUserManager.GetUserId(User))
				.ToList();

			List<int> _ChannelIds = new List<int>();
			foreach (BindChannelUser _Bind in _BindChannelUser)
			{
				_ChannelIds.Add(_Bind.ChannelId);
			}

			if (_Search == null && User.IsInRole("Admin"))
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
		public IActionResult RemoveMember(int _ID, string? _UserId)
		{
			if (_UserId == null)
			{
				return View("Error", new ErrorViewModel { RequestId = "No user ID supplied for kick operation!" });
			}

			Channel? _Channel = MyDataBase.Channels.Include("BindsChannelUser").Where(m => m.ID == _ID).First();

			if (_Channel == null || _Channel.BindsChannelUser == null)
			{
				return View("Error", new ErrorViewModel { RequestId = "No channel found for the ID!" });
			}

			if (!User.IsInRole("Moderator") && !User.IsInRole("Admin"))
			{
				foreach (BindChannelUser _Bind in _Channel.BindsChannelUser)
				{
					if (_Bind.IsOwner)
					{
						if (_Bind.UserId != MyUserManager.GetUserId(User) && MyUserManager.GetUserId(User) != _UserId)
						{
							return View("Error", new ErrorViewModel { RequestId = "Access denied!" });
						}

						if (_Bind.UserId == _UserId)
						{
							return View("Error", new ErrorViewModel { RequestId = "Did you want to delete the chat???" });
						}
					}
				}
			}
			else
			{
				foreach (BindChannelUser _Bind in _Channel.BindsChannelUser)
				{
					if (_Bind.IsOwner)
					{
						if (_Bind.UserId == _UserId)
						{
							return View("Error", new ErrorViewModel { RequestId = "Did you want to delete the chat???" });
						}
					}
				}
			}

			bool _Found = false;

			foreach (BindChannelUser _Bind in _Channel.BindsChannelUser)
			{
				if (_Bind.UserId == _UserId)
				{
					_Found = true;
					break;
				}
			}

			if (!_Found)
			{
				return View("Error", new ErrorViewModel { RequestId = "No such user in this chat dude..." });
			}

			try
			{
				BindChannelUser? _OriginalBind = MyDataBase.BindChannelUserEntries.Where(m => m.UserId == _UserId && m.ChannelId == _ID).First();

				MyDataBase.BindChannelUserEntries.Remove(_OriginalBind);

				MyDataBase.SaveChanges();
			}
			catch
			{
				return View("Error", new ErrorViewModel { RequestId = "An error occured while trying to remove a member from the chat. Please contact the dev team in order to resolve this issue." });
			}

			if (_UserId == MyUserManager.GetUserId(User))
			{
				return Redirect("/Users/Show/" + MyUserManager.GetUserId(User));
			}
			else
			{
				return Redirect("/Channels/Display/" + _ID);
			}
		}

		[Authorize(Roles = "User,Moderator,Admin")]
		public IActionResult AddMember(int _ID)
		{
			Channel? _Channel = MyDataBase.Channels.Include("BindsChannelUser").Where(m => m.ID == _ID).First();

			if (_Channel == null || _Channel.BindsChannelUser == null)
			{
				return View("Error", new ErrorViewModel { RequestId = "This channel does not exist!" });
			}

			if (!User.IsInRole("Admin"))
			{
				foreach (BindChannelUser _Bind in _Channel.BindsChannelUser)
				{
					if (_Bind.IsOwner)
					{
						if (_Bind.UserId != MyUserManager.GetUserId(User))
						{
							return View("Error", new ErrorViewModel { RequestId = "Access denied!" });
						}

						break;
					}
				}
			}

			return View(_Channel);
		}

		[Authorize(Roles = "User,Moderator,Admin")]
		[HttpPost]
		public IActionResult AddMember(int _ID, string? _UserName)
		{
			if (_UserName == null)
			{
				return View();
			}

			Channel? _Channel = MyDataBase.Channels.Include("BindsChannelUser").Where(m => m.ID == _ID).First();

			if (_Channel == null || _Channel.BindsChannelUser == null)
			{
				return View("Error", new ErrorViewModel { RequestId = "No such channel out there..." });
			}

			if (!User.IsInRole("Admin"))
			{
				foreach (BindChannelUser _Bind in _Channel.BindsChannelUser)
				{
					if (_Bind.IsOwner)
					{
						if (_Bind.UserId != MyUserManager.GetUserId(User))
						{
							return View("Error", new ErrorViewModel { RequestId = "Access denied!" });
						}

						break;
					}
				}
			}

			ApplicationUser? _AddedUser = MyDataBase.AppUsers.Include("BindsChannelUser").Where(m => m.UserName == _UserName).First();

			if (_AddedUser == null || _AddedUser.BindsChannelUser == null)
			{
				return View("Error", new ErrorViewModel { RequestId = "No user with that username!" });
			}

			if (!_AddedUser.BindsChannelUser.Contains(new BindChannelUser { UserId = _AddedUser.Id, ChannelId = _ID }))
			{
				try
				{
					BindChannelUser _NewBind = new BindChannelUser { UserId = _AddedUser.Id, ChannelId = _ID };

					MyDataBase.BindChannelUserEntries.Add(_NewBind);

					MyDataBase.SaveChanges();
				}
				catch
				{
					return View("Error", new ErrorViewModel { RequestId = "An error occured while trying to add a new member. Please contact the dev team in order to resolve this issue." });
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

			if (User.IsInRole("Moderator"))
			{
				bool _Found = false;

				foreach (BindChannelUser _Bind in _Channel.BindsChannelUser)
				{
					if (_Bind.UserId == MyUserManager.GetUserId(User))
					{
						_Found = true;
						break;
					}
				}

				if (!_Found)
				{
					return View("Error", new ErrorViewModel { RequestId = "Access denied!" });
				}
			}
			else if (User.IsInRole("User"))
			{
				foreach (BindChannelUser _Bind in _Channel.BindsChannelUser)
				{
					if (_Bind.IsOwner)
					{
						if (_Bind.UserId != MyUserManager.GetUserId(User))
						{
							return View("Error", new ErrorViewModel { RequestId = "Access denied!" });
						}
					}
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

			if (User.IsInRole("Moderator"))
			{
				bool _Found = false;

				foreach (BindChannelUser _Bind in _Channel.BindsChannelUser)
				{
					if (_Bind.UserId == MyUserManager.GetUserId(User))
					{
						_Found = true;
						break;
					}
				}

				if (!_Found)
				{
					return View("Error", new ErrorViewModel { RequestId = "Access denied!" });
				}
			}
			else if (User.IsInRole("User"))
			{
				foreach (BindChannelUser _Bind in _Channel.BindsChannelUser)
				{
					if (_Bind.IsOwner)
					{
						if (_Bind.UserId != MyUserManager.GetUserId(User))
						{
							return View("Error", new ErrorViewModel { RequestId = "Access denied!" });
						}
					}
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

				return Redirect("/Channels/Display/" + _ID);
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

			if (User.IsInRole("Moderator"))
			{
				bool _Found = false;

				foreach (BindChannelUser _Bind in _Channel.BindsChannelUser)
				{
					if (_Bind.UserId == MyUserManager.GetUserId(User))
					{
						_Found = true;
						break;
					}
				}

				if (!_Found)
				{
					return View("Error", new ErrorViewModel { RequestId = "Access denied!" });
				}
			}
			else if (User.IsInRole("User"))
			{
				foreach (BindChannelUser _Bind in _Channel.BindsChannelUser)
				{
					if (_Bind.IsOwner)
					{
						if (_Bind.UserId != MyUserManager.GetUserId(User))
						{
							return View("Error", new ErrorViewModel { RequestId = "Access denied!" });
						}
					}
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
