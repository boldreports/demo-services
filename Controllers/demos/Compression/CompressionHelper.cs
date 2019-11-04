using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Web;

namespace ReportServices.Controllers.demos
{
    public class CompressionHelper
    {
        public static byte[] Compress(byte[] data, bool useGZipCompression)
        {
            CompressionLevel compressionLevel = CompressionLevel.Optimal;

            using (MemoryStream memoryStream = new MemoryStream())
            {
                if (useGZipCompression)
                {
                    using (GZipStream gZipStream = new GZipStream(memoryStream, compressionLevel, true))
                    {
                        gZipStream.Write(data, 0, data.Length);
                    }
                }
                else
                {
                    using (DeflateStream dZipStream = new DeflateStream(memoryStream, compressionLevel, true))
                    {
                        dZipStream.Write(data, 0, data.Length);
                    }
                }

                return memoryStream.ToArray();
            }
        }

        public static bool IsCompressionSupported()
        {
            string AcceptEncoding = HttpContext.Current.Request.Headers["Accept-Encoding"];

            return ((!string.IsNullOrEmpty(AcceptEncoding) &&
                    (AcceptEncoding.Contains("gzip") || AcceptEncoding.Contains("deflate"))));
        }
    }
}
