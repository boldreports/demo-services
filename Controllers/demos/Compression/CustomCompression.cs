using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Web.Http.Filters;
using System.Net.Http;

namespace ReportServices.Controllers.demos
{
    public class CustomCompression: ActionFilterAttribute
    {
        public override void OnActionExecuted(HttpActionExecutedContext actionContext)
        {
            bool isCompressionSupported = CompressionHelper.IsCompressionSupported();
            string contentType = HttpContext.Current.Request.Headers["Content-Type"];

            if (isCompressionSupported && (!string.IsNullOrEmpty(contentType) && (contentType.Contains("application/json"))))
            {
                string acceptEncoding = HttpContext.Current.Request.Headers["Accept-Encoding"];
                var content = actionContext.Response.Content;
                var byteArray = content == null ? null : content.ReadAsByteArrayAsync().Result;
                MemoryStream memoryStream = new MemoryStream(byteArray);

                if (acceptEncoding.Contains("gzip"))
                {
                    actionContext.Response.Content = new ByteArrayContent(CompressionHelper.Compress(memoryStream.ToArray(), true));
                    actionContext.Response.Content.Headers.Remove("Content-Type");

                    actionContext.Response.Content.Headers.Add("Content-encoding", "gzip");
                    actionContext.Response.Content.Headers.Add("Content-Type", "application/json");
                }
                else if (acceptEncoding.Contains("deflate"))
                {
                    actionContext.Response.Content = new ByteArrayContent(CompressionHelper.Compress(memoryStream.ToArray(), false));
                    actionContext.Response.Content.Headers.Remove("Content-Type");

                    actionContext.Response.Content.Headers.Add("Content-encoding", "deflate");
                    actionContext.Response.Content.Headers.Add("Content-Type", "application/json");
                }

            }
            base.OnActionExecuted(actionContext);
        }
    }
}
