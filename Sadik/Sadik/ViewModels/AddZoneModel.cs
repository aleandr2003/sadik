using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Sadik.ViewModels
{
    public class AddZoneModel
    {
        [Required(ErrorMessage = "Пожалуйста, введите название зоны")]
        [StringLength(150, MinimumLength = 2, ErrorMessage = "Имя должно быть не короче 2 и не длиннее 150 символов")]
        public string Name { get; set; }

        public int? parentZoneId { get; set; }
    }
}