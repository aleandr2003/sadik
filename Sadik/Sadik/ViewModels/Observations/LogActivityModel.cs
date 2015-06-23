using Sadik.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Sadik.ViewModels.Observations
{
    public class LogActivityModel : LogObservationModel
    {
        [Required(ErrorMessage = "Пожалуйста, выберите материал")]
        public int? ItemId { get; set; }

        public int? DurationMinutes { get; set; }

        public bool Polarization { get; set; }

        public bool ChoseHimSelf { get; set; }

        //public override DateTime GetObservationDate()
        //{
        //    DateTime result = DateTime.Now;
        //    if (DateObserved.HasValue)
        //    {
        //        result = DateObserved.Value;
        //        if (Hours.HasValue) result = result.AddHours(Hours.Value);
        //        if (Minutes.HasValue) result = result.AddMinutes(Minutes.Value);
        //    }
        //    return result;
        //}
    }
}