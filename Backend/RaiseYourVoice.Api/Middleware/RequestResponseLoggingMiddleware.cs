using System.Diagnostics;
using System.Text;
using Microsoft.IO;
using Serilog;
using Serilog.Context;
using System.Text.RegularExpressions;

namespace RaiseYourVoice.Api.Middleware
{
    /// <summary>
    /// Middleware to log detailed information about HTTP requests and responses
    /// while being careful with sensitive information
    /// </summary>
    public class RequestResponseLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestResponseLoggingMiddleware> _logger;
        private readonly RecyclableMemoryStreamManager _recyclableMemoryStreamManager;
        
        // Paths that should not be logged in detail (e.g., file uploads, sensitive operations)
        private readonly string[] _excludePaths = new[] 
        { 
            "/api/auth", 
            "/api/users/password",
            "/api/webhooks",
            "/health"
        };
        
        // Headers that should be redacted in logs to protect sensitive data
        private readonly string[] _sensitiveHeaders = new[] 
        { 
            "Authorization", 
            "X-API-Key", 
            "Cookie", 
            "Set-Cookie" 
        };
        
        // Patterns to identify and redact sensitive data in request/response bodies
        private readonly List<Regex> _sensitiveDataPatterns = new List<Regex>
        {
            // Credit Card Numbers
            new Regex(@"[0-9]{13,16}", RegexOptions.Compiled),
            
            // Email Addresses
            new Regex(@"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}", RegexOptions.Compiled),
            
            // Phone Numbers
            new Regex(@"(\+?[0-9]{1,4}[\s-]?)?(\([0-9]{3}\)|[0-9]{3})[\s-]?[0-9]{3}[\s-]?[0-9]{4}", RegexOptions.Compiled),
            
            // Password fields
            new Regex(@"""(password|pwd|secret|token|apiKey|api_key|key)"":\s*""[^""]*""", RegexOptions.Compiled | RegexOptions.IgnoreCase)
        };

        public RequestResponseLoggingMiddleware(
            RequestDelegate next,
            ILogger<RequestResponseLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
            _recyclableMemoryStreamManager = new RecyclableMemoryStreamManager();
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Generate a correlation ID for tracking the request through the system
            if (string.IsNullOrEmpty(context.TraceIdentifier))
            {
                context.TraceIdentifier = Guid.NewGuid().ToString();
            }

            // Skip detailed logging for excluded paths
            bool detailedLogging = !_excludePaths.Any(path => 
                context.Request.Path.StartsWithSegments(path, StringComparison.OrdinalIgnoreCase));

            // Always add the correlation ID to the logs
            using (LogContext.PushProperty("CorrelationId", context.TraceIdentifier))
            {
                // Start timing the request
                var stopwatch = Stopwatch.StartNew();

                // Log the request 
                if (detailedLogging)
                {
                    await LogRequestAsync(context.Request);
                }
                else
                {
                    LogBasicRequestInfo(context.Request);
                }
                
                // Enable response body buffering
                var originalBodyStream = context.Response.Body;
                await using var responseBody = _recyclableMemoryStreamManager.GetStream();
                context.Response.Body = responseBody;
                
                try
                {
                    // Process the request
                    await _next(context);
                    stopwatch.Stop();
                    
                    // Reset the stream position for reading
                    context.Response.Body.Seek(0, SeekOrigin.Begin);
                    
                    // Log the response
                    if (detailedLogging)
                    {
                        await LogResponseAsync(context.Response, stopwatch.ElapsedMilliseconds);
                    }
                    else
                    {
                        LogBasicResponseInfo(context.Response, stopwatch.ElapsedMilliseconds);
                    }
                    
                    // Copy the response to the original stream
                    context.Response.Body.Seek(0, SeekOrigin.Begin);
                    await responseBody.CopyToAsync(originalBodyStream);
                }
                catch (Exception ex)
                {
                    stopwatch.Stop();
                    LogException(ex, stopwatch.ElapsedMilliseconds);
                    throw; // Re-throw to be handled by the error handling middleware
                }
                finally
                {
                    // Restore the original body stream
                    context.Response.Body = originalBodyStream;
                }
            }
        }

