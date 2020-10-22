﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Routing;
using System.Net;
using System.Text;
using System.IO;
using Bold.Licensing;
using BoldReports.Base.Logger;
using BoldReports.Web;
using Newtonsoft.Json;
using System.Reflection;

namespace ReportServices
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            // Register Bold Reports license
            string License = File.ReadAllText(Server.MapPath("BoldLicense.txt"), Encoding.UTF8);
            log4net.GlobalContext.Properties["LogPath"] = this.GetAppDataFolderPath();
            BoldReports.Base.Logger.LogExtension.RegisterLog4NetConfig();
            BoldLicenseProvider.RegisterLicense(License);

            ReportConfig.DefaultSettings = new ReportSettings()
            {
                MapSetting = this.GetMapSettings()
            }.RegisterExtensions(this.GetDataExtension());


            //Establish a TLS connection for image downloading.
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            System.Web.Http.GlobalConfiguration.Configuration.EnableCors();
            System.Web.Http.GlobalConfiguration.Configuration.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{action}/{id}",
                defaults: new { id = RouteParameter.Optional });
        }
        public string GetAppDataFolderPath()
        {
            try
            {
                return System.IO.Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory);
            }
            catch
            {
                return null;
            }
        }
        private BoldReports.Web.MapSetting GetMapSettings()
        {
            try
            {
                string basePath = HttpContext.Current.Server.MapPath("~/Scripts");
                return new MapSetting()
                {
                    ShapePath = basePath + "\\ShapeData\\",
                    MapShapes = JsonConvert.DeserializeObject<List<MapShape>>(System.IO.File.ReadAllText(basePath + "\\ShapeData\\mapshapes.txt"))
                };
            }
            catch (Exception ex)
            {
                LogExtension.LogError("Failed to Load Map Settings", ex, MethodBase.GetCurrentMethod());
            }
            return null;
        }

        private List<string> GetDataExtension()
        {
            var extensions = !string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["ExtAssemblies"]) ? System.Configuration.ConfigurationManager.AppSettings["ExtAssemblies"] : string.Empty;
            try
            {
                return new List<string>(extensions.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries));
            }
            catch (Exception ex)
            {
                LogExtension.LogError("Failed to Load Data Extension", ex, MethodBase.GetCurrentMethod());
            }
            return null;
        }

    }
}
