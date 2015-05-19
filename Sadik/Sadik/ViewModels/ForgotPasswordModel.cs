using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Sadik.ViewModels
{
    public class ForgotPasswordModel
    {
        [Required(ErrorMessage = "Пожалуйста, введите e-mail")]
        [DataType(DataType.EmailAddress, ErrorMessage = "Введен некорректный e-mail")]
        public string Email { get; set; }
    }
}