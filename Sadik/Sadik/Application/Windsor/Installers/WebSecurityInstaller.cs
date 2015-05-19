using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Sadik.Services;
using Sadik.Services.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sadik.Application.Windsor.Installers
{
    public class WebSecurityInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(
                Component.For<IUserSession>().ImplementedBy<UserSession>().LifeStyle.Transient);
            container.Register(
                Component.For<INotificationSender>().ImplementedBy<NotificationSender>().LifeStyle.Transient);
            container.Register(
                Component.For<IAuthorizationService>().ImplementedBy<AuthorizationService>().LifeStyle.Transient);
        }
    }
}