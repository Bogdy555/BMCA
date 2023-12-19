using BMCA.Data;
using BMCA.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;



namespace BMCA.Controllers
{

	[Authorize(Roles = "Admin")]
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

		public IActionResult List()
		{
			ViewBag.Categories = MyDataBase.Categories;

			if (TempData.ContainsKey("Message"))
			{
				ViewBag.TempMsg = TempData["Message"];
			}

			return View();
		}

		public IActionResult New()
		{
			return View();
		}

		[HttpPost]
		public IActionResult New(Category _Category)
		{
			if (!ModelState.IsValid)
			{
				return View(_Category);
			}

			try
			{
				MyDataBase.Categories.Add(_Category);

				MyDataBase.SaveChanges();

				TempData["TempMsg"] = "New category added!";

				return RedirectToAction("List");
			}
			catch
			{
				return View("Error", new ErrorViewModel { RequestId = "An error occured while trying to add the category. Please contact the dev team in order to resolve this issue." });
			}
		}

		public IActionResult Edit(int _ID)
		{
			try
			{
				return View(MyDataBase.Categories.Find(_ID));
			}
			catch
			{
				return View("Error", new ErrorViewModel { RequestId = "Edit attempt on non existing category!" });
			}
		}

		[HttpPost]
		public IActionResult Edit(int _ID, Category _Category)
		{
			_Category.ID = _ID;

			Category? _OriginalCategory = MyDataBase.Categories.Find(_ID);

			if (_OriginalCategory == null)
			{
				return View("Error", new ErrorViewModel { RequestId = "Edit attempt on non existing category!" });
			}

			if (!ModelState.IsValid)
			{
				return View(_Category);
			}

			try
			{
				_OriginalCategory.Name = _Category.Name;

				MyDataBase.SaveChanges();

				TempData["TempMsg"] = "Edit completed!";

				return RedirectToAction("List");
			}
			catch
			{
				return View("Error", new ErrorViewModel { RequestId = "An error occured while trying to edit the category. Please contact the dev team in order to resolve this issue." });
			}
		}

		[HttpPost]
		public IActionResult Delete(int _ID)
		{
			Category? _Category = MyDataBase.Categories.Find(_ID);

			if (_Category == null)
			{
				return View("Error", new ErrorViewModel { RequestId = "Delete attempt on non existing category!" });
			}

			try
			{
				MyDataBase.Categories.Remove(_Category);

				MyDataBase.SaveChanges();

				return RedirectToAction("List");
			}
			catch
			{
				return View("Error", new ErrorViewModel { RequestId = "An error occured while trying to delete the category. Please contact the dev team in order to resolve this issue." } );
			}
		}

	}

}