        private async Task LogRequestAsync(HttpRequest request)
        {
            request.EnableBuffering();

            var headers = RedactSensitiveHeaders(request.Headers);
            
            // Read the request body
            var body = await ReadRequestBodyAsync(request);

            _logger.LogInformation(
                "HTTP Request: {Method} {Scheme}://{Host}{Path}{QueryString} | Headers: {Headers} | Body: {Body}",
                request.Method,
                request.Scheme,
                request.Host,
                request.Path,
                request.QueryString,
                headers,
                RedactSensitiveData(body));

            // Reset the request body position
            request.Body.Position = 0;
        }

        private void LogBasicRequestInfo(HttpRequest request)
        {
            _logger.LogInformation(
                "HTTP Request: {Method} {Path}{QueryString}",
                request.Method,
                request.Path,
                request.QueryString);
        }

        private async Task LogResponseAsync(HttpResponse response, long elapsedMs)
        {
            var headers = RedactSensitiveHeaders(response.Headers);
            
            // Read the response body
            var body = await ReadResponseBodyAsync(response);

            _logger.LogInformation(
                "HTTP Response: Status {StatusCode} in {ElapsedMs}ms | Headers: {Headers} | Body: {Body}",
                response.StatusCode,
                elapsedMs,
                headers,
                RedactSensitiveData(body));
        }

        private void LogBasicResponseInfo(HttpResponse response, long elapsedMs)
        {
            _logger.LogInformation(
                "HTTP Response: Status {StatusCode} in {ElapsedMs}ms",
                response.StatusCode,
                elapsedMs);
        }

        private void LogException(Exception ex, long elapsedMs)
        {
            _logger.LogError(
                ex,
                "Request failed after {ElapsedMs}ms with error: {ErrorMessage}",
                elapsedMs,
                ex.Message);
        }

        private async Task<string> ReadRequestBodyAsync(HttpRequest request)
        {
            if (!request.Body.CanRead || request.ContentLength == 0)
            {
                return string.Empty;
            }

            try
            {
                using var reader = new StreamReader(
                    request.Body,
                    encoding: Encoding.UTF8,
                    detectEncodingFromByteOrderMarks: false,
                    leaveOpen: true);

                var body = await reader.ReadToEndAsync();
                return body;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error reading request body");
                return "[Error reading request body]";
            }
        }

        private async Task<string> ReadResponseBodyAsync(HttpResponse response)
        {
            if (!response.Body.CanRead)
            {
                return string.Empty;
            }

            try
            {
                response.Body.Seek(0, SeekOrigin.Begin);

                using var reader = new StreamReader(
                    response.Body,
                    encoding: Encoding.UTF8,
                    detectEncodingFromByteOrderMarks: false,
                    leaveOpen: true);

                var body = await reader.ReadToEndAsync();
                response.Body.Seek(0, SeekOrigin.Begin);
                
                return body;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error reading response body");
                return "[Error reading response body]";
            }
        }

        private string RedactSensitiveData(string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                return content;
            }

            // Don't try to process non-JSON or very large content
            if (content.Length > 100000 || 
                (!content.StartsWith("{") && !content.StartsWith("[")))
            {
                return $"[Content length: {content.Length} characters]";
            }

            var redactedContent = content;

            // Apply all regex patterns to redact sensitive data
            foreach (var pattern in _sensitiveDataPatterns)
            {
                redactedContent = pattern.Replace(redactedContent, match =>
                {
                    if (match.Value.Contains("password") || 
                        match.Value.Contains("secret") || 
                        match.Value.Contains("token") ||
                        match.Value.Contains("key"))
                    {
                        // Replace the entire password JSON property
                        return Regex.Replace(match.Value, @""":\s*""[^""]*""", @""":""*****""");
                    }
                    
                    // For other patterns like credit cards, emails, phones, mask most characters
                    return new string('*', Math.Max(0, match.Length - 4)) + 
                           match.Value.Substring(Math.Max(0, match.Length - 4));
                });
            }

            return redactedContent;
        }

        private Dictionary<string, string> RedactSensitiveHeaders(IHeaderDictionary headers)
        {
            var result = new Dictionary<string, string>();

            foreach (var header in headers)
            {
                if (_sensitiveHeaders.Contains(header.Key, StringComparer.OrdinalIgnoreCase))
                {
                    // Redact sensitive headers
                    result.Add(header.Key, "*****");
                }
                else
                {
                    result.Add(header.Key, string.Join(", ", header.Value));
                }
            }

            return result;
        }
    }
}