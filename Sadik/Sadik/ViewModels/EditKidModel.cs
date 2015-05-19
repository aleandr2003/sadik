using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Sadik.ViewModels
{
    public class EditKidModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Пожалуйста, введите имя ребенка")]
        [StringLength(150, MinimumLength = 3, ErrorMessage = "Имя должно быть не короче 3 и не длиннее 150 символов")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Пожалуйста, введите фамилию")]
        [StringLength(150, MinimumLength = 3, ErrorMessage = "Фамилия должна быть не короче 3 и не длиннее 150 символов")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Пожалуйста, введите отчество")]
        [StringLength(150, MinimumLength = 3, ErrorMessage = "Отчество должно быть не короче 3 и не длиннее 150 символов")]
        public string Patronymic { get; set; }

        [Required(ErrorMessage = "Пожалуйста, введите дату рождения")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "Пожалуйста, укажите, когда ребенка приняли в садик")]
        [DataType(DataType.Date)]
        public DateTime DateAccepted { get; set; }
    }
}