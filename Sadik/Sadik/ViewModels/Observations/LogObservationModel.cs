using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Sadik.ViewModels.Observations
{
    public abstract class LogObservationModel
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "Пожалуйста, выберите ребенка")]
        public int? KidId { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? DateObserved { get; set; }

        public int? Hours { get; set; }

        public int? Minutes { get; set; }

        public string Comment { get; set; }

        [Required(ErrorMessage = "Уникальный идентификатор наблюдения отсутствует.")]
        public Guid UniqueId { get; set; }

        public virtual DateTime GetObservationDate()
        {
            DateTime result = DateTime.Now.Date;
            if (DateObserved.HasValue && Hours.HasValue && Minutes.HasValue)
            {
                result = DateObserved.Value;
                result = result.AddHours(Hours.Value);
                result = result.AddMinutes(Minutes.Value);
            }
            else if (DateObserved.HasValue)
            {
                if (!Hours.HasValue) throw new ArgumentException("Не указано время. Поле час");
                if (!Minutes.HasValue) throw new ArgumentException("Не указано время. Поле минуты");
            }
            else if (Hours.HasValue && Minutes.HasValue)
            {
                result = DateTime.Now.Date;
                result = result.AddHours(Hours.Value);
                result = result.AddMinutes(Minutes.Value);
            }
            else
            {
                result = DateTime.Now;
            }
            return result;
        }
    }
}