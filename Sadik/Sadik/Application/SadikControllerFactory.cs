using Castle.Windsor;
using Sadik.Application.Windsor;
using Sadik.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Sadik.Application
{
    public class SadikControllerFactory : WindsorControllerFactory
    {
        public SadikControllerFactory(IWindsorContainer container)
            :base(container)
        {
        }

        protected override IController GetControllerInstance(RequestContext requestContext, Type controllerType)
        {
            var controller = base.GetControllerInstance(requestContext, controllerType);
            if (controller is KindergartenDependentController)
            {
                if (requestContext.RouteData.Values.Keys.Contains("KindergartenId"))
                {
                    if (requestContext.RouteData.Values["KindergartenId"] is string)
                    {
                        ((KindergartenDependentController)controller).KindergartenId = int.Parse((string)requestContext.RouteData.Values["KindergartenId"]);
                    }
                    else if (requestContext.RouteData.Values["KindergartenId"] is int)
                    {
                        ((KindergartenDependentController)controller).KindergartenId = (int)requestContext.RouteData.Values["KindergartenId"];
                    }
                    else
                    {
                        throw new Exception("KindergartenId malformated");
                    }
                }
                else
                {
                    throw new Exception("KindergartenId is missing");
                }
            }
            return controller;
        }
    }
}