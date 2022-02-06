using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CommandQuery.Api.Settings;
using Microsoft.Extensions.Logging;

namespace CommandQuery.Api.Middleware
{
    public class CommandQueryMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<CommandQueryMiddleware> _logger;
        private readonly CommandQuerySettings _settings;

        public CommandQueryMiddleware(RequestDelegate next, ILogger<CommandQueryMiddleware> logger, CommandQuerySettings settings)
        {
            _next = next;
            _logger = logger;
            _settings = settings;
        }

        public async Task Invoke(HttpContext context)
        {
            // Wrap the request in a new seekable stream so it does't get consumed.
            // This might happen with [FromBody] or [FromForm] when decorating with Swashbuckle or other similar Middlware...
            using var memStream = new MemoryStream();

            // Copy the body to a Seekable stream
            if (context.Request.ContentLength > 0)
            {
                await context.Request.Body.CopyToAsync(memStream);
            }

            // Reset the stream to the start
            memStream.Seek(0, SeekOrigin.Begin);

            // Make the request the seekable stream as we have already consumed the original one by copying it...
            context.Request.Body = memStream;

            // Check to see if the Controller endpoint has a [Command] Atrribute attached
            var endpoint = context.Features.Get<IEndpointFeature>()?.Endpoint;
            var attribute = endpoint?.Metadata.GetMetadata<CommandAttribute>();

            // If there is no attribute (or we are already on the primary api) then just call the next thing in the stack,
            // which is probably the controller, and then return...
            if (attribute == null || _settings.IsPrimaryApi)
            {
                _logger.LogInformation("Continuing with Request. IsPrimaryApi = " + _settings.IsPrimaryApi);
                // Run the code against this API and then return to prevent redirecting to the command api as it is not required...
                await _next(context);
                return;
            }
            else
            {
                _logger.LogInformation("Redirecting Request to " + _settings.PrimaryApiUrlBase);
            }

            /*
             *
             * Any code after this point is directing command requests from a query API to the command API
             *
             */

            // Create request message
            var requestMessage = new HttpRequestMessage();

            // Copy body
            if (context.Request.ContentLength > 0)
            {
                var streamContent = new StreamContent(memStream);
                requestMessage.Content = streamContent;

                // Try and get a content type for the request body
                string contentType = context.Request.Headers["Content-Type"];

                // Add the content type to the body as it belongs to the body and not the request itself (this needs to be added here as it fails in the loop below).
                if (!string.IsNullOrEmpty(contentType))
                {
                    requestMessage.Content.Headers.Add("Content-Type", contentType);
                }
            }

            // Copy headers
            foreach (var header in context.Request.Headers)
            {
                requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
            }

            var url = CombineAsUrl(_settings.PrimaryApiUrlBase, context.Request.Path, context.Request.QueryString.Value ?? "");

            // Construct the target uri
            var uri = new Uri(url);
            requestMessage.RequestUri = uri;
            requestMessage.Headers.Host = uri.Host;
            requestMessage.Method = new HttpMethod(context.Request.Method);

            // Fetch the response from the Command API and set it's result as our own so we can return it...
            using var response = await ForwardToCommand(context, requestMessage);
            await response.Content.CopyToAsync(context.Response.Body);
        }

        /// <summary>
        /// Executes a request against the Global Command API
        /// </summary>
        private async Task<HttpResponseMessage> ForwardToCommand(HttpContext context, HttpRequestMessage requestMessage)
        {
            // In case you need to handle cookies, currently we do not need to use this. 
            var cookies = new CookieContainer();

            // Make a client handler and do not allow it to follow redirects (these should be passed back through)
            // in theory I don't suspect we will get these anyway but if they do the client should handle them.
            using var httpClientHandler = new HttpClientHandler();
            httpClientHandler.AllowAutoRedirect = false;
            httpClientHandler.CookieContainer = cookies;

            using var client = new HttpClient(httpClientHandler);

            // Send request to Command API
            var responseMessage = await client.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, context.RequestAborted);

            // Construct response to client based on response from the Command API
            context.Response.StatusCode = (int)responseMessage.StatusCode;

            // Copy headers
            foreach (var header in responseMessage.Headers)
            {
                if (header.Key.ToLower() != "set-cookie")
                    context.Response.Headers[header.Key] = header.Value.ToArray();
            }

            // Copy content headers
            foreach (var header in responseMessage.Content.Headers)
            {
                context.Response.Headers[header.Key] = header.Value.ToArray();
            }

            // Remove any headers we do not need
            context.Response.Headers.Remove("transfer-encoding");
            return responseMessage;
        }

        /// <summary>
        /// Helper function to combine url segments
        /// </summary>
        /// <param name="urlSegments"></param>
        /// <returns></returns>
        private static string CombineAsUrl(params string[] urlSegments)
        {
            var url = "";

            foreach (var segment in urlSegments)
            {
                if (string.IsNullOrEmpty(url))
                {
                    url = segment;
                    continue;
                }

                // If the segment is not null or empty append it to the url
                if (!string.IsNullOrEmpty(segment))
                {
                    url = $"{url.TrimEnd('/')}/{segment.TrimStart('/')}";
                }
            }

            return url;
        }
    }
}
