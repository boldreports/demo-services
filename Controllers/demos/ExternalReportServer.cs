﻿#region Copyright Syncfusion Inc. 2001-2025.
// Copyright Syncfusion Inc. 2001-2025. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Xml.Serialization;
using System.Xml;
using System.Net.Http;
using BoldReports.ServerProcessor;
using System.Web;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;

namespace ReportServices.Controllers.demos
{
    public sealed class ExternalServer : ReportingServer
    {
        private IWebHostEnvironment _hostingEnvironment;
        string basePath;
        public string reportType
        {
            get;
            set;
        }

        public ExternalServer(IWebHostEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
            basePath = _hostingEnvironment.WebRootPath;
        }
        public override List<CatalogItem> GetItems(string folderName, ItemTypeEnum type, string permissionType)
        {
            List<CatalogItem> _items = new List<CatalogItem>();
            string targetFolder = Path.Combine(this.basePath, "resources", "demos");

            if (type == ItemTypeEnum.Folder || type == ItemTypeEnum.Report)
            {
                targetFolder = Path.Combine(targetFolder, "Report");
                if (!(string.IsNullOrEmpty(folderName) || folderName.Trim() == "/"))
                {
                    targetFolder = targetFolder + folderName;
                }
            }

            if (type == ItemTypeEnum.DataSet)
            {
                foreach (var file in Directory.GetFiles(Path.Combine(targetFolder, "DataSet")))
                {
                    CatalogItem catalogItem = new CatalogItem();
                    catalogItem.Name = Path.GetFileNameWithoutExtension(file);
                    catalogItem.Type = ItemTypeEnum.DataSet;
                    catalogItem.Id = Regex.Replace(catalogItem.Name, @"[^0-9a-zA-Z]+", "_");
                    _items.Add(catalogItem);
                }
            }
            else if (type == ItemTypeEnum.DataSource)
            {
                foreach (var file in Directory.GetFiles(Path.Combine(targetFolder, "DataSource")))
                {
                    CatalogItem catalogItem = new CatalogItem();
                    catalogItem.Name = Path.GetFileNameWithoutExtension(file);
                    catalogItem.Type = ItemTypeEnum.DataSource;
                    catalogItem.Id = Regex.Replace(catalogItem.Name, @"[^0-9a-zA-Z]+", "_");
                    _items.Add(catalogItem);
                }
            }
            else if (type == ItemTypeEnum.Folder)
            {
                foreach (var file in Directory.GetDirectories(targetFolder))
                {
                    CatalogItem catalogItem = new CatalogItem();
                    catalogItem.Name = Path.GetFileNameWithoutExtension(file);
                    catalogItem.Type = ItemTypeEnum.Folder;
                    catalogItem.Id = Regex.Replace(catalogItem.Name, @"[^0-9a-zA-Z]+", "_");
                    _items.Add(catalogItem);
                }
            }
            else if (type == ItemTypeEnum.Report)
            {
                string reportTypeExt = this.reportType == "RDLC" ? ".rdlc" : ".rdl";

                foreach (var file in Directory.GetFiles(targetFolder, "*" + reportTypeExt))
                {
                    CatalogItem catalogItem = new CatalogItem();
                    catalogItem.Name = Path.GetFileNameWithoutExtension(file);
                    catalogItem.Type = ItemTypeEnum.Report;
                    catalogItem.Id = Regex.Replace(catalogItem.Name, @"[^0-9a-zA-Z]+", "_");
                    _items.Add(catalogItem);
                }
            }

            return _items;
        }

        public override bool CreateReport(string reportName, string folderName, byte[] reportdata, out string exception)
        {
            return base.CreateReport(reportName, folderName, reportdata, out exception);
        }

