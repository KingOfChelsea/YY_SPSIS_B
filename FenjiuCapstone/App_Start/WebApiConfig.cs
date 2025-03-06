using Microsoft.IdentityModel.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Cors;

namespace FenjiuCapstone
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // 启用CORS，允许所有来源、所有头和所有方法
            var cors = new EnableCorsAttribute("*", "*", "*");
            config.EnableCors(cors);

            // Web API 配置和服务

            // Web API 路由
            config.MapHttpAttributeRoutes();
            IdentityModelEventSource.ShowPII = true;
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
