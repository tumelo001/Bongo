using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bongo.Models
{
    public class Timetable
    {
        [Key]
        public int TimetableID { get; set; }

        public string TimetableText { get; set; }

        public string Username { get; set; }
    }
}
