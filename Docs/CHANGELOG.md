# Changelog

## [Interface Implementations & Backend Fixes] - 2025-05-01 19:30

### Fixed
- Completed interface implementations for repository classes:
  - Added missing methods in `EncryptionKeyRepository` for `IEncryptionKeyRepository`:
    - Implemented `GetActiveKeyAsync`, `ActivateKeyAsync`, `GetHighestVersionAsync` and more
    - Added proper method signatures with correct parameter ordering
  - Added missing methods in `RefreshTokenRepository` for `IRefreshTokenRepository`:
    - Added `FindValidTokenAsync` and `MarkTokenAsRevokedAsync` methods
    - Fixed `ExpiresAt` property reference in token expiration check
- Fixed the `KeyRotationBackgroundService` class:
  - Updated to properly inherit from `BackgroundService`
  - Implemented required methods: `ExecuteAsync`, `StopAsync`, and `Dispose`
  - Added configurable rotation check interval
- Updated rate limiting configuration in Program.cs:
  - Changed to modern approach with `AddFixedWindowLimiter`
  - Fixed QueueProcessingOrder and PermitLimit settings
- Implemented comprehensive `IPushNotificationService` interface:
  - Added new notification methods in the interface definition
  - Fixed references in WebhooksController and service classes
- Fixed Stripe implementation in StripePaymentGateway:
  - Removed reference to obsolete `Charges` API
  - Updated to use modern Stripe payment intents API
- Updated KeyRotationOptions in InfrastructureServiceRegistration.cs:
  - Added proper configuration binding for rotation parameters
  - Added support for RotationCheckIntervalHours setting

### Dependencies
To complete implementation, the following NuGet packages are needed:
```
Microsoft.AspNetCore.RateLimiting
Microsoft.Extensions.Hosting.Abstractions
AspNetCore.HealthChecks.MongoDb
AspNetCore.HealthChecks.Redis
Swashbuckle.AspNetCore
Stripe.net
```

## [Backend Bug Fixes & Dependencies] - 2025-05-01 18:45

### Fixed
- Implemented missing methods in `PushNotificationService`:
  - Added `SendNotificationAsync` method to handle direct user notifications
  - Added `SendAdminNotificationAsync` method for admin-specific notifications
  - Added `SendCampaignUpdateNotificationAsync` method for campaign donors
  - Fixed references to these methods across numerous controllers and services
- Fixed Path handling in `ApiPathEncryptionMiddleware`:
  - Properly handled nullable `PathString?` with `HasValue` check before accessing the value
  - Correctly used the `Value` property for non-null PathString instances
- Updated repositories to use correct property references:
  - Refactored `NotificationRepository` to use `TargetAudience.UserIds` instead of non-existent `RecipientIds` field
  - Fixed `DonationRepository` to reference `UserId` and `PaymentStatus` instead of `DonorId` and `Status`
  - Corrected MongoDb index definitions to match actual entity properties
- Fixed inheritance issues in repository classes:
  - Corrected `RefreshTokenRepository` to properly inherit from `MongoGenericRepository<RefreshToken>`
  - Fixed `EncryptionKeyRepository` constructor to match base class requirements
- Addressed configuration binding issues in `InfrastructureServiceRegistration.cs`:
  - Changed from passing `IConfigurationSection` to using lambda-based configuration
  - Fixed options pattern implementation for MongoDB, Stripe, and KeyRotation settings
- Added missing `Cancelled` enum value to `PaymentStatus` in `PaymentEnums.cs`
- Updated `Program.cs` to correctly implement and use:
  - Swagger for API documentation
  - Rate limiting with proper options
  - Health checks with MongoDB integration

### Added
- Implemented comprehensive `PushNotificationService` with:
  - Device token management for push notifications
  - Role-based notification sending
  - Support for various notification targets (single user, multiple users, roles)
  - Proper error handling and logging

### Dependencies
- Need to install the following NuGet packages to complete the fixes:
  - Swashbuckle.AspNetCore for Swagger support
  - AspNetCore.HealthChecks.MongoDb for MongoDB health checks
  - AspNetCore.HealthChecks.Redis for Redis health checks
  - Microsoft.AspNetCore.RateLimiting for API rate limiting

## [Bug Fixes & Code Completion] - 2025-05-01 17:30

### Fixed
- Fixed missing return statement in `CampaignService.AddCampaignMilestoneAsync` method:
  - Added `return false` statement to the catch block
  - Ensured all code paths return a value
  - Prevented potential runtime errors when adding campaign milestones

## [Code Refactoring & Architecture Improvements] - 2025-05-01 16:45

### Improvements
- Consolidated duplicate `PaymentRequest` classes to eliminate redundancy:
  - Removed duplicate `PaymentRequest` class from `IPaymentGateway.cs`
  - Updated the single `PaymentRequest` class in Models/Requests with all necessary properties
  - Modified `StripePaymentGateway` to work with the updated class structure
  - Updated `DonationsController` to use the consolidated model
- Enhanced code consistency and maintainability through class consolidation

## [Security Enhancements & Architecture Improvements] - 2025-05-01 14:30

### Added
- Created dedicated `IRefreshTokenRepository` interface extending the generic repository pattern
- Implemented concrete `RefreshTokenRepository` class with specialized methods for token management
- Updated `TokenService` to use the repository abstraction instead of direct MongoDB access
- Registered the new repository in the dependency injection container
- Enhanced `Program.cs` to support multiple configuration sources:
  - Environment variables with RYV_ prefix (for Kubernetes)
  - User secrets for local development
  - Better error handling for missing configuration
- Created detailed documentation for secrets management in `k8s/secrets-management.md`
- Implemented JWT signing key rotation:
  - Registered `JwtKeyManager` as a singleton service
  - Updated `TokenService` to use the key manager for token generation and validation
  - Modified JWT authentication setup to use all available signing keys
  - Enhanced the key rotation background service to also rotate JWT keys
  - Updated the token validation parameters to dynamically refresh with latest keys
  - Added configuration option for JWT key rotation interval

### Improvements
- Improved separation of concerns in the `TokenService` by removing direct MongoDB dependencies
- Enhanced testability of the `TokenService` through proper abstraction
- Better error handling for missing configuration with descriptive exception messages
- More consistent repository pattern implementation across the application
- Improved security through automatic JWT key rotation
- Added support for multiple active signing keys for seamless token validation during key rotations
- Enhanced security logging with JWT-specific rotation events

### To Do
- Complete missing interface implementations
- Create proper automated tests for the security features
- Complete gRPC services for mobile app communication
- Finalize web application features
- Set up complete deployment pipeline

## Security Improvements

### Completed
- ✅ Field-level encryption for sensitive data using AES-256
- ✅ Versioned encryption keys for secure key rotation
- ✅ MongoDB repository abstraction for improved separation of concerns
- ✅ Environment-based configuration for secrets management
- ✅ Kubernetes-compatible secrets strategy with documentation
- ✅ JWT signing key rotation implementation

### Pending
- ⏳ Automated tests for security features
- ⏳ Complete CI/CD pipeline setup