using Bongo.Models;
using Bongo.Models.ViewModels;
using Bongo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Bongo.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly UserManager<BongoUser> _userManager;
        private readonly SignInManager<BongoUser> _signInManager;
        private readonly IMailService _mailSender;
        private readonly IConfiguration _config;

        public AccountController(UserManager<BongoUser> userManager, SignInManager<BongoUser> signInManager,
            IMailService mailSender, IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _mailSender = mailSender;
            _config = configuration;
        }
        [TempData]
        public string Message { get; set; }




        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }
        [AllowAnonymous]
        public IActionResult SignIn(string returnUrl)
        {
            return View(new LoginViewModel
            { ReturnUrl = returnUrl }
                        );
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel loginModel)
        {
            if (ModelState.IsValid)
            {
                BongoUser user =
                await _userManager.FindByNameAsync(loginModel.Username.Trim());  //check if the user exists
                if (user != null)
                {

                    var result = await _signInManager.PasswordSignInAsync(user,
                       loginModel.Password, isPersistent: loginModel.RememberMe, false);
                    if (result.Succeeded)
                    {
                        Response.Cookies.Append("Notified", user.Notified.ToString().ToLower(),
                            new CookieOptions { Expires = DateTime.Now.AddDays(90) }
                            );
                        if (user.SecurityQuestion != default)
                            return RedirectToAction("TimeTableFileUpload", "Session");
                        else
                            return RedirectToAction("SecurityQuestion", new { username = user.UserName, sendingAction = "LogIn" });
                    }
                }
                ModelState.AddModelError("", "Invalid username or password");

            }
            return View("SignIn", loginModel);
        }
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View(new RegisterModel());
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegisterModel registerModel)
        {
            if (ModelState.IsValid)
            {
                if (await _userManager.FindByNameAsync(registerModel.UserName.Trim()) != null)
                {
                    ModelState.AddModelError("", "Username already exists💀");
                    return View();
                }
                var user = new BongoUser
                {
                    UserName = registerModel.UserName.Trim(),
                    Email = registerModel.Email
                };

                var result = await _userManager.CreateAsync(user, registerModel.Password);

                if (result.Succeeded)
                {
                    try
                    {
                        //var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                        /* Dictionary<string, string> emailOptions = new Dictionary<string, string>
                         { { "username", user.UserName},
                           { "link",_config.GetValue<string>("Application:AppDomain") + $"Account/ConfirmEmail?userId={user.Id}&token{token}" }
                         };
 */
                        //await _mailSender.SendMailAsync(registerModel.Email, "Welcome to Bongo", "WelcomeEmail", emailOptions);
                    }
                    catch (Exception)
                    {
                        ModelState.AddModelError("", "Something went wrong while registering your account. It's not you, it's us💀");
                        return View();
                    }
                    return RedirectToAction("SecurityQuestion", new { username = user.UserName, sendingAction = "Register" });
                }

                else
                {
                    foreach (var error in result.Errors)
                        ModelState.AddModelError("", error.Description);
                }

            }
            return View(registerModel);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ConfirmEmail(string userId, string token)
        {
            return View(new ConfirmEmail { UserId = userId, Token = token });
        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(ConfirmEmail model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(model.UserId);
                if (user != null)
                {
                    var result = await _userManager.ConfirmEmailAsync(user, model.Token);
                    if (result.Succeeded)
                    {
                        Message = "Successfully registered";
                    }
                    else
                    {

                    }
                    TempData["Message"] = "Email verified successfully";
                }
                TempData["Message"] = "Something went wrong😐.";

                return RedirectToAction("SignIn");
            }
            return View(model);

        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyUsername(string username)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByNameAsync(username);
                if (user != null)
                {
                    return View("AskSecurityQuestion", new AnswerSecurityQuestionViewModel { Username = user.UserName, SecurityQuestion = user.SecurityQuestion });
                }
                ModelState.AddModelError("", $"Invalid. Please enter an existing username");
            }
            return View("ForgotPassword", new ForgotPassword { Username = username });
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword(string username)
        {
            return View(new ForgotPassword { Username = username });
        }


        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword(AnswerSecurityQuestionViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByNameAsync(model.Username);
                if (user != null)
                {
                    if (user.SecurityAnswer.ToLower().Trim() == model.SecurityAnswer.ToLower().Trim())
                    {
                        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                        return RedirectToAction("ResetPassword", new { userId = user.Id, token = token });
                    }
                    ModelState.AddModelError("", $"Incorrect answer. Please try again.");
                    return View("AskSecurityQuestion", model);
                }
                ModelState.AddModelError("", $"Invalid. User with username {model.Username} does not exist");
                return View(new { username = model.Username });
            }
            ModelState.AddModelError("", $"Something went wrong with username {model.Username}. Please try again, if the problem persists contact us.");
            return View(new { username = model.Username });
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string userId, string token)
        {
            return View(new ResetPassword { Token = token, UserId = userId });
        }


        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(ResetPassword model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(model.UserId);
                if (user != null)
                {
                    var result = await _userManager.ResetPasswordAsync(user, model.Token, model.ConfirmPassword);
                    if (result.Succeeded)
                    {
                        TempData["SuccessMessage"] = "Successfully reset password";
                        return RedirectToAction("SignIn");
                    }
                    foreach (var error in result.Errors)
                        ModelState.AddModelError("", error.Description);
                }
            }
            return View(model);
        }
        [AllowAnonymous]
        public IActionResult SecurityQuestion(string username, string sendingAction)
        {
            return View(new SecurityQuestionViewModel { UserName = username, SendingAction = sendingAction });
        }
        [HttpPost]
        [AllowAnonymous]
        [ActionName("SecurityQuestion")]
        public async Task<IActionResult> UpdateSecurityQuestion(SecurityQuestionViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByNameAsync(model.UserName);
                user.SecurityQuestion = model.SecurityQuestion;
                user.SecurityAnswer = model.SecurityAnswer;
                await _userManager.UpdateAsync(user);
                bool fromRegister = model.SendingAction == "Register";
                if (fromRegister)
                    Message = "Successfully registered";
                return RedirectToAction(fromRegister ? "SignIn" : "TimeTableFileUpload", fromRegister ? "Account" : "Session");
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }

        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

    }
}
