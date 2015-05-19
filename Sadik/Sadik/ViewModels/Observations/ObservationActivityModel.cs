using Sadik.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sadik.ViewModels.Observations
{
    public class ObservationActivityModel : ObservationModel
    {
        public Activity observation;
        public ObservationActivityModel(Activity activity)
        {
            observation = activity;
            KidId = observation.KidId;
            DateObserved = observation.DateObserved;
        }
    }
}