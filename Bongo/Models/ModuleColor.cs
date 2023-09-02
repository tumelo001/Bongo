using System.ComponentModel.DataAnnotations;

namespace Bongo.Models
{
    public class ModuleColor
    {
        [Key]
        public int ModuleColorId { get; set; }
        public string ModuleCode { get; set; }
        public string Username { get; set; }

        public int ColorId { get; set; }

        public Color Color { get; set; }
    }
}
