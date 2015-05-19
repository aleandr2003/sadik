using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

namespace Sadik.Application.Windsor.Installers
{
    public class ControllersInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container
                .RegisterControllers(Assembly.GetExecutingAssembly())
                .Register(
                //    Component.For<IControllerFactory>().ImplementedBy<WindsorControllerFactory>()
                    Component.For<IControllerFactory>().ImplementedBy<SadikControllerFactory>()
                /*,Component.For<IFilterProvider>().ImplementedBy<WindsorFilterAttributeFilterProvider>()*/);
        }
    }
}