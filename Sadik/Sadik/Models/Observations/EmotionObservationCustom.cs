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
                //отнимаем 2 часа потому что сервер в Москве, а все клиенты в Ебурге. 
                //TODO придумать правильное решение ASAP!
                return (long)(DateObserved.AddHours(-2).ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
            }
        }
    }
}