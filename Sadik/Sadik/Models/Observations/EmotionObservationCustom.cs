using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sadik.Models
{
    public partial class EmotionObservation
    {
        public long DateObservedMilliseconds{
            get
            {
                return (long)(DateObserved - new DateTime(1970, 1, 1)).TotalMilliseconds;
            }
        }
    }
}