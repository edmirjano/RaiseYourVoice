using Microsoft.Extensions.Options;
using RaiseYourVoice.Application.Interfaces;

namespace RaiseYourVoice.Api.Middleware
{
    /// <summary>
    /// Middleware for handling encrypted API paths, providing enhanced security
    /// by obfuscating the actual API endpoints from potential attackers.
    /// </summary>
    public class ApiPathEncryptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IEncryptionService _encryptionService;
        private readonly ApiPathEncryptionOptions _options;
        private readonly Dictionary<string, string> _pathMappings = new Dictionary<string, string>();

        public ApiPathEncryptionMiddleware(
            RequestDelegate next,
            IEncryptionService encryptionService,
            IOptions<ApiPathEncryptionOptions> options)
        {
            _next = next;
            _encryptionService = encryptionService;
            _options = options.Value;
            
            // Initialize path mappings from configuration if any
            if (_options.PathMappings != null)
            {
                foreach (var mapping in _options.PathMappings)
                {
                    _pathMappings[mapping.EncryptedPath.ToLowerInvariant()] = mapping.RealPath;
                }
            }
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Skip middleware if it's disabled or the request doesn't match the base path
            if (!_options.Enabled || !context.Request.Path.StartsWithSegments(_options.BasePath, StringComparison.OrdinalIgnoreCase))
            {
                await _next(context);
                return;
            }

            var originalPath = context.Request.Path.Value;
            
            // Try to decode the path
            var decodedPath = DecodeApiPath(originalPath);
            
            if (decodedPath != null)
            {
                // Store original path for logging/debugging
                context.Items["OriginalEncryptedPath"] = originalPath;
                
                // Rewrite the request path
                context.Request.Path = decodedPath;
            }

            await _next(context);
        }

        /// <summary>
        /// Decodes an encrypted API path
        /// </summary>
        private PathString? DecodeApiPath(string encryptedPath)
        {
            // First check if this is in our static mapping
            var normalizedPath = encryptedPath.ToLowerInvariant();
            if (_pathMappings.TryGetValue(normalizedPath, out var mappedPath))
            {
                return new PathString(mappedPath);
            }

            // If not in static mapping but path has our encryption prefix
            if (IsPathEncrypted(encryptedPath))
            {
                try
                {
                    // Extract the encrypted portion (after the prefix)
                    var segments = encryptedPath.Split('/');
                    
                    // Look for the segment that contains our encrypted part
                    for (int i = 0; i < segments.Length; i++)
                    {
                        if (segments[i].StartsWith(_options.PathPrefix, StringComparison.OrdinalIgnoreCase))
                        {
                            // Extract the base64 part
                            var base64Part = segments[i].Substring(_options.PathPrefix.Length);
                            
                            // Replace URL-safe characters back
                            base64Part = base64Part.Replace('-', '+').Replace('_', '/');
                            
                            // Add padding if needed
                            switch (base64Part.Length % 4)
                            {
                                case 2: base64Part += "=="; break;
                                case 3: base64Part += "="; break;
                            }
                            
                            try
                            {
                                // Decrypt the segment
                                var decodedPathPart = _encryptionService.Decrypt(base64Part);
                                
                                // Replace the encrypted segment with the decoded one
                                segments[i] = decodedPathPart;
                                
                                // Reconstruct the full path
                                return new PathString("/" + string.Join("/", segments.Skip(1)));
                            }
                            catch
                            {
                                // If decryption fails, continue to the next segment
                                continue;
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    // If any exception occurs, return null to leave the path unchanged
                    return null;
                }
            }

            // If we reach here, we couldn't decode the path
            return null;
        }

        /// <summary>
        /// Determines if a path appears to be encrypted
        /// </summary>
        private bool IsPathEncrypted(string path)
        {
            return path.Contains(_options.PathPrefix, StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <summary>
    /// Configuration options for the API path encryption middleware
    /// </summary>
    public class ApiPathEncryptionOptions
    {
        /// <summary>
        /// Whether API path encryption is enabled
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Base path that should be subject to encryption/decryption
        /// </summary>
        public string BasePath { get; set; } = "/api/v1";

        /// <summary>
        /// Prefix used to identify encrypted path segments
        /// </summary>
        public string PathPrefix { get; set; } = "e-";

        /// <summary>
        /// Static mappings for encrypted paths to real paths
        /// </summary>
        public List<ApiPathMapping> PathMappings { get; set; }
    }

    /// <summary>
    /// Mapping from encrypted path to real path
    /// </summary>
    public class ApiPathMapping
    {
        public string EncryptedPath { get; set; }
        public string RealPath { get; set; }
    }
}