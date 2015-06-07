using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sadik.Models
{
    public partial class CameInClass
    {
        public long DateObservedMilliseconds
        {
            get
            {
                return (long)(DateTimeCameInClass.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
            }
        }
    }
}