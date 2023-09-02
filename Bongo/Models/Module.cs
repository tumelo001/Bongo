using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bongo.Models
{
    public class Module
    {
        [Key]
        public string ModuleCode { get; set; }
        public string ModuleName { get; set; }
        public int Credits { get { return int.Parse(ModuleCode.Substring(ModuleCode.Length - 1, 1)) * 4; } }
        public double ModuleHoursPerWeek  //v2
        {
            get
            {
                double TotalNotionalHrs = Credits * 10;
                double TotalNoWeeks = int.Parse(ModuleCode.Substring(6, 1)) != 0 ? 14 : 26;
                return Math.Round(TotalNotionalHrs / TotalNotionalHrs, 1);
            }
        }

    }
}
