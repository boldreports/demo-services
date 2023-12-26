using Microsoft.AspNetCore.Http;
using System.IO;
using System.IO.Compression;

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

        public static bool IsCompressionSupported(HttpContext context)
        {
            string acceptEncoding = context.Request.Headers["Accept-Encoding"];

            return !string.IsNullOrEmpty(acceptEncoding) &&
                   (acceptEncoding.Contains("gzip") || acceptEncoding.Contains("deflate"));
        }
    }
}
