using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Http;
using BoldReports.Web;
using BoldReports.Web.ReportViewer;

namespace ReportServices.Controllers.demos
{
    [System.Web.Http.Cors.EnableCors(origins: "*", headers: "*", methods: "*")]
    [RoutePrefix("")]
    public class ReportViewerWebApiController : ApiController, IReportController, IReportLogger
    {
        private string resourceRootLoc = "~/Resources/demos/Report/";
        public object GetResource(string key, string resourcetype, bool isPrint)
        {
            return ReportHelper.GetResource(key, resourcetype, isPrint);
        }

        public void LogError(string message, Exception exception, MethodBase methodType, ErrorType errorType)
        {
            ReportServices.WebApiApplication.log.DebugFormat("Logging Error Message Time: {0};  Error: {1}, Method: {2}; ErrorType: {3}", DateTime.Now, message, methodType, errorType == ErrorType.Error ? "Error" : "Info");
        }

        public void LogError(string errorCode, string message, Exception exception, string errorDetail, string methodName, string className)
        {
            ReportServices.WebApiApplication.log.DebugFormat("Logging Error Message Time: {0};  Error: {1}, Method: {2}; ErrorType: {3}", DateTime.Now, message, methodName, "Error");
        }

        public void OnInitReportOptions(ReportViewerOptions reportOption)
        {
            string reportName = reportOption.ReportModel.ReportPath;
            string directoryName = Path.GetDirectoryName(reportName);
            if (directoryName.Length <= 0)
            {
                reportOption.ReportModel.ReportPath = HttpContext.Current.Server.MapPath(resourceRootLoc + reportName);
            }
        }

        public void OnReportLoaded(ReportViewerOptions reportOption)
        {

        }

        [CustomCompression]
        public object PostReportAction(Dictionary<string, object> jsonResult)
        {
            return ReportHelper.ProcessReport(jsonResult, this);
        }
    }
}
