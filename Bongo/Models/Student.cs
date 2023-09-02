using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bongo.Models
{
    public class Student //not in use 
    {
        [Key]
        public string StudentNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        
    }
}
