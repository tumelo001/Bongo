using System.ComponentModel.DataAnnotations;

namespace Bongo.Models.ViewModels
{
    public class SecurityQuestionViewModel
    {
        [Display(Name ="Question")]
        public string SecurityQuestion { get; set; }

        [Display(Name = "Answer")]
        public string SecurityAnswer { get; set; }

        public string UserName { get; set; }

        public string SendingAction { get; set; }
    }
}
