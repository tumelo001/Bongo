using System.ComponentModel.DataAnnotations;

namespace Bongo.Models.User
{
    public class UserReview
    {
        [Key]
        public int ReviewId { get; set; }
        public string Username { get; set; }
        public string Review { get; set; }
        public int Rating { get; set; }
        public DateTime ReviewDate { get; set; }
        public string Email { get; set; }   
    }
}