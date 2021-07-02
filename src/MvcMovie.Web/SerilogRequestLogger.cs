using Microsoft.AspNetCore.Http;

using Serilog;

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvcMovie.Web
{
    public class SerilogRequestLogger
    {
        readonly RequestDelegate _next;

        public SerilogRequestLogger(RequestDelegate next)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
        }

        public async Task Invoke(HttpContext httpContext)
        {
            if (httpContext == null) throw new ArgumentNullException(nameof(httpContext));

            string requestBody = "";
            HttpRequestRewindExtensions.EnableBuffering(httpContext.Request);
            Stream body = httpContext.Request.Body;
            byte[] buffer = new byte[Convert.ToInt32(httpContext.Request.ContentLength)];
            await httpContext.Request.Body.ReadAsync(buffer, 0, buffer.Length);
            requestBody = Encoding.UTF8.GetString(buffer);
            body.Seek(0, SeekOrigin.Begin);
            httpContext.Request.Body = body;

            Log.ForContext("RequestHeaders",
                    httpContext.Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()),
                    destructureObjects: true)
                .ForContext("Protocol", httpContext.Request.Protocol)
                .ForContext("Schema", httpContext.Request.Scheme)
                .ForContext("QueryString", httpContext.Request.QueryString.Value)
                .ForContext("EndpointName", httpContext.GetEndpoint()?.DisplayName)
                .ForContext("RequestBody", requestBody)
                .Information("Request information {RequestMethod} {RequestPath} information", httpContext.Request.Method, httpContext.Request.Path);

            using (var responseBodyMemoryStream = new MemoryStream())
            {
                var originalResponseBodyReference = httpContext.Response.Body;
                httpContext.Response.Body = responseBodyMemoryStream;

                await _next(httpContext);

                httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
                var responseBody = await new StreamReader(httpContext.Response.Body).ReadToEndAsync();
                httpContext.Response.Body.Seek(0, SeekOrigin.Begin);

                Log.ForContext("ResponseBody", responseBody)
                    .Information("Response information {RequestMethod} {RequestPath} {statusCode}", httpContext.Request.Method, httpContext.Request.Path, httpContext.Response.StatusCode);

                await responseBodyMemoryStream.CopyToAsync(originalResponseBodyReference);
            }
        }
    }
}
