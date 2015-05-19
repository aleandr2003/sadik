using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Sadik.ViewModels
{
    public class EditProfileModel
    {
        [Required(ErrorMessage = "Пожалуйста, введите свое имя")]
        [StringLength(150, MinimumLength = 3, ErrorMessage = "Имя должно быть не короче 3 и не длиннее 150 символов")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Пожалуйста, введите фамилию")]
        [StringLength(150, MinimumLength = 3, ErrorMessage = "Фамилия должно быть не короче 3 и не длиннее 150 символов")]
        public string LastName { get; set; }

        //[Required(ErrorMessage = "Пожалуйста, введите e-mail")]
        //[DataType(DataType.EmailAddress, ErrorMessage = "Введен некорректный e-mail")]
        //public string Email { get; set; }
    }
}