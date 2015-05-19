using Sadik.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Sadik.ViewModels.Observations
{
    public class LogEmotionModel : LogObservationModel
    {
        public EmotionType Emotion { get; set; }
    }
}