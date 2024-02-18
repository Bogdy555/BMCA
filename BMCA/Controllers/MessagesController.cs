using BMCA.Data;
using BMCA.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Update.Internal;

namespace BMCA.Controllers
{

	public class MessagesController : Controller
	{

		private readonly ApplicationDbContext MyDataBase;
		private readonly UserManager<ApplicationUser> MyUserManager;
		private readonly RoleManager<IdentityRole> MyRoleManager;
		private readonly SignInManager<ApplicationUser> MySignInManager;
		private readonly IWebHostEnvironment WebEnv;

		public MessagesController(ApplicationDbContext _MyDataBase, UserManager<ApplicationUser> _MyUserManager, RoleManager<IdentityRole> _MyRoleManager, SignInManager<ApplicationUser> _MySignInManager, IWebHostEnvironment _WebEnv)
		{
			MyDataBase = _MyDataBase;
			MyUserManager = _MyUserManager;
			MyRoleManager = _MyRoleManager;
			MySignInManager = _MySignInManager;
			WebEnv = _WebEnv;
		}

		[Authorize(Roles = "User,Moderator,Admin")]
		[HttpPost]
		public IActionResult New(Message _Message, IFormFile? _File)
		{
			Channel? _Channel = MyDataBase.Channels.Include("BindsChannelUser").Where(m => m.ID == _Message.ChannelId).First();

			if (_Channel == null || _Channel.BindsChannelUser == null)
			{
				return View("Error", new ErrorViewModel { RequestId = "The channel does not exist" });
			}

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
				return View("Error", new ErrorViewModel { RequestId = "Access denied" });
			}

			_Message.Date = DateTime.Now;
			_Message.UserId = MyUserManager.GetUserId(User);

			if (_File != null)
			{
				if (_File.Length == 0)
				{
					return View("Error", new ErrorViewModel { RequestId = "No empty files allowed here!" });
				}

				try
				{
					if (!Directory.Exists(Path.Combine(WebEnv.WebRootPath, "media")))
					{
						Directory.CreateDirectory(Path.Combine(WebEnv.WebRootPath, "media"));
					}

					string _FileName = BitConverter.ToString(System.Security.Cryptography.SHA256.Create().ComputeHash(_File.OpenReadStream())).Replace("-", "").ToLowerInvariant() + Path.GetExtension(_File.FileName);

					FileStream _FileStream = new FileStream(Path.Combine(WebEnv.WebRootPath, "media", _FileName), FileMode.Create);

					_File.CopyTo(_FileStream);

					_FileStream.Close();

					if (_File.ContentType.Contains("image"))
					{
						_Message.FilePath = "/media/" + _FileName;
						_Message.FileType = "image/" + Path.GetExtension(_FileName).Remove(0, 1);
					}
					else if (_File.ContentType.Contains("video"))
					{
						_Message.FilePath = "/media/" + _FileName;
						_Message.FileType = "video/" + Path.GetExtension(_FileName).Remove(0, 1);
					}
					else
					{
						return View("Error", new ErrorViewModel { RequestId = "Only images and videos allowed" });
					}
				}
				catch
				{
					return View("Error", new ErrorViewModel { RequestId = "An error occured while trying to add the message. Please contact the dev team in order to resolve this issue." });
				}
			}

			ModelState.Clear();
			TryValidateModel(_Message);

			if (!ModelState.IsValid || (_Message.Content == null && _File == null))
			{
				return Redirect("/Channels/Show/" + _Message.ChannelId);
			}

			try
			{
				MyDataBase.Messages.Add(_Message);

				MyDataBase.SaveChanges();

				return Redirect("/Channels/Show/" + _Message.ChannelId);
			}
			catch
			{
				return View("Error", new ErrorViewModel { RequestId = "An error occured while trying to add a message. Please contact the dev team in order to resolve this issue." });
			}
		}

		[Authorize(Roles = "User,Moderator,Admin")]
		public IActionResult Edit(int _ID)
		{
			Message? _Message = MyDataBase.Messages.Find(_ID);

			if (_Message == null)
			{
				return View("Error", new ErrorViewModel { RequestId = "Edit attempt on non existing message!" });
			}

			if (_Message.UserId != MyUserManager.GetUserId(User) && !User.IsInRole("Admin") && !User.IsInRole("Moderator"))
			{
				return View("Error", "Access denied!");
			}

			return View(_Message);
		}

		[Authorize(Roles = "User,Moderator,Admin")]
		[HttpPost]
		public IActionResult Edit(int _ID, Message _Message, IFormFile? _File)
		{
			_Message.ID = _ID;

			Message? _OriginalMessage = MyDataBase.Messages.Find(_ID);

			if (_OriginalMessage == null)
			{
				return View("Error", new ErrorViewModel { RequestId = "Edit attempt on non existing message!" });
			}

			if (_OriginalMessage.UserId != MyUserManager.GetUserId(User) && !User.IsInRole("Moderator") && !User.IsInRole("Admin"))
			{
				return View("Error", new ErrorViewModel { RequestId = "Access denied" });
			}

			_Message.UserId = _OriginalMessage.UserId;

			if (_File != null)
			{
				if (_File.Length == 0)
				{
					return View("Error", new ErrorViewModel { RequestId = "No empty files allowed here!" });
				}

				try
				{
					if (!Directory.Exists(Path.Combine(WebEnv.WebRootPath, "media")))
					{
						Directory.CreateDirectory(Path.Combine(WebEnv.WebRootPath, "media"));
					}

					string _FileName = BitConverter.ToString(System.Security.Cryptography.SHA256.Create().ComputeHash(_File.OpenReadStream())).Replace("-", "").ToLowerInvariant() + Path.GetExtension(_File.FileName);

					FileStream _FileStream = new FileStream(Path.Combine(WebEnv.WebRootPath, "media", _FileName), FileMode.Create);

					_File.CopyTo(_FileStream);

					_FileStream.Close();

					if (_File.ContentType.Contains("image"))
					{
						_Message.FilePath = "/media/" + _FileName;
						_Message.FileType = "image/" + Path.GetExtension(_FileName).Remove(0, 1);
					}
					else if (_File.ContentType.Contains("video"))
					{
						_Message.FilePath = "/media/" + _FileName;
						_Message.FileType = "video/" + Path.GetExtension(_FileName).Remove(0, 1);
					}
					else
					{
						return View("Error", new ErrorViewModel { RequestId = "Only images and videos allowed" });
					}
				}
				catch
				{
					return View("Error", new ErrorViewModel { RequestId = "An error occured while trying to add the message. Please contact the dev team in order to resolve this issue." });
				}
			}

			ModelState.Clear();
			TryValidateModel(_Message);

			if (!ModelState.IsValid || (_Message.Content == null && _File == null))
			{
				return View(_Message);
			}

			try
			{
				_OriginalMessage.Content = _Message.Content;
				_OriginalMessage.FilePath = _Message.FilePath;
				_OriginalMessage.FileType = _Message.FileType;

				MyDataBase.SaveChanges();

				return Redirect("/Channels/Show/" + _OriginalMessage.ChannelId);
			}
			catch
			{
				return View("Error", new ErrorViewModel { RequestId = "An error occured while trying to edit the message. Please contact the dev team in order to resolve this issue." });
			}
		}

		[Authorize(Roles = "User,Moderator,Admin")]
		[HttpPost]
		public IActionResult Delete(int _ID)
		{
			Message? _Message = MyDataBase.Messages.Find(_ID);

			if (_Message == null)
			{
				return View("Error", new ErrorViewModel { RequestId = "Delete attempt on non existing message!" });
			}

			if (!User.IsInRole("Admin") && !User.IsInRole("Moderator") && _Message.UserId != MyUserManager.GetUserId(User))
			{
				return View("Error", new ErrorViewModel { RequestId = "Access denied!" });
			}

			try
			{
				int _ChannelId = _Message.ChannelId;

				MyDataBase.Messages.Remove(_Message);

				MyDataBase.SaveChanges();

				return Redirect("/Channels/Show/" + _ChannelId);
			}
			catch
			{
				return View("Error", new ErrorViewModel { RequestId = "An error occured while trying to delete the message. Please contact the dev team in order to resolve this issue." } );
			}
		}

	}

}
