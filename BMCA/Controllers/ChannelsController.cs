using BMCA.Data;
using BMCA.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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

        public IActionResult Show(int _ID)
        {
            Channel _Channel = MyDataBase.Channels.Find(_ID);

            ViewBag.Messages = MyDataBase.Channels
                .Include("Message")
                .Where(m => m.ID == _ID);

            string UserId = MyUserManager.GetUserId(User);

            ViewBag.UserChannels = MyDataBase.BindChannelUserEntries
                .Include(_BindChannelUser => _BindChannelUser.Channel)
                .Where(_BindChannelUser => _BindChannelUser.UserId == UserId)
                .ToList();

            return View(_Channel);
        }

        public IActionResult New()
        {
            return View();
        }

        [HttpPost]
        public IActionResult New(Channel _Channel)
        {
			if (!ModelState.IsValid)
			{
				return View(_Channel);
			}

            try
            {
                MyDataBase.Channels.Add(_Channel);

                TempData["Message"] = "Channel created successfully!";

                MyDataBase.SaveChanges();

                var userId = MyUserManager.GetUserId(User);

                var bindChannelUserEntry = new BindChannelUser
                {
                    IsOwner = true,
                    ChannelId = _Channel.ID,
                    UserId = userId
                };

                MyDataBase.BindChannelUserEntries.Add(bindChannelUserEntry);

                MyDataBase.SaveChanges();

                return Redirect($"/Users/Show/{userId}");
            }
            catch
            {
                return View("Error", new ErrorViewModel { RequestId = "An error occured while trying to create a new channel. Please contact the dev team in order to resolve this issue." });
            }
        }

        [HttpPost]
        [Authorize(Roles = "User, Moderator, Admin")]
        public IActionResult Edit(int _ID, Channel _Channel)
        {
			_Channel.ID = _ID;

            BindChannelUser _BindChannelUser = MyDataBase.BindChannelUserEntries.Include("IsOwner")
                .Where(_BindChannelUser => _BindChannelUser.ChannelId == _ID)
                .First();

            Channel? _OriginalChannel = MyDataBase.Channels.Find(_Channel.ID);

            if (_OriginalChannel == null || _BindChannelUser.IsOwner == false || !User.IsInRole("Admin") || !User.IsInRole("Moderator"))
            {
                return View("Error", new ErrorViewModel { RequestId = "Delete attempt on non existing channel!" });
            }

            if (!ModelState.IsValid)
            {
                return View(_Channel);
            }

            try
            {
                _OriginalChannel.Name = _Channel.Name;
                _OriginalChannel.Description = _Channel.Description;

                MyDataBase.SaveChanges();

                TempData["Message"] = "Channel edited successfully!";

                return Redirect($"/Users/Show/{MyUserManager.GetUserId(User)}");
            }
            catch
            {
                return View("Error", new ErrorViewModel { RequestId = "An error occured while trying to edit the channel. Please contact the dev team in order to resolve this issue." });
            }
        }       

        [HttpPost]
        [Authorize(Roles = "User, Moderator, Admin")]
        public IActionResult Delete(int _ID)
        {
            BindChannelUser _BindChannelUser = MyDataBase.BindChannelUserEntries.Include("IsOwner")
                .Where(_BindChannelUser => _BindChannelUser.ChannelId == _ID)
                .First();

            Channel _Channel = MyDataBase.Channels.Find(_ID);

            if (_Channel == null || _BindChannelUser.IsOwner == false || !User.IsInRole("Admin") || !User.IsInRole("Moderator"))
            {
                return View("Error", new ErrorViewModel { RequestId = "Delete attempt on non existing channel!" });
            }

            try
            {
                MyDataBase.BindChannelUserEntries.RemoveRange(MyDataBase.BindChannelUserEntries.Where(_BindChannelUser => _BindChannelUser.ChannelId == _ID));

                MyDataBase.Channels.Remove(_Channel);

                MyDataBase.SaveChanges();

                TempData["Message"] = "Channel deleted successfully!";

                return RedirectToAction("List", "Users");
            }
            catch
            {
                return View("Error", new ErrorViewModel { RequestId = "An error occured while trying to delete the channel. Please contact the dev team in order to resolve this issue." });
            }
        }   
    }
}
