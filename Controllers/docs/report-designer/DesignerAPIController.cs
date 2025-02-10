using Microsoft.AspNetCore.Mvc;
using BoldReports.Web.ReportDesigner;
using BoldReports.Web.ReportViewer;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.Cors;

namespace ReportServices.Controllers.docs
{
    [EnableCors("AllowAllOrigins")]
    [Route("api/[controller]/[action]")]
    public class ReportingAPIController : Controller, IReportDesignerController
    {
        private Microsoft.Extensions.Caching.Memory.IMemoryCache _cache;
        private IWebHostEnvironment _hostingEnvironment;
        private string _basePath;
        public ReportingAPIController(Microsoft.Extensions.Caching.Memory.IMemoryCache memoryCache, IWebHostEnvironment hostingEnvironment)
        {
            _cache = memoryCache;
            _hostingEnvironment = hostingEnvironment;
            _basePath = _hostingEnvironment.WebRootPath;
        }
        private string GetFilePath(string itemName, string key)
        {
            string targetFolder = Path.Combine(Directory.GetCurrentDirectory(), "Cache");

            if (!Directory.Exists(targetFolder))
            {
                Directory.CreateDirectory(targetFolder);
            }

            if (!Directory.Exists(Path.Combine(targetFolder, key)))
            {
                Directory.CreateDirectory(Path.Combine(targetFolder, key));
            }

            return Path.Combine(targetFolder, key, itemName);
        }

        [ActionName("GetResource")]
        [AcceptVerbs("GET")]
        public object GetImage(string key, string image)
        {
            return ReportDesignerHelper.GetImage(key, image, this);
        }

        [HttpPost]
        public object PostDesignerAction([FromBody] Dictionary<string, object> jsonResult)
        {
            return ReportDesignerHelper.ProcessDesigner(jsonResult, this, null, this._cache);
        }

        [HttpPost]
        public object PostFormDesignerAction()
        {
            return ReportDesignerHelper.ProcessDesigner(null, this, null, this._cache);
        }

        [HttpPost]
        public object PostFormReportAction()
        {
            return ReportHelper.ProcessReport(null, this, this._cache);
        }

        [HttpPost]
        public void UploadReportAction()
        {
            ReportDesignerHelper.ProcessDesigner(null, this, HttpContext.Request.Form.Files[0], this._cache);
        }

        [ActionName("GetResource")]
        [AcceptVerbs("GET")]
        public object GetResource(ReportResource resource)
        {
            return ReportHelper.GetResource(resource, this, _cache);
        }

        public void OnInitReportOptions(ReportViewerOptions reportOption)
        {
            // You can update report options here
            string resourcesPath = Path.Combine(Directory.GetCurrentDirectory(), "Scripts");

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

            if (reportOption.ReportModel.FontSettings == null)
            {
                reportOption.ReportModel.FontSettings = new BoldReports.RDL.Data.FontSettings();
            }
            reportOption.ReportModel.FontSettings.BasePath = Path.Combine(_hostingEnvironment.WebRootPath, "fonts");
        }

        public bool SetData(string key, string itemId, ItemInfo itemData, out string errMsg)
        {
            errMsg = string.Empty;
            try
            {
                if (itemData.Data != null)
                {
                    System.IO.File.WriteAllBytes(GetFilePath(itemId, key), itemData.Data);
                }
                else if (itemData.PostedFile != null)
                {
                    var fileName = itemId;
                    if (string.IsNullOrEmpty(itemId))
                    {
                        fileName = System.IO.Path.GetFileName(itemData.PostedFile.FileName);
                    }

                    using (System.IO.MemoryStream stream = new System.IO.MemoryStream())
                    {
                        itemData.PostedFile.OpenReadStream().CopyTo(stream);
                        byte[] bytes = stream.ToArray();
                        var writePath = this.GetFilePath(fileName, key);

                        if (System.IO.File.Exists(writePath))
                        {
                            System.IO.File.Delete(writePath);
                        }

                        System.IO.File.WriteAllBytes(writePath, bytes);
                        stream.Close();
                        stream.Dispose();
                    }
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
                resource.Data = System.IO.File.ReadAllBytes(GetFilePath(itemId, key));
            }
            catch (Exception ex)
            {
                resource.ErrorMessage = ex.Message;
            }
            return resource;
        }

        public void OnReportLoaded(ReportViewerOptions reportOption)
        {
            // You can update report options here
        }

        [HttpPost]
        public object PostReportAction([FromBody] Dictionary<string, object> jsonResult)
        {
            return ReportHelper.ProcessReport(jsonResult, this as IReportController, this._cache);
        }
    }
}
