using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Sadik.ViewModels
{
    public class ADRModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Пожалуйста, укажите дату ДТР")]
        [DataType(DataType.Date)]
        public DateTime? Date { get; set; }

        [Required(ErrorMessage = "Пожалуйста, выберите ребенка")]
        public int KidId { get; set; }
        public string AchievementsTeachers { get; set; }
        public string AchievementsParents { get; set; }
        public string DifficultiesTeachers { get; set; }
        public string DifficultiesParents { get; set; }
        public string RecommendationsTeachers { get; set; }
        public string RecommendationsParents { get; set; }
        public bool Performed { get; set; }

        public DateTime? StartPeriod { get; set; }
        public DateTime? EndPeriod { get; set; }
    }
}