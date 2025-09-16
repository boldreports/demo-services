using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using BoldReports.Web.ReportViewer;
using Microsoft.AspNetCore.Hosting;
using System.Net.Mail;
using System.Net.Mime;
using Microsoft.AspNetCore.Cors;
using Microsoft.Extensions.Configuration;

namespace ReportServices.Controllers.docs
{
    [EnableCors("AllowAllOrigins")]
    [Route("api/[controller]/[action]")]
    public class ReportViewerController : Controller, IReportController
    {
        private Microsoft.Extensions.Caching.Memory.IMemoryCache _cache;
        private IWebHostEnvironment _hostingEnvironment;
        private readonly IConfiguration _configuration; 
        string basePath;

        public ReportViewerController(Microsoft.Extensions.Caching.Memory.IMemoryCache memoryCache,
           IWebHostEnvironment hostingEnvironment, IConfiguration configuration)
        {
            _cache = memoryCache;
            _hostingEnvironment = hostingEnvironment;
            basePath = _hostingEnvironment.WebRootPath;
            _configuration = configuration;
        }
        //Post action for processing the rdl/rdlc report 
        public object PostReportAction([FromBody] Dictionary<string, object> jsonResult)
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
            reportOption.ReportModel.EmbedImageData = true;
            string reportName = reportOption.ReportModel.ReportPath;
            string basePath = _hostingEnvironment.WebRootPath;
            string reportPath = reportName.Replace("~/Resource", "resource").Replace("~/", "");
            if ((dynamic)reportOption.ReportModel.ReportPath.Split('.').Length <= 1 && reportOption.ReportModel.ProcessingMode.ToString() == "Remote")
            {
                reportPath += ".rdl";
            }
            else if ((dynamic)reportOption.ReportModel.ReportPath.Split('.').Length <= 1 && reportOption.ReportModel.ProcessingMode.ToString() == "Local")
            {
                reportPath += ".rdlc";
            }
            var reportFileInfo = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(Path.Combine(basePath)).GetFileInfo(reportPath);
            reportOption.ReportModel.Stream = reportFileInfo.Exists ? reportFileInfo.CreateReadStream() : throw new FileNotFoundException();

            if (reportOption.ReportModel.FontSettings == null)
            {
                reportOption.ReportModel.FontSettings = new BoldReports.RDL.Data.FontSettings();
            }
            reportOption.ReportModel.FontSettings.BasePath = Path.Combine(_hostingEnvironment.WebRootPath, "fonts");
        }

        public object SendEmail(Dictionary<string, object> jsonResult)
        {
            string _token = jsonResult["reportViewerToken"].ToString();
            var stream = ReportHelper.GetReport(_token, jsonResult["exportType"].ToString(), this, this._cache);
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
                SmtpServer.Credentials = new System.Net.NetworkCredential(_configuration["reportViewer:userName"], _configuration["reportViewer:password"]);
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
