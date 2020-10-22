using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Http;
using BoldReports.Web;
using BoldReports.Web.ReportViewer;
using BoldReports.Base.Logger;
using ReportServices.Models;
using System.Data;

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
            LogExtension.LogError(message, exception, methodType, errorType == ErrorType.Error ? "Error" : "Info");
        }

        public void LogError(string errorCode, string message, Exception exception, string errorDetail, string methodName, string className)
        {
            LogExtension.LogError(message, exception, System.Reflection.MethodBase.GetCurrentMethod(), errorCode + "-" + errorDetail);
        }

        public void OnInitReportOptions(ReportViewerOptions reportOption)
        {
            reportOption.ReportModel.EmbedImageData = true;
            string reportName = reportOption.ReportModel.ReportPath;
            string directoryName = Path.GetDirectoryName(reportName);
            if (directoryName.Length <= 0)
            {
                reportOption.ReportModel.ReportPath = HttpContext.Current.Server.MapPath(resourceRootLoc + reportName);
            }
            if (reportName == "load-large-data.rdlc")
            {
                SqlQuery.getJson();
                reportOption.ReportModel.DataSources.Add(new ReportDataSource("SalesOrderDetail", HttpContext.Current.Cache.Get("SalesOrderDetail") as DataTable));
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
