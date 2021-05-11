using ReportServices.Model;
using BoldReports.Web.ReportViewer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;

namespace ReportServices.Controllers.docs
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class SubreportDataSourcesController : ApiController, IReportController
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
            if (reportOption.SubReportModel != null)
            {
                // Opens the report from application Resources folder using FileStream and loads the sub report stream.
                FileStream reportStream = new FileStream(System.Web.Hosting.HostingEnvironment.MapPath(@"~/Resources/docs/product-list.rdlc"), FileMode.Open, FileAccess.Read);
                reportOption.SubReportModel.Stream = reportStream;                
            }
            else
            {
                FileStream reportStream = new FileStream(System.Web.Hosting.HostingEnvironment.MapPath(@"~/Resources/docs/product-list-main.rdlc"), FileMode.Open, FileAccess.Read);
                reportOption.ReportModel.Stream = reportStream;
            }

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