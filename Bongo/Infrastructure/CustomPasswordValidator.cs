using Bongo.Models;
using Microsoft.AspNetCore.Identity;
using System.Text.RegularExpressions;

namespace Bongo.Infrastructure
{
    public class CustomPasswordValidator : IPasswordValidator<BongoUser>
    {
        public Task<IdentityResult> ValidateAsync(UserManager<BongoUser> manager,
            BongoUser user, string password)
        {
            List<IdentityError> errors = new();

            Regex nonNumerics = new Regex(@"!|@|#|\$|%|\^|&|\*|\(|\)|~|\.|\?");

            if (!nonNumerics.IsMatch(password.ToLower()))
            {
                errors.Add(new IdentityError
                {
                    Code = "PasswordWithoutNonAlphanumeric",
                    Description = "Password must contain at least one of the following characters: !@#$%^&*()~.?"
                });
            }

            return Task.FromResult(errors.Count == 0 ?
              IdentityResult.Success : IdentityResult.Failed(errors.ToArray()));
        }
    }
}