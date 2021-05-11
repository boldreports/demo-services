using BoldReports.Web.ReportDesigner;
using BoldReports.Web.ReportViewer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace ReportServices.Controllers.docs
{
    [System.Web.Http.Cors.EnableCors(origins: "*", headers: "*", methods: "*")]
    public class ReportingAPIController : ApiController, IReportDesignerController
    {
        private string GetFilePath(string itemName, string key)
        {
            string targetFolder = HttpContext.Current.Server.MapPath("~/");
            targetFolder += "Cache";

            if (!Directory.Exists(targetFolder))
            {
                Directory.CreateDirectory(targetFolder);
            }

            if (!Directory.Exists(targetFolder + "\\" + key))
            {
                Directory.CreateDirectory(targetFolder + "\\" + key);
            }

            return targetFolder + "\\" + key + "\\" + itemName;
        }

        [System.Web.Http.ActionName("GetResource")]
        [AcceptVerbs("GET")]
        public object GetImage(string key, string image)
        {
            return ReportDesignerHelper.GetImage(key, image, this);
        }

        public object PostDesignerAction(Dictionary<string, object> jsonResult)
        {
            return ReportDesignerHelper.ProcessDesigner(jsonResult, this, null);
        }

        public bool UploadFile(System.Web.HttpPostedFile httpPostedFile)
        {
            string targetFolder = System.Web.HttpContext.Current.Server.MapPath("~/");
            string fileName = !string.IsNullOrEmpty(ReportDesignerHelper.SaveFileName) ? ReportDesignerHelper.SaveFileName : System.IO.Path.GetFileName(httpPostedFile.FileName);
            targetFolder += "Cache";

            if (!System.IO.Directory.Exists(targetFolder))
            {
                System.IO.Directory.CreateDirectory(targetFolder);
            }

            if (!System.IO.Directory.Exists(targetFolder + "\\" + ReportDesignerHelper.EJReportDesignerToken))
            {
                System.IO.Directory.CreateDirectory(targetFolder + "\\" + ReportDesignerHelper.EJReportDesignerToken);
            }

            httpPostedFile.SaveAs(targetFolder + "\\" + ReportDesignerHelper.EJReportDesignerToken + "\\" + fileName);
            return true;
        }

        public void UploadReportAction()
        {
            ReportDesignerHelper.ProcessDesigner(null, this, System.Web.HttpContext.Current.Request.Files[0]);
        }

        [System.Web.Http.ActionName("GetResource")]
        [AcceptVerbs("GET")]		
        public object GetResource(string key, string resourcetype, bool isPrint)
        {
            return ReportHelper.GetResource(key, resourcetype, isPrint);
        }

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

        public bool SetData(string key, string itemId, ItemInfo itemData, out string errMsg)
        {
            errMsg = string.Empty;
            try
            {
                if (itemData.Data != null)
                {
                    File.WriteAllBytes(this.GetFilePath(itemId, key), itemData.Data);
                }
                else if (itemData.PostedFile != null)
                {
                    var fileName = itemId;
                    if (string.IsNullOrEmpty(itemId))
                    {
                        fileName = Path.GetFileName(itemData.PostedFile.FileName);
                    }
                    itemData.PostedFile.SaveAs(this.GetFilePath(fileName, key));
                }
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
                return false;
            }
            return true;
        }

        public ResourceInfo GetData(string key, string itemId)
        {
            var resource = new ResourceInfo();
            try
            {
                resource.Data = File.ReadAllBytes(this.GetFilePath(itemId, key));
            }
            catch (Exception ex)
            {
                resource.ErrorMessage = ex.Message;
            }
            return resource;
        }
        public void OnReportLoaded(ReportViewerOptions reportOption)
        {
            //You can update report options here
        }

        public object PostReportAction(Dictionary<string, object> jsonResult)
        {
            return ReportHelper.ProcessReport(jsonResult, this as IReportController);
        }

        
    }
}