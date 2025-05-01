using RaiseYourVoice.Application.Interfaces;

namespace RaiseYourVoice.Api.Security
{
    /// <summary>
    /// Helper class for working with encrypted API paths
    /// </summary>
    public class ApiPathEncryptionHelper
    {
        private readonly IEncryptionService _encryptionService;
        private readonly string _pathPrefix;

        public ApiPathEncryptionHelper(IEncryptionService encryptionService, string pathPrefix = "e-")
        {
            _encryptionService = encryptionService ?? throw new ArgumentNullException(nameof(encryptionService));
            _pathPrefix = pathPrefix;
        }

        /// <summary>
        /// Encrypts an API path segment for secure routing
        /// </summary>
        /// <param name="pathSegment">The path segment to encrypt</param>
        /// <returns>The encrypted path segment</returns>
        public string EncryptPathSegment(string pathSegment)
        {
            if (string.IsNullOrEmpty(pathSegment))
            {
                return pathSegment;
            }

            try
            {
                // Encrypt the path segment
                var encrypted = _encryptionService.Encrypt(pathSegment);
                
                // Ensure base64 is URL-safe (replace + with -, / with _, and remove padding =)
                encrypted = encrypted.Replace('+', '-').Replace('/', '_').TrimEnd('=');
                
                // Prefix the encrypted segment
                return _pathPrefix + encrypted;
            }
            catch (Exception ex)
            {
                // Log the error (should use proper logging in production)
                Console.WriteLine($"Error encrypting path: {ex.Message}");
                
                // Return the original segment if encryption fails
                return pathSegment;
            }
        }

        /// <summary>
        /// Encrypts an entire API path for secure routing
        /// </summary>
        /// <param name="path">The API path to encrypt</param>
        /// <returns>The path with encrypted segments</returns>
        public string EncryptPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return path;
            }

            // Split the path into segments
            var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            
            // Encrypt each segment
            for (int i = 0; i < segments.Length; i++)
            {
                // Skip segments that look like query parameters
                if (!segments[i].Contains('='))
                {
                    segments[i] = EncryptPathSegment(segments[i]);
                }
            }
            
            // Reconstruct the path
            return "/" + string.Join('/', segments);
        }

        /// <summary>
        /// Generates an encrypted URL for an API endpoint
        /// </summary>
        /// <param name="baseUrl">The base URL of the API</param>
        /// <param name="path">The API path to encrypt</param>
        /// <returns>The full encrypted URL</returns>
        public string GenerateEncryptedUrl(string baseUrl, string path)
        {
            if (string.IsNullOrEmpty(baseUrl))
            {
                throw new ArgumentNullException(nameof(baseUrl));
            }

            if (string.IsNullOrEmpty(path))
            {
                return baseUrl;
            }

            // Ensure the base URL doesn't end with a slash
            baseUrl = baseUrl.TrimEnd('/');
            
            // Ensure the path starts with a slash
            if (!path.StartsWith('/'))
            {
                path = "/" + path;
            }
            
            // Encrypt the path and combine with the base URL
            return baseUrl + EncryptPath(path);
        }
    }
}