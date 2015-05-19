using Sadik.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sadik.ViewModels.Observations
{
    public class ObservationCameInClassModel: ObservationModel
    {
        public CameInClass observation;
        public ObservationCameInClassModel(CameInClass cameIn)
        {
            observation = cameIn;
            KidId = observation.KidId;
            DateObserved = observation.DateTimeCameInClass;
        }
    }
}