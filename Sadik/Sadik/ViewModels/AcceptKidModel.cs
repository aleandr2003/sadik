using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Sadik.ViewModels
{
    public class AcceptKidModel
    {
        [Required(ErrorMessage = "Пожалуйста, введите имя ребенка")]
        [StringLength(150, MinimumLength = 3, ErrorMessage = "Имя должно быть не короче 3 и не длиннее 150 символов")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Пожалуйста, введите фамилию")]
        [StringLength(150, MinimumLength = 3, ErrorMessage = "Фамилия должна быть не короче 3 и не длиннее 150 символов")]
        public string LastName { get; set; }

        [StringLength(150, ErrorMessage = "Отчество должно быть не длиннее 150 символов")]
        public string Patronymic { get; set; }

        [Required(ErrorMessage = "Пожалуйста, введите дату рождения")]
        [DataType(DataType.DateTime)]
        public DateTime? DateOfBirth { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? DateAccepted { get; set; }
    }
}