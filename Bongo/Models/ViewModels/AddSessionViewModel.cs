using Bongo.Infrastructure;
using System.ComponentModel.DataAnnotations;

namespace Bongo.Models.ViewModels
{
    public class AddSessionViewModel
    {
        [Required(ErrorMessage = "Please enter the module code.")]
        [RegularExpression(@"\w{4}\d{4}|CLASH!![\d]", ErrorMessage = "The module code must have 4 letters and 4 digits, no spaces.")]
        [Display(Name = "Module code")]
        public string ModuleCode { get; set; }

        [Required(ErrorMessage = "Please select the session type.")]
        [Display(Name = "Session type")]
        public string SessionType { get; set; }

        [Required(ErrorMessage = "Please select the session number")]
        [Range(1, int.MaxValue, ErrorMessage ="Please enter a valid session number.")]
        public int SessionNumber { get; set; }

        [Required(ErrorMessage = "Please enter the venue.")]
        public string Venue { get; set; }
        public string Day { get; set; }

        [Required(ErrorMessage = "Please select the start time.")]
        [Display(Name = "Start time")]
        public string startTime { get; set; }

        [Required(ErrorMessage = "Please select the end time.")]
        [Display(Name = "End time")]
        public string endTime { get; set; }

    }
}
