using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.Http.Controllers;
using System.Net.Http;
using System.Web.Mvc;
using System.Web.Routing;
using AOPDemo.Common;

namespace AOPDemo
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            // 注册新的Controller工厂
            ControllerBuilder.Current.SetControllerFactory(new MyBatisControllerFactory());

            // 使AOP适应 WebApi
            GlobalConfiguration.Configuration.Services.Replace(typeof(IHttpControllerActivator), new MyHttpControllerActivator());

            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }



        private class MyBatisControllerFactory : DefaultControllerFactory
        {
            public override IController CreateController(RequestContext requestContext, string controllerName)
            {
                IController controller = base.CreateController(requestContext, controllerName);

                /// 自动装配属性
                /// <para>为属性对象启用代理，并延迟初始化被代理的对象</para>
                DelayProxyUtil.AutowiredProperties(controller);

                return controller;
            }
        }



        /// <summary>
        /// 用于Web Api
        /// </summary>
        private class MyHttpControllerActivator : IHttpControllerActivator
        {
            private DefaultHttpControllerActivator defaultActivator;

            public MyHttpControllerActivator()
            {
                this.defaultActivator = new DefaultHttpControllerActivator();
            }

            public IHttpController Create(HttpRequestMessage request, HttpControllerDescriptor controllerDescriptor, Type controllerType)
            {
                IHttpController httpController = this.defaultActivator.Create(request, controllerDescriptor, controllerType);

                if (httpController != null)
                {
                    /// 自动装配属性
                    /// <para>为属性对象启用代理，并延迟初始化被代理的对象</para>
                    DelayProxyUtil.AutowiredProperties(httpController);
                }

                return httpController;
            }
        }


    }



}