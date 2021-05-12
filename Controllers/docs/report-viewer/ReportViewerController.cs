using BoldReports.Web.ReportViewer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Net.Mime;
using System.Web.Http;
using System.Web.Http.Cors;

namespace ReportServices.Controllers.docs
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class ReportViewerController : ApiController, IReportController
    {
        //Post action for processing the rdl/rdlc report 
        public object PostReportAction(Dictionary<string, object> jsonResult)
        {
            return ReportHelper.ProcessReport(jsonResult, this);
        }

        //Get action for getting resources from the report
        [System.Web.Http.ActionName("GetResource")]
        [AcceptVerbs("GET")]
        public object GetResource(string key, string resourcetype, bool isPrint)
        {
            return ReportHelper.GetResource(key, resourcetype, isPrint);
        }

        //Method will be called when initialize the report options before start processing the report        
        public void OnInitReportOptions(ReportViewerOptions reportOption)
        {
            //You can update report options here

            string resourcesPath = System.Web.Hosting.HostingEnvironment.MapPath("~/Scripts");

            reportOption.ReportModel.ExportResources.Scripts = new List<string>
            {
                resourcesPath + @"\bold-reports\common\bold.reports.common.min.js",
                resourcesPath + @"\bold-reports\common\bold.reports.widgets.min.js",
                //Chart component script
                resourcesPath + @"\bold-reports\data-visualization\ej.chart.min.js",
                //Report Viewer Script
                resourcesPath + @"\bold-reports\bold.report-viewer.min.js"
            };

            reportOption.ReportModel.ExportResources.DependentScripts = new List<string>
            {
                resourcesPath + @"\dependent\jquery.min.js"
            };
        }

        public object SendEmail(Dictionary<string, object> jsonResult)
        {
            string _token = jsonResult["reportViewerToken"].ToString();
            var stream = ReportHelper.GetReport(_token, jsonResult["exportType"].ToString());
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