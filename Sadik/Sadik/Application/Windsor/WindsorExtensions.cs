using System;
using System.Reflection;
using System.Web.Mvc;
using Castle.Core;
using Castle.MicroKernel;
using Castle.MicroKernel.ComponentActivator;
using Castle.Windsor;
using System.Linq;

namespace Sadik.Application.Windsor
{
    public static class WindsorExtensions
    {
        public static IWindsorContainer RegisterController<T>(this IWindsorContainer container) where T : IController
        {
            container.RegisterControllers(typeof(T));
            return container;
        }

        public static IWindsorContainer RegisterControllers(this IWindsorContainer container, params Type[] controllerTypes)
        {
            foreach (var type in controllerTypes)
            {
                if (type.Name.EndsWith("Controller") && !type.IsAbstract)
                {
                    container.AddComponentLifeStyle(type.FullName.ToLower(), type, LifestyleType.Transient);
                }
            }

            return container;
        }

        public static IWindsorContainer RegisterControllers(this IWindsorContainer container, params Assembly[] assemblies)
        {
            foreach (var assembly in assemblies)
            {
                container.RegisterControllers(assembly.GetExportedTypes());
            }
            return container;
        }

        public static T ResolveOrDefault<T>(this IWindsorContainer container)
        {
            return container.ResolveAll<T>().FirstOrDefault();
        }


        public static void InjectProperties(this IWindsorContainer container, object target, bool overwrite = false)
        {
            var kernel = container.Kernel;
            var type = target.GetType();
            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (property.CanWrite && (overwrite || property.GetValue(target, null) == null) && kernel.HasComponent(property.PropertyType))
                {
                    var value = kernel.Resolve(property.PropertyType);
                    try
                    {
                        property.SetValue(target, value, null);
                    }
                    catch (Exception ex)
                    {
                        var message = string.Format("Error setting property {0} on type {1}, See inner exception for more information.", property.Name, type.FullName);
                        throw new ComponentActivatorException(message, ex);
                    }
                }
            }
        }
    }
}