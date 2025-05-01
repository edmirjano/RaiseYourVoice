using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace RaiseYourVoice.Infrastructure.Security
{
    /// <summary>
    /// Service for logging encryption-related operations and events
    /// </summary>
    public class EncryptionLoggingService
    {
        private readonly ILogger<EncryptionLoggingService> _logger;

        public EncryptionLoggingService(ILogger<EncryptionLoggingService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Log an encryption operation
        /// </summary>
        public void LogEncryption(string operation, string purpose, int keyVersion, bool success, TimeSpan duration, string errorMessage = null)
        {
            if (success)
            {
                _logger.LogInformation(
                    "Encryption operation: {Operation} using key {KeyVersion} for {Purpose} completed in {DurationMs}ms",
                    operation, keyVersion, purpose, duration.TotalMilliseconds);
            }
            else
            {
                _logger.LogError(
                    "Encryption operation: {Operation} using key {KeyVersion} for {Purpose} failed after {DurationMs}ms: {ErrorMessage}",
                    operation, keyVersion, purpose, duration.TotalMilliseconds, errorMessage);
            }
        }

        /// <summary>
        /// Log a key rotation event
        /// </summary>
        public void LogKeyRotation(string purpose, int oldVersion, int newVersion)
        {
            _logger.LogInformation(
                "Encryption key rotated for {Purpose}: old version {OldVersion}, new version {NewVersion}",
                purpose, oldVersion, newVersion);
        }

        /// <summary>
        /// Log a key generation event
        /// </summary>
        public void LogKeyGeneration(string purpose, int version, DateTime activationDate, DateTime expiryDate)
        {
            _logger.LogInformation(
                "New encryption key generated for {Purpose}: version {Version}, activates on {ActivationDate}, expires on {ExpiryDate}",
                purpose, version, activationDate, expiryDate);
        }

        /// <summary>
        /// Log key activation
        /// </summary>
        public void LogKeyActivation(string purpose, int version)
        {
            _logger.LogInformation(
                "Encryption key activated for {Purpose}: version {Version}",
                purpose, version);
        }

        /// <summary>
        /// Track and log the performance of an encryption or decryption operation
        /// </summary>
        public async Task<T> TrackOperationAsync<T>(string operation, string purpose, int keyVersion, Func<Task<T>> action)
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                T result = await action();
                stopwatch.Stop();
                
                LogEncryption(operation, purpose, keyVersion, true, stopwatch.Elapsed);
                
                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                LogEncryption(operation, purpose, keyVersion, false, stopwatch.Elapsed, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Track and log the performance of an encryption or decryption operation (synchronous version)
        /// </summary>
        public T TrackOperation<T>(string operation, string purpose, int keyVersion, Func<T> action)
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                T result = action();
                stopwatch.Stop();
                
                LogEncryption(operation, purpose, keyVersion, true, stopwatch.Elapsed);
                
                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                LogEncryption(operation, purpose, keyVersion, false, stopwatch.Elapsed, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Log a security event related to encryption
        /// </summary>
        public void LogSecurityEvent(string eventType, string details, int severity = 0)
        {
            var severityLabel = severity switch
            {
                0 => "Info",
                1 => "Warning",
                2 => "Critical",
                _ => "Unknown"
            };

            _logger.LogInformation(
                "Security event: {EventType} | Severity: {Severity} | {Details}",
                eventType, severityLabel, details);
        }
    }
}