        public override System.IO.Stream GetReport()
        {
            string targetFolder = Path.Combine(this.basePath, "resources", "demos", "Report");
            string reportPath = Path.HasExtension(this.ReportPath) ? Path.Combine(targetFolder, this.ReportPath) : Path.Combine(targetFolder, $"{this.ReportPath}.{this.reportType.ToLower()}");

            if (File.Exists(reportPath))
            {
                return this.ReadFiles(reportPath);
            }

            return null;
        }

        private Stream ReadFiles(string filePath)
        {
            using (FileStream fileStream = File.OpenRead(filePath))
            {
                fileStream.Position = 0;
                MemoryStream memStream = new MemoryStream();
                memStream.SetLength(fileStream.Length);
                fileStream.Read(memStream.GetBuffer(), 0, (int)fileStream.Length);
                return memStream;
            }
        }

        public override bool EditReport(byte[] reportdata)
        {
            string reportPath = this.ReportPath.TrimStart('/').TrimEnd('/').Trim();
            string reportName = reportPath.Substring(reportPath.IndexOf('/') + 1).Trim();
            string catagoryName = reportPath.Substring(0, reportPath.IndexOf('/') > 0 ? reportPath.IndexOf('/') : 0).Trim();

            string targetFolder = Path.Combine(this.basePath, "resources", "demos", "Report");
            string reportPat = Path.Combine(targetFolder, catagoryName, reportName);
            File.WriteAllBytes(reportPat, reportdata.ToArray());

            return true;
        }

        public override DataSourceDefinition GetDataSourceDefinition(string dataSource)
        {
            if (dataSource != null && dataSource.Contains("/"))
            {
                string[] _dataSrcPathHierarchy = dataSource.Split('/');
                dataSource = _dataSrcPathHierarchy.Last().TrimStart('/');
            }
            string targetFolder = Path.Combine(this.basePath, "resources", "demos", "DataSource");
            string dataSourcePath = Path.Combine(targetFolder, $"{dataSource}.rds");

            if (File.Exists(dataSourcePath))
            {
                var _sharedDatasetInfo = new SharedDatasetinfo();
                var stream = this.ReadFiles(dataSourcePath);
                int length = Convert.ToInt32(stream.Length);
                byte[] data = new byte[length];
                stream.Read(data, 0, length);
                stream.Close();
                return this.GetDataSourceDefinition(data, dataSource, null);
            }

            return null;
        }

        DataSourceDefinition GetDataSourceDefinition(byte[] dataSourceContent, string name, string guid)
        {
            var _rptDefinition = new DataSourceDefinition();
            var _datasourceStream = this.GetFileToStream(dataSourceContent);
            var _umpDefinition = this.DeseralizeObj<DataSourceDefinition>(_datasourceStream);
            _rptDefinition = _umpDefinition;
            return _rptDefinition;
        }

        public override SharedDatasetinfo GetSharedDataDefinition(string dataSet)
        {
            string targetFolder = Path.Combine(this.basePath, "resources", "demos", "DataSet");
            string dataSetPath = Path.Combine(targetFolder, $"{dataSet}.rsd");

            if (File.Exists(dataSetPath))
            {
                var _sharedDatasetInfo = new SharedDatasetinfo();
                var stream = this.ReadFiles(dataSetPath);
                int length = Convert.ToInt32(stream.Length);
                byte[] data = new byte[length];
                stream.Read(data, 0, length);
                stream.Close();
                var _datasetStream = this.GetFileToStream(data);
                _sharedDatasetInfo.DataSetStream = _datasetStream;
                _sharedDatasetInfo.Guid = Guid.Empty.ToString();
                return _sharedDatasetInfo;
            }

            return null;
        }

        T DeseralizeObj<T>(Stream str)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            XmlReader reader = XmlReader.Create(str);
            return (T)serializer.Deserialize(reader);
        }

        private Stream GetFileToStream(byte[] _fileContent)
        {
            MemoryStream memStream = new MemoryStream();
            memStream.Write(_fileContent, 0, _fileContent.Length);
            memStream.Seek(0, SeekOrigin.Begin);
            return memStream;
        }
    }
}
