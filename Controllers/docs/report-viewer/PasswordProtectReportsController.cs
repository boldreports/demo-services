using BoldReports.Web.ReportViewer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Microsoft.AspNetCore.Cors;


namespace ReportServices.Controllers.docs
{
    [EnableCors("AllowAllOrigins")]
    [Route("api/[controller]/[action]")]
    public class PasswordProtectReportsController : Controller, IReportController
    {
        private Microsoft.Extensions.Caching.Memory.IMemoryCache _cache;
        private IWebHostEnvironment _hostingEnvironment;
        string basePath;

        public PasswordProtectReportsController(Microsoft.Extensions.Caching.Memory.IMemoryCache memoryCache,
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

            //PDF encryption
            reportOption.ReportModel.PDFOptions = new BoldReports.Writer.PDFOptions();
            reportOption.ReportModel.PDFOptions.Security = new Syncfusion.Pdf.Security.PdfSecurity()
            {
                UserPassword = "password"
            };

            //Word encryption
            reportOption.ReportModel.WordOptions = new BoldReports.Writer.WordOptions()
            {
                EncryptionPassword = "password"
            };

            //Excel encryption

            reportOption.ReportModel.ExcelOptions = new BoldReports.Writer.ExcelOptions()
            {
                PasswordToModify = "password",
                PasswordToOpen = "password"
            };
            //PPT encryption
            reportOption.ReportModel.PPTOptions = new BoldReports.Writer.PPTOptions()
            {
                EncryptionPassword = "password"
            };
        }

        //Method will be called when reported is loaded
        public void OnReportLoaded(ReportViewerOptions reportOption)
        {
            //You can update report options here
        }
    }
}