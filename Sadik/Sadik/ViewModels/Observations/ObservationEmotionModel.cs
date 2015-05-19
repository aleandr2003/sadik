using Sadik.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sadik.ViewModels.Observations
{
    public class ObservationEmotionModel: ObservationModel
    {
        public EmotionObservation observation;
        public ObservationEmotionModel(EmotionObservation observation)
        {
            this.observation = observation;
            KidId = observation.KidId;
            DateObserved = observation.DateObserved;
        }
    }
}