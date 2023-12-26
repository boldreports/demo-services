using BoldReports.Web.ReportViewer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using ReportServices.Model;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Cors;

namespace ReportServices.Controllers.docs
{
    [EnableCors("AllowAllOrigins")]
    [Route("api/[controller]/[action]")]
    public class SubreportDataSourcesController : Controller, IReportController
    {
        private Microsoft.Extensions.Caching.Memory.IMemoryCache _cache;
        private IWebHostEnvironment _hostingEnvironment;
        string basePath;

        public SubreportDataSourcesController(Microsoft.Extensions.Caching.Memory.IMemoryCache memoryCache,
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

        //Method will be called when initialize the report options before start processing the report     
        public void OnInitReportOptions(ReportViewerOptions reportOption)
        {
            if (reportOption.SubReportModel != null)
            {
                // Opens the report from application Resources folder using FileStream and loads the sub report stream.
                FileStream reportStream = new FileStream(this.basePath + @"/Resources/docs/product-list.rdlc", FileMode.Open, FileAccess.Read);
                reportOption.SubReportModel.Stream = reportStream;                
            }
            else
            {
                FileStream reportStream = new FileStream(this.basePath + @"/Resources/docs/product-list-main.rdlc", FileMode.Open, FileAccess.Read);
                reportOption.ReportModel.Stream = reportStream;
            }

            string resourcesPath = this.basePath;

            reportOption.ReportModel.ExportResources.Scripts = new List<string>
            {
                resourcesPath + @"\scripts\bold-reports\common\bold.reports.common.min.js",
                resourcesPath + @"\scripts\bold-reports\common\bold.reports.widgets.min.js",
                //Chart component script
                resourcesPath + @"\scripts\bold-reports\data-visualization\ej.chart.min.js",
                //Report Viewer Script
                resourcesPath + @"\scripts\bold-reports\bold.report-viewer.min.js"
            };

            reportOption.ReportModel.ExportResources.DependentScripts = new List<string>
            {
                resourcesPath + @"\scripts\dependent\jquery.min.js"
            };
        }

        //Method will be called when reported is loaded
        public void OnReportLoaded(ReportViewerOptions reportOption)
        {
            //You can update report options here
            if (reportOption.SubReportModel != null)
            {
                //Set child subreport data source
                reportOption.SubReportModel.DataSources = new BoldReports.Web.ReportDataSourceCollection();
                reportOption.SubReportModel.DataSources.Add(new BoldReports.Web.ReportDataSource { Name = "list", Value = ProductList.GetData() });
            }
            else
            {
                reportOption.SubReportModel.DataSources = new BoldReports.Web.ReportDataSourceCollection();
                reportOption.SubReportModel.DataSources.Add(new BoldReports.Web.ReportDataSource { Name = "list", Value = ProductList.GetData() });
            }
        }
    }
}