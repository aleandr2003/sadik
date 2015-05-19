using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Sadik.Models
{
    public enum ActivityDuration:byte
    {
        [Display(Name = "Быстро")]
        Brief,

        [Display(Name = "Недолго")]
        Short,

        [Display(Name = "Долго")]
        Long
    }
}