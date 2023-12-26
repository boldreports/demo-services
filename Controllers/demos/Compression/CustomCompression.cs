using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Net.Http.Headers;

namespace ReportServices.Controllers.demos
{
    public class CustomCompression : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            bool isCompressionSupported = CompressionHelper.IsCompressionSupported(context.HttpContext);
            string contentType = context.HttpContext.Request.Headers["Content-Type"];

            if (isCompressionSupported && (!string.IsNullOrEmpty(contentType) && (contentType.Contains("application/json"))))
            {
                string acceptEncoding = context.HttpContext.Request.Headers["Accept-Encoding"];
                var content = context.Result as ObjectResult;

                if (content != null)
                {
                    var byteArray = content.Value as byte[];

                    if (byteArray != null)
                    {
                        MemoryStream memoryStream = new MemoryStream(byteArray);

                        if (acceptEncoding.Contains("gzip"))
                        {
                            context.HttpContext.Response.Headers.Remove(HeaderNames.ContentType);
                            context.HttpContext.Response.Headers.Add(HeaderNames.ContentEncoding, "gzip");
                            context.HttpContext.Response.Headers.Add(HeaderNames.ContentType, "application/json");

                            context.Result = new FileContentResult(CompressionHelper.Compress(memoryStream.ToArray(), true), "application/json");
                        }
                        else if (acceptEncoding.Contains("deflate"))
                        {
                            context.HttpContext.Response.Headers.Remove(HeaderNames.ContentType);
                            context.HttpContext.Response.Headers.Add(HeaderNames.ContentEncoding, "deflate");
                            context.HttpContext.Response.Headers.Add(HeaderNames.ContentType, "application/json");

                            context.Result = new FileContentResult(CompressionHelper.Compress(memoryStream.ToArray(), false), "application/json");
                        }
                    }
                }
            }

            base.OnActionExecuted(context);
        }
    }
}
