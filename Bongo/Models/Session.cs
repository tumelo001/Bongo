using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bongo.Models
{
    public class Session
    {
        [Key]
        public string Username { get; set; }
        public string StudentNumber { get; set; }
        [NotMapped]
        public int[] Period { get; set; } = new int[2];
        public string sessionType { get; set; }
        public string ModuleCode { get; set; }
        public string Venue { get; set; }
        public string sessionInPDFValue { get; set; }
        public string Description
        {
            get { return sessionType.ToString() + "\n" + ModuleCode + "\n" + Venue; }
        }
    }
}