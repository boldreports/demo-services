using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using BoldReports.Web.ReportViewer;
using Microsoft.AspNetCore.Hosting;
using System.Net.Mail;
using System.Net.Mime;
using Microsoft.AspNetCore.Cors;

namespace ReportServices.Controllers.docs
{
    [EnableCors("AllowAllOrigins")]
    [Route("api/[controller]/[action]")]
    public class ReportViewerController : Controller, IReportController
    {
        private Microsoft.Extensions.Caching.Memory.IMemoryCache _cache;
        private IWebHostEnvironment _hostingEnvironment;
        string basePath;

        public ReportViewerController(Microsoft.Extensions.Caching.Memory.IMemoryCache memoryCache,
           IWebHostEnvironment hostingEnvironment)
        {
            _cache = memoryCache;
            _hostingEnvironment = hostingEnvironment;
            basePath = _hostingEnvironment.WebRootPath;
        }
        //Post action for processing the rdl/rdlc report 
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
            //You can update report options here

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

        public object SendEmail(Dictionary<string, object> jsonResult)
        {
            string _token = jsonResult["reportViewerToken"].ToString();
            var stream = ReportHelper.GetReport(_token, jsonResult["exportType"].ToString(), this,  this._cache);
            stream.Position = 0;

            if (!ComposeEmail(stream, jsonResult["reportName"].ToString()))
            {
                return "Mail not sent !!!";
            }

            return "Mail Sent !!!";
        }

        public bool ComposeEmail(Stream stream, string reportName)
        {
            try
            {
                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");
                mail.IsBodyHtml = true;
                mail.From = new MailAddress("xx@gmail.com");
                mail.To.Add("xx@gmail.com");
                mail.Subject = "Report Name : " + reportName;
                stream.Position = 0;

                if (stream != null)
                {
                    ContentType ct = new ContentType();
                    ct.Name = reportName + DateTime.Now.ToString() + ".pdf";
                    System.Net.Mail.Attachment attachment = new System.Net.Mail.Attachment(stream, ct);
                    mail.Attachments.Add(attachment);
                }

                SmtpServer.Port = 587;
                SmtpServer.Credentials = new System.Net.NetworkCredential("xx@gmail.com", "xx");
                SmtpServer.EnableSsl = true;
                SmtpServer.Send(mail);

                return true;
            }
            catch
            {
                return false;
            }
        }

        //Method will be called when reported is loaded
        public void OnReportLoaded(ReportViewerOptions reportOption)
        {
            //You can update report options here
        }
    }
}