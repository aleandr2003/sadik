using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Sadik.ViewModels
{
    public class EditKindergartenModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Пожалуйста, введите название садика")]
        [StringLength(150, MinimumLength = 3, ErrorMessage = "Название должно быть не короче 3 и не длиннее 150 символов")]
        public string Name { get; set; }
    }
}