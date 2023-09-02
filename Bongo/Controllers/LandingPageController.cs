using Bongo.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bongo.Controllers
{
    [AllowAnonymous]
    public class LandingPageController : Controller
    {
        private readonly IRepositoryWrapper _repo;

        public LandingPageController(IRepositoryWrapper repo)
        {
            _repo = repo;
        }
        public IActionResult Index()
        {
            return View(_repo.UserReview.FindAll().ToList());
        }
    }
}