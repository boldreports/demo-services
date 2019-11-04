using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Routing;
using System.Net;
using System.Text;
using System.IO;
using Bold.Licensing;

namespace ReportServices
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        public static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        protected void Application_Start()
        {
            // Register Bold Reports license
            string License = File.ReadAllText(Server.MapPath("BoldLicense.txt"), Encoding.UTF8);
            BoldLicenseProvider.RegisterLicense(License);

            //Establish a TLS connection for image downloading.
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            System.Web.Http.GlobalConfiguration.Configuration.EnableCors();
            System.Web.Http.GlobalConfiguration.Configuration.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{action}/{id}",
                defaults: new { id = RouteParameter.Optional });
        }
    }
}
