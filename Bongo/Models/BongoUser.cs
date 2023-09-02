using Microsoft.AspNetCore.Identity;

namespace Bongo.Models
{
    public class BongoUser : IdentityUser
    {
        public string SecurityQuestion { get; set; }
        public string SecurityAnswer { get; set; }
        public bool Notified { get; set; }
    }
}
