using BMCA.Data;
using BMCA.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Update.Internal;

namespace BMCA.Controllers
{

	public class MessagesController : Controller
	{

		private readonly ApplicationDbContext MyDataBase;
		private readonly UserManager<ApplicationUser> MyUserManager;
		private readonly RoleManager<IdentityRole> MyRoleManager;
		private readonly SignInManager<ApplicationUser> MySignInManager;

		public MessagesController(ApplicationDbContext _MyDataBase, UserManager<ApplicationUser> _MyUserManager, RoleManager<IdentityRole> _MyRoleManager, SignInManager<ApplicationUser> _MySignInManager)
		{
			MyDataBase = _MyDataBase;
			MyUserManager = _MyUserManager;
			MyRoleManager = _MyRoleManager;
			MySignInManager = _MySignInManager;
		}

		[Authorize(Roles = "User,Moderator,Admin")]
		[HttpPost]
		public IActionResult New(Message _Message, int _ChannelId)
		{
			// vezi daca userul face parte din channel
			// adauga channel id pe model

			if (!ModelState.IsValid)
			{
				return View(_Message);
			}

			try
			{
				MyDataBase.Messages.Add(_Message);

				MyDataBase.SaveChanges();

				return Redirect("/Channels/Show/" + _ChannelId);
			}
			catch
			{
				return View("Error", new ErrorViewModel { RequestId = "An error occured while trying to add a message. Please contact the dev team in order to resolve this issue." });
			}
		}

		[Authorize(Roles = "User,Moderator,Admin")]
		[HttpPost]
		public IActionResult Edit(int _ID, Message _Message)
		{
			// verifica daca userul e cel care a creat mesajul

			_Message.ID = _ID;

			Message? _OriginalMessage = MyDataBase.Messages.Find(_ID);

			if (_OriginalMessage == null)
			{
				return View("Error", new ErrorViewModel { RequestId = "Edit attempt on non existing message!" });
			}

			if (!ModelState.IsValid)
			{
				return Redirect("/Channels/Show/" + _OriginalMessage.ChannelId);
			}

			try
			{
				_OriginalMessage.Content = _Message.Content;
				_OriginalMessage.Date = DateTime.Now;

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
