using BoldReports.Web.ReportViewer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Microsoft.AspNetCore.Cors;
using System.IO;

namespace ReportServices.Controllers.docs
{
    [EnableCors("AllowAllOrigins")]
    [Route("api/[controller]/[action]")]
    public class SubreportPropertiesController : Controller, IReportController
    {
        private Microsoft.Extensions.Caching.Memory.IMemoryCache _cache;
        private IWebHostEnvironment _hostingEnvironment;
        string basePath;

        public SubreportPropertiesController(Microsoft.Extensions.Caching.Memory.IMemoryCache memoryCache,
           IWebHostEnvironment hostingEnvironment)
        {
            _cache = memoryCache;
            _hostingEnvironment = hostingEnvironment;
            basePath = _hostingEnvironment.WebRootPath;
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

        public void OnInitReportOptions(ReportViewerOptions reportOption)
        {
            if (reportOption.SubReportModel != null)
            {
                //Change path of the child report. Load new report from Resources.
                reportOption.SubReportModel.ReportPath = Path.Combine(this.basePath, "resources", "docs", "sub-report-detail.rdl");

                //Modify data source connection string of the child report.
                reportOption.SubReportModel.DataSourceCredentials = new List<BoldReports.Web.DataSourceCredentials>();
                reportOption.SubReportModel.DataSourceCredentials.Add(new BoldReports.Web.DataSourceCredentials("NorthWind", "demoreadonly@data-platform-demo", "password=N@c)=Y8s*1&dh"));

                //Set parameter default values to the child report.
                reportOption.SubReportModel.Parameters = new BoldReports.Web.ReportParameterInfoCollection();
                reportOption.SubReportModel.Parameters.Add(new BoldReports.Web.ReportParameterInfo()
                {
                    Name = "SalesPersonID",
                    Values = new List<string>() { "2" }
                });
            }

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