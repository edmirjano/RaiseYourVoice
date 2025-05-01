using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RaiseYourVoice.Application.Interfaces;
using RaiseYourVoice.Infrastructure.Security;

namespace RaiseYourVoice.Infrastructure.Services
{
    /// <summary>
    /// Background service that periodically checks and rotates encryption keys
    /// </summary>
    public class KeyRotationBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<KeyRotationBackgroundService> _logger;
        private readonly KeyRotationOptions _options;
        private Timer _timer;

        public KeyRotationBackgroundService(
            IServiceProvider serviceProvider,
            IOptions<KeyRotationOptions> options,
            ILogger<KeyRotationBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _options = options.Value;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Key Rotation Service is starting");

            // Don't start the timer if disabled
            if (!_options.Enabled)
            {
                _logger.LogInformation("Key Rotation Service is disabled");
                return Task.CompletedTask;
            }

            // Start immediately, then run on a schedule
            _timer = new Timer(DoWork, null, TimeSpan.Zero, 
                TimeSpan.FromHours(_options.CheckIntervalHours));

            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            _logger.LogInformation("Key Rotation Service is checking for key rotation needs");
            
            // Run asynchronously but don't await (we're in a timer callback)
            Task.Run(async () =>
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var encryptionService = scope.ServiceProvider.GetRequiredService<IEncryptionService>();
                    var encryptionLogger = scope.ServiceProvider.GetService<EncryptionLoggingService>();
                    
                    // Log the start of key rotation check
                    encryptionLogger?.LogSecurityEvent(
                        "KeyRotationCheckStarted",
                        "Scheduled key rotation check is being performed",
                        severity: 0);
                    
                    // Perform key rotation check
                    bool anyRotated = await encryptionService.PerformScheduledKeyRotationAsync();
                    
                    // Log the result of key rotation check
                    if (anyRotated)
                    {
                        encryptionLogger?.LogSecurityEvent(
                            "KeyRotationPerformed",
                            "Key rotation performed successfully",
                            severity: 0);
                        
                        _logger.LogInformation("Key rotation performed successfully");
                    }
                    else
                    {
                        encryptionLogger?.LogSecurityEvent(
                            "NoKeyRotationNeeded",
                            "No key rotation needed at this time",
                            severity: 0);
                        
                        _logger.LogInformation("No key rotation needed at this time");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred during key rotation check");
                    
                    // Try to log the error with the encryption logger if available
                    try
                    {
                        using var errorScope = _serviceProvider.CreateScope();
                        var encryptionLogger = errorScope.ServiceProvider.GetService<EncryptionLoggingService>();
                        
                        encryptionLogger?.LogSecurityEvent(
                            "KeyRotationError",
                            $"Error during key rotation check: {ex.Message}",
                            severity: 2);
                    }
                    catch
                    {
                        // Ignore errors in error logging to prevent cascading failures
                    }
                }
            });
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Key Rotation Service is stopping");
            
            _timer?.Change(Timeout.Infinite, 0);
            
            return base.StopAsync(cancellationToken);
        }

        public override void Dispose()
        {
            _timer?.Dispose();
            base.Dispose();
        }
    }

    /// <summary>
    /// Configuration options for key rotation service
    /// </summary>
    public class KeyRotationOptions
    {
        /// <summary>
        /// Whether key rotation is enabled
        /// </summary>
        public bool Enabled { get; set; } = true;
        
        /// <summary>
        /// How often to check for key rotation needs (in hours)
        /// </summary>
        public int CheckIntervalHours { get; set; } = 12;
    }
}