using Bongo.Data;
using Bongo.Models;
using Bongo.Models.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Bongo.Controllers
{
    [Authorize]
    public class UserReviewController : Controller
    {
        private readonly IRepositoryWrapper _repo;
        private readonly UserManager<BongoUser> _userManager;

        public UserReviewController(IRepositoryWrapper repo, UserManager<BongoUser> userManager)
        {
            _repo = repo;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult AddReview()
        {
            UserReview model = _repo.UserReview.FindAll().FirstOrDefault(r => r.Username == User.Identity.Name);
            return View(model ?? new UserReview { Username = User.Identity.Name});
        }
        [HttpPost]
        public async Task<IActionResult> AddReview(UserReview model)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(model.Username);
                model.ReviewDate = DateTime.Now.ToLocalTime();
                model.Email = user.Email;
                if (model.ReviewId != 0)
                {
                    _repo.UserReview.Update(model);
                    TempData["Message"] = "Review updated successfully. Thank you";
                }
                else
                {
                    _repo.UserReview.Create(model);
                    TempData["Message"] = "Review submited successfully. Thank you";
                }
                _repo.SaveChanges();
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Someting went wrong while saving. Please try again later.😔");
            }
            var newModel = _repo.UserReview.FindAll().FirstOrDefault(r => r.Username.Equals(model.Username));   
            return View(newModel ?? model);
        }
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult ListReviews()
        {
            return View(_repo.UserReview.FindAll().OrderBy(u => u.Username));
        }
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult ListUsers()
        {
            return View(_userManager.Users.OrderBy(u => u.UserName));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(string id)
        {
            var user = _userManager.Users.FirstOrDefault(u => u.Id == id);
            if (user != null)
            {
                try
                {
                    await _userManager.DeleteAsync(user);
                    IEnumerable<ModuleColor> moduleColors = _repo.ModuleColor.GetByCondition(mc => mc.Username == user.UserName);
                    foreach (var module in moduleColors)
                        _repo.ModuleColor.Delete(module);
                    var table = _repo.Timetable.FindAll().FirstOrDefault(t => t.Username == user.UserName);
                    if (table != null)
                        _repo.Timetable.Delete(table);

                    _repo.SaveChanges();
                    TempData["Message"] = $"{user.UserName} has been deleted successfully!";
                }
                catch (Exception e)
                {
                    TempData["Message"] = $"Something went wrong!\nMessage: {e.ToString()}";
                }
            }
            return RedirectToAction("ListUsers");
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult DeleteReview(string username)
        {
            var userReview = _repo.UserReview.FindAll().FirstOrDefault(ur => ur.Username == username);
            if (userReview != null)
            {
                try
                {
                    _repo.UserReview.Delete(userReview);

                    _repo.SaveChanges();
                    TempData["Message"] = $"{username}'s review has been deleted successfully!";
                }
                catch (Exception e)
                {
                    TempData["Message"] = $"Something went wrong!\nMessage: {e.ToString()}";
                }
            }
            return RedirectToAction("ListReviews");
        }
    }
}
