using System.ComponentModel.DataAnnotations;

namespace Bongo.Models.ViewModels
{
    public class AnswerSecurityQuestionViewModel
    {
        public string Username { get; set; }
        public string SecurityQuestion { get; set; }
        public string SecurityAnswer { get; set; }
    }
}
