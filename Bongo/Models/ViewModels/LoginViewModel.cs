using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Bongo.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Required")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Required")]
        [UIHint("password")]
        public string Password { get; set; }

        public string ReturnUrl { get; set; } = "/";

        [DisplayName("Remember me")]
        public bool RememberMe { get; set; }

    }
    public class RegisterModel
    {
        [Required(ErrorMessage = "Required")]
        [Display(Name = "Username")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "Email")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Required")]
        [DataType(DataType.Password)] //Indicate that password options specified in Program.cs must be used.
        public string Password { get; set; }

        [Compare("Password", ErrorMessage = "Passwords must match")]
        [Display(Name = "Confirm Password")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }
    }

    public class ForgotPassword
    {
        [Required(ErrorMessage = "Required")]
        [Display(Name = "Username")]
        public string Username { get; set; }
    }

    public class ResetPassword
    {
        public string UserId { get; set; }
        public string Token { get; set; }

        [Required(ErrorMessage = "Required")]
        [Display(Name = "New Password")]
        [DataType(DataType.Password)] //Indicate that password options specified in Program.cs must be used.
        public string Password { get; set; }

        [Compare("Password", ErrorMessage = "Passwords must match")]
        [Display(Name = "Confirm Password")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }
    }

    public class ConfirmEmail
    {
        public string UserId { get; set; }
        public string Token { get; set; }
    }

}