using BoldReports.Web.ReportViewer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Cors;

namespace ReportServices.Controllers.docs
{
    [EnableCors("AllowAllOrigins")]
    [Route("api/[controller]/[action]")]
    public class CustomAjaxHeadersController : Controller, IReportController
    {
        private Microsoft.Extensions.Caching.Memory.IMemoryCache _cache;
        private IWebHostEnvironment _hostingEnvironment;
        string basePath;

        public CustomAjaxHeadersController(Microsoft.Extensions.Caching.Memory.IMemoryCache memoryCache,
           IWebHostEnvironment hostingEnvironment)
        {
            _cache = memoryCache;
            _hostingEnvironment = hostingEnvironment;
            basePath = _hostingEnvironment.WebRootPath;
        }

        //Post action for processing the rdl/rdlc report 
        public string DefaultParameter = null;
        string authenticationHeader;

        [HttpPost]
        public object PostReportAction(Dictionary<string, object> jsonResult)
        {
            if (jsonResult != null)
            {
                if (jsonResult.ContainsKey("CustomData"))
                {
                    //Get client side custom data and store in local variable. Here parameter values are sent.
                    DefaultParameter = jsonResult["CustomData"].ToString();
                }

                //Get client side custom ajax header and store in local variable
                authenticationHeader = HttpContext.Request.Headers["Authorization"];

                //Perform your custom validation here
                if (authenticationHeader == "")
                {
                    return new Exception("Authentication failed!!!");
                }
                else
                {
                    return ReportHelper.ProcessReport(jsonResult, this, this._cache);
                }
            }

            return null;
        }

        //Get action for getting resources from the report
        [ActionName("GetResource")]
        [AcceptVerbs("GET")]
        public object GetResource(ReportResource resource)
        {
            return ReportHelper.GetResource(resource, this, this._cache);
        }

        [HttpPost]
        public object PostFormReportAction()
        {
            return ReportHelper.ProcessReport(null, this, _cache);
        }


        //Method will be called when initialize the report options before start processing the report        
        public void OnInitReportOptions(ReportViewerOptions reportOption)
        {
            string resourcesPath = this.basePath;

            reportOption.ReportModel.ExportResources.Scripts = new List<string>
            {
                resourcesPath + "/scripts/bold-reports/common/bold.reports.common.min.js",
                resourcesPath + "/scripts/bold-reports/common/bold.reports.widgets.min.js",
                // Chart component script
                resourcesPath + "/scripts/bold-reports/data-visualization/ej.chart.min.js",
                // Report Viewer Script
                resourcesPath + "/scripts/bold-reports/bold.report-viewer.min.js"
            };

            reportOption.ReportModel.ExportResources.DependentScripts = new List<string>
            {
                resourcesPath + "/scripts/dependent/jquery.min.js"
            };
        }

        //Method will be called when reported is loaded
        public void OnReportLoaded(ReportViewerOptions reportOption)
        {
            if (DefaultParameter != null)
            {
                //Set client side custom header data 
                reportOption.ReportModel.Parameters = JsonConvert.DeserializeObject<List<BoldReports.Web.ReportParameter>>(DefaultParameter);
            }
        }
    }
}