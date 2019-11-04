using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web;
using System.IO;
using BoldReports.Web.ReportViewer;
using BoldReports.Web.ReportDesigner;
using BoldReports.Web;
using System.Reflection;
using Newtonsoft.Json;


namespace ReportServices.Controllers.demos
{
    [System.Web.Http.Cors.EnableCors(origins: "*", headers: "*", methods: "*")]
    [RoutePrefix("")]
    public class ReportDesignerWebApiController : ApiController, IReportDesignerController, IReportLogger
    {
        const string CachePath = "Cache\\";

        internal ExternalServer Server
        {
            get;
            set;
        }

        internal string ServerURL
        {
            get;
            set;
        }

        internal string AuthorizationHeaderValue
        {
            get;
            set;
        }

        public ReportDesignerWebApiController()
        {
            ExternalServer externalServer = new ExternalServer();
            this.Server = externalServer;
            this.ServerURL = "Sample";
            externalServer.ReportServerUrl = this.ServerURL;
            ReportDesignerHelper.ReportingServer = externalServer;
        }

        [HttpPost]
        public void UploadReportAction()
        {
            ReportDesignerHelper.ProcessDesigner(null, this, HttpContext.Current.Request.Files[0]);
        }

        [HttpGet]
        public object GetImage(string key, string image)
        {
            return ReportDesignerHelper.GetImage(key, image, this);
        }

        [HttpPost]
        [CustomCompression]
        public object PostDesignerAction(Dictionary<string, object> jsonResult)
        {
            string reportType = "";
            if (jsonResult.ContainsKey("customData"))
            {
                string customData = jsonResult["customData"].ToString();
                reportType = (string)(JsonConvert.DeserializeObject(customData) as dynamic).reportType;
            }
            else if (!string.IsNullOrEmpty(HttpContext.Current.Request.Form["customData"]))
            {
                string customData = JsonConvert.DeserializeObject(HttpContext.Current.Request.Form["customData"]).ToString();
                reportType = (JsonConvert.DeserializeObject(customData) as dynamic).reportType;
            }
            this.Server.reportType = String.IsNullOrEmpty(reportType) ? "RDL" : reportType;
            return ReportDesignerHelper.ProcessDesigner(jsonResult, this, null);
        }

        [CustomCompression]
        public object PostReportAction(Dictionary<string, object> jsonResult)
        {
            return ReportHelper.ProcessReport(jsonResult, this as IReportController);
        }

        public void OnInitReportOptions(ReportViewerOptions reportOption)
        {
            reportOption.ReportModel.ReportingServer = this.Server;
            reportOption.ReportModel.ReportServerUrl = this.ServerURL;
            reportOption.ReportModel.ReportServerCredential = new NetworkCredential("Sample", "Passwprd");
        }

        public void OnReportLoaded(ReportViewerOptions reportOption)
        {
        }

        public object GetResource(string key, string resourcetype, bool isPrint)
        {
            return ReportHelper.GetResource(key, resourcetype, isPrint);
        }

        public bool UploadFile(HttpPostedFile httpPostedFile)
        {
            string targetFolder = HttpContext.Current.Server.MapPath("~/");
            string fileName = !string.IsNullOrEmpty(ReportDesignerHelper.SaveFileName) ? ReportDesignerHelper.SaveFileName : Path.GetFileName(httpPostedFile.FileName);
            targetFolder += CachePath;

            if (!Directory.Exists(targetFolder))
            {
                Directory.CreateDirectory(targetFolder);
            }

            if (!Directory.Exists(targetFolder + "\\" + ReportDesignerHelper.EJReportDesignerToken))
            {
                Directory.CreateDirectory(targetFolder + "\\" + ReportDesignerHelper.EJReportDesignerToken);
            }

            httpPostedFile.SaveAs(targetFolder + "\\" + ReportDesignerHelper.EJReportDesignerToken + "\\" + fileName);
            return true;
        }

        public List<FileModel> GetFiles(FileType fileType)
        {
            List<FileModel> databases = new List<FileModel>();
            var folderPath = HttpContext.Current.Server.MapPath("~/") + CachePath + ReportDesignerHelper.EJReportDesignerToken + "\\";

            if (Directory.Exists(folderPath))
            {
                DirectoryInfo dinfo = new DirectoryInfo(folderPath);
                FileInfo[] Files = dinfo.GetFiles(this.GetFileExtension(fileType));

                foreach (FileInfo file in Files)
                {
                    databases.Add(new FileModel() { Name = file.Name, Path = "../" + CachePath + ReportDesignerHelper.EJReportDesignerToken + "/" + file.Name });
                }
            }

            return databases;
        }

        private string GetFileExtension(FileType fileType)
        {
            if (fileType == FileType.Sdf)
            {
                return "*.sdf";
            }
            else if (fileType == FileType.Xml)
            {
                return "*.xml";
            }

            return "*.rdl";
        }

        public string GetFilePath(string fileName)
        {
            string targetFolder = HttpContext.Current.Server.MapPath("~/");
            targetFolder += CachePath;

            if (!Directory.Exists(targetFolder))
            {
                Directory.CreateDirectory(targetFolder);
            }

            if (!Directory.Exists(targetFolder + "\\" + ReportDesignerHelper.EJReportDesignerToken))
            {
                Directory.CreateDirectory(targetFolder + "\\" + ReportDesignerHelper.EJReportDesignerToken);
            }

            var folderPath = HttpContext.Current.Server.MapPath("~/") + CachePath + ReportDesignerHelper.EJReportDesignerToken + "\\";
            return folderPath + fileName;
        }


        public FileModel GetFile(string filename, bool isOverride)
        {
            throw new NotImplementedException();
        }

        public void LogError(string message, Exception exception, MethodBase methodType, ErrorType errorType)
        {
            ReportServices.WebApiApplication.log.DebugFormat("Logging Error Message Time: {0};  Error: {1}, Method: {2}; ErrorType: {3}", DateTime.Now, message, methodType, errorType == ErrorType.Error ? "Error" : "Info");
        }

        public void LogError(string errorCode, string message, Exception exception, string errorDetail, string methodName, string className)
        {
            ReportServices.WebApiApplication.log.DebugFormat("Logging Error Message Time: {0};  Error: {1}, Method: {2}; ErrorType: {3}", DateTime.Now, message, methodName, "Error");
        }

    }
}
