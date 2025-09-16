using BoldReports.Web.ReportViewer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Microsoft.AspNetCore.Cors;
using Microsoft.Extensions.Configuration;

namespace ReportServices.Controllers.docs
{
    [EnableCors("AllowAllOrigins")]
    [Route("api/[controller]/[action]")]
    public class SSRSReportsController : Controller, IReportController
    {
        private Microsoft.Extensions.Caching.Memory.IMemoryCache _cache;
        private IWebHostEnvironment _hostingEnvironment;
        private readonly IConfiguration _configuration; 
        string basePath;

        public SSRSReportsController(Microsoft.Extensions.Caching.Memory.IMemoryCache memoryCache,
           IWebHostEnvironment hostingEnvironment, IConfiguration configuration)
        {
            _cache = memoryCache;
            _hostingEnvironment = hostingEnvironment;
            basePath = _hostingEnvironment.WebRootPath;
            _configuration = configuration;
        }

        //Post action for processing the rdl/rdlc report 
        [HttpPost]
        public object PostReportAction(Dictionary<string, object> jsonResult)
        {
            return ReportHelper.ProcessReport(jsonResult, this, this._cache);
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
            //Add SSRS Report Server credential
            reportOption.ReportModel.ReportServerCredential = new System.Net.NetworkCredential(_configuration["ssrsReport:userName"], _configuration["ssrsReport:password"]);

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
            //You can update report options here
        }
    }
}