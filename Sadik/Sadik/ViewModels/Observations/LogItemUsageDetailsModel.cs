using Sadik.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Sadik.ViewModels.Observations
{
    public class LogItemUsageDetailsModel
    {
        [Required(ErrorMessage = "Пожалуйста, выберите ребенка")]
        public int KidId { get; set; }

        [Required(ErrorMessage = "Пожалуйста, выберите материал")]
        public int ItemId { get; set; }

        public bool presPerformed { get; set; }

        public DateTime? datePerformed { get; set; }

        public SkillDegree skillDegree { get; set; }

        public Guid UniqueId { get; set; }
    }
}