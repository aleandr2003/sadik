using Sadik.Services.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Sadik.Controllers
{
    public class KindergartenDependentController : SadikController
    {
        private int kindergartenId;
        public int KindergartenId 
        {
            get { return kindergartenId; }
            set {
                kindergartenId = value;
                ViewBag.KindergartenId = value;
            } 
        }
        public KindergartenDependentController(IUserSession userSession)
            :base(userSession)
        {
        }

    }
}
