using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Castle.Windsor.Configuration.Interpreters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Castle.Windsor.Installer;
using Sadik.Application.Validation;
using Sadik.Application.Windsor;
using Sadik.Application;
using System.Web.Optimization;

namespace Sadik
{
    // Примечание: Инструкции по включению классического режима IIS6 или IIS7 
    // см. по ссылке http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        static IWindsorContainer container;
        public IWindsorContainer Container
		{
			get { return container; }
		}

        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new Sadik.Application.HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("sadiksearch.html");
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.MapRoute(
                "KindergartenDependentWithId", // Имя маршрута
                "{controller}/{action}/{KindergartenId}/{id}", // URL-адрес с параметрами
                null,
                constraints: new { controller = @"(Observation|ADR|Inventory|Kids)" }
            );
            routes.MapRoute(
                "KindergartenDependent", // Имя маршрута
                "{controller}/{action}/{KindergartenId}", // URL-адрес с параметрами
                null,
                constraints: new { controller = @"(Observation|ADR|Inventory|Kids)" }
            );
            routes.MapRoute(
                "Default", // Имя маршрута
                "{controller}/{action}/{id}", // URL-адрес с параметрами
                new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Параметры по умолчанию
            );

        }

        protected void Application_Start()
        {

            InitContainer();
            DataAnnotationsModelValidatorProvider.RegisterAdapter(typeof(DataTypeAttribute), typeof(DataTypeAttributeAdapter));
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);

			RegisterRoutesWithStandardIgnores(RouteTable.Routes);

            ConfigureOptionalServices();
            BundleTable.Bundles.Add(CreateCssBundle());
            BundleTable.Bundles.Add(CreateBasicJsBundle());
            BundleTable.Bundles.Add(CreateJsObservationBundle());
            BundleTable.Bundles.Add(CreateJsJQPlotBundle());
        }

        private Bundle CreateBasicJsBundle()
        {
            Bundle bundle = new Bundle("~/Scripts/js", new JsMinify());

            bundle.AddFile("~/Scripts/jquery-ui-1.11.2.custom/jquery.js");
            bundle.AddFile("~/Scripts/jquery-ui-1.11.2.custom/jquery-ui.min.js");
            bundle.AddFile("~/Scripts/jquery.ui.datepicker-ru.js");
            bundle.AddFile("~/Scripts/jquery.validate.min.js");
            bundle.AddFile("~/Scripts/jquery.validate.unobtrusive.min.js");
            bundle.AddFile("~/Scripts/jquery.unobtrusive-ajax.min.js");
            bundle.AddFile("~/Scripts/Sadik/Misc.js");
            bundle.AddFile("~/Scripts/Sadik/Collections.js");
            bundle.AddFile("~/Scripts/Sadik/Collapsible.js");
            bundle.AddFile("~/Scripts/Sadik/klass.js");
            bundle.AddFile("~/Scripts/Sadik/pubsub.js");

            return bundle;
        }

        private Bundle CreateJsObservationBundle()
        {
            Bundle bundle = new Bundle("~/Scripts/js_observ", new JsMinify());

            bundle.AddFile("~/Scripts/orm/model.js");
            bundle.AddFile("~/Scripts/orm/observation.js");
            bundle.AddFile("~/Scripts/orm/cameInClass.js");
            bundle.AddFile("~/Scripts/orm/emotion.js");
            bundle.AddFile("~/Scripts/orm/activity.js");
            bundle.AddFile("~/Scripts/orm/itemUsageDetails.js");
            bundle.AddFile("~/Scripts/ObservationLogger/BaseObservationLogger.js");
            bundle.AddFile("~/Scripts/ObservationLogger/ActivityLogger.js");
            bundle.AddFile("~/Scripts/ObservationLogger/CameInClassLogger.js");
            bundle.AddFile("~/Scripts/ObservationLogger/EmotionLogger.js");
            bundle.AddFile("~/Scripts/ObservationLogger/ObservationLoggerModule.js");
            return bundle;
        }

        private Bundle CreateJsJQPlotBundle()
        {
            Bundle bundle = new Bundle("~/Scripts/js_jqPlot", new JsMinify());

            bundle.AddFile("~/Scripts/jqPlot/excanvas.min.js");
            bundle.AddFile("~/Scripts/jqPlot/jquery.jqplot.min.js");
            bundle.AddFile("~/Scripts/jqPlot/jqplot.dateAxisRenderer.min.js");
            return bundle;
        }

        private Bundle CreateCssBundle()
        {
            Bundle bundle = new Bundle("~/Content/css");

            bundle.AddFile("~/Content/Site.css");
            bundle.AddFile("~/Content/Modifiers.css");
            bundle.AddFile("~/Content/jquery-ui.min.css");

            return bundle;
        }

        private void ConfigureOptionalServices()
        {
            var controllerFactory = Container.ResolveOrDefault<IControllerFactory>();
            if (controllerFactory != null)
                ControllerBuilder.Current.SetControllerFactory(controllerFactory);

            var binder = Container.ResolveOrDefault<IModelBinder>();
            if (binder != null)
                ModelBinders.Binders.DefaultBinder = binder;

            var filterProvider = Container.ResolveOrDefault<IFilterProvider>();
            if (filterProvider != null)
            {
                var oldProvider = FilterProviders.Providers.Single(f => f is FilterAttributeFilterProvider);
                FilterProviders.Providers.Remove(oldProvider);
                FilterProviders.Providers.Add(filterProvider);
            }
        }

        void RegisterRoutesWithStandardIgnores(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.IgnoreRoute("Content/{*pathInfo}");
            routes.IgnoreRoute("favicon.ico");

            RegisterRoutes(routes);
        }

        private void InitContainer()
        {
            container = new WindsorContainer(new XmlInterpreter());
            container.Register(Component.For<IWindsorContainer>().Instance(container));
            container.Install(FromAssembly.This());
        }

    }
}