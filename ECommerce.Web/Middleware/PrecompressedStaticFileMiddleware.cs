namespace ECommerce.Web.Middleware
{
    public class PrecompressedStaticFileMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IWebHostEnvironment _env;

        private static readonly Dictionary<string, string> ContentTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            [".css"] = "text/css",
            [".js"] = "application/javascript"
        };

        public PrecompressedStaticFileMiddleware(RequestDelegate next, IWebHostEnvironment env)
        {
            _next = next;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value;

            if (string.IsNullOrEmpty(path) ||
                !path.StartsWith("/lib/", StringComparison.OrdinalIgnoreCase) ||
                !ContentTypes.TryGetValue(Path.GetExtension(path), out var contentType))
            {
                await _next(context);
                return;
            }

            var physicalPath = Path.Combine(
                _env.WebRootPath,
                path.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

            var acceptEncoding = context.Request.Headers.AcceptEncoding.ToString();

            string? encoding = null;
            string? filePath = null;

            if (acceptEncoding.Contains("br", StringComparison.OrdinalIgnoreCase) && File.Exists(physicalPath + ".br"))
            {
                encoding = "br";
                filePath = physicalPath + ".br";
            }
            else if (acceptEncoding.Contains("gzip", StringComparison.OrdinalIgnoreCase) && File.Exists(physicalPath + ".gz"))
            {
                encoding = "gzip";
                filePath = physicalPath + ".gz";
            }

            if (encoding is null || filePath is null)
            {
                await _next(context);
                return;
            }

            context.Response.Headers.ContentEncoding = encoding;
            context.Response.Headers.Vary = "Accept-Encoding";
            context.Response.Headers.CacheControl = "public,max-age=31536000,immutable";
            context.Response.ContentType = contentType;

            await context.Response.SendFileAsync(filePath);
        }
    }
}