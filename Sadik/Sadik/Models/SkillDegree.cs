using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Sadik.Models
{
    public enum SkillDegree:byte
    {
        [Display(Name = "не пробовал")]
        None,
        [Display(Name = "пробовал")]
        Basic,
        [Display(Name = "работает")]
        Intermediate,
        [Display(Name = "умеет")]
        UpperIntermediate,
        [Display(Name = "умммееееет")]
        Advanced
    }
}