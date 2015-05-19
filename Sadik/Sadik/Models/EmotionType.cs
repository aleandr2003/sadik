using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Sadik.Models
{
    public enum EmotionType:byte
    {
        [Display(Name = "Спокойный")]
        Neutral = 0,

        [Display(Name = "Грустный")]
        Sad = 10,

        [Display(Name = "Плачет")]
        Crying,

        [Display(Name = "Истерика")]
        Histerical,

        [Display(Name = "Сердитый")]
        Grumpy = 20,

        [Display(Name = "Агрессивный")]
        Agressive,

        [Display(Name = "Дерется")]
        Fighting,

        [Display(Name = "Улыбается")]
        Smiley = 30,

        [Display(Name = "Радостный")]
        Happy,

        [Display(Name = "Веселый")]
        Cheery
    }
}