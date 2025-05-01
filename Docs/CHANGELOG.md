# Changelog

## [Null Reference Exception Fixes] - 2025-05-01 23:45

### Fixed
- Fixed potential null reference exceptions across multiple controllers:
  - Added null conditional operator (`?.`) and null coalescing with exception throwing in `User.Identity?.Name` calls
  - Updated `AuthController`, `CampaignsController`, `CommentsController`, `DonationsController`, `NotificationsController`, and `PostsController`
  - Replaced direct property access with null-safe patterns to prevent runtime exceptions
  - Added explicit error messages in the UnauthorizedAccessException when authentication fails
- Fixed nullable parameter handling in `ApiPathEncryptionMiddleware`:
  - Added null check for `encryptedPath` parameter before processing
  - Improved error handling for null path segments
- Added required modifier to properties in `ApiPathMapping` class:
  - Added `required` keyword to `EncryptedPath` and `RealPath` properties
  - Ensures proper initialization of mapping objects
- Fixed nullable type handling in controller parameters:
  - Added nullable annotation (`?`) to `object` parameter in `SuccessWithLocalizedMessage`

### Improved
- Enhanced error messages when authentication fails with descriptive UnauthorizedAccessException
- Strengthened null safety across the authentication flow
- Improved code robustness in middleware and controller components
- Reduced risk of runtime exceptions from null references

## [Build Warning Fixes] - 2025-05-01 23:30

### Fixed
- Fixed non-nullable reference type warnings across the codebase:
  - Added `required` modifier to properties in `AuthResponse.cs`, `SubscriptionRequest.cs`, and other model classes
  - Made navigation properties nullable with `?` suffix in `Donation.cs` entity class
  - Fixed `MongoRepository<T>` to properly handle nullable return types in `GetByIdAsync` method
  - Initialized non-nullable fields properly in `JwtKeyManager` and `MongoDbSettings`
- Fixed hidden member warnings in repository classes:
  - Added `new` keyword to `_collection` field in `EncryptionKeyRepository` and `RefreshTokenRepository`
- Fixed `ASP0019` warnings in `SecurityHeadersMiddleware`:
  - Changed from `Add` to using indexer assignment for response headers
- Fixed async methods without await operators in `WebhooksController`:
  - Removed `async` keyword from methods that don't use await
  - Added `Task.CompletedTask` return values for non-async methods
- Fixed interface implementation warnings:
  - Updated `LocalizationService.SetLocalizedStringAsync` parameter types to match interface
  - Fixed return type in `MongoRepository<T>.GetByIdAsync` to match `IGenericRepository<T>` interface
- Fixed potential null reference exceptions in controllers:
  - Added null checks for `User.Identity?.Name` in controllers before using it
  - Added proper null validation in authentication-related code

### Improved
- Enhanced overall code quality and reduced warning count from 93 to 0
- Added proper logging field to `MongoRepository<T>` for error handling
- Improved nullable reference type handling throughout the codebase
- Fixed parameter type mismatches between interfaces and implementations

## [Build Problem Fixes] - 2025-05-01 23:00

### Fixed
- Fixed build errors in WebhooksController.cs:
  - Added missing Stripe.net package (version 48.1.0) to resolve 'Events' reference issues
  - Updated code to use the proper namespace with `Stripe.Events` instead of just `Events`
  - Changed `ChargeId` property references to `PaymentIntentId` to align with latest Stripe API
- Fixed rate limiting configuration issues in Program.cs:
  - Updated to use built-in ASP.NET Core rate limiting APIs 
  - Added proper namespace references to Microsoft.AspNetCore.RateLimiting
  - Fixed configuration of QueueProcessingOrder and other rate limiter options
- Fixed MongoDB repository implementation issues:
  - Added missing `Id` property to TeamMember class in Organization entity
  - Fixed DateTime null comparison in EncryptionKeyRepository by using `Filter.Exists()` instead of direct null comparison
  - Resolved null argument issue when checking for expired encryption keys

### Improved
- Enhanced code maintainability through proper namespace usage
- Updated Stripe integration to use the latest API conventions
- Improved MongoDB query patterns for better performance and stability
- Ensured consistent repository patterns across the codebase

## [Repository Structure Fix & MongoDB Integration] - 2025-05-01 21:15

### Fixed
- Fixed the repository class structure issue:
  - Changed `MongoGenericRepository<T>` references to correct `MongoRepository<T>` in all repository classes
  - Updated access modifiers in `MongoRepository<T>` to make `_collection` protected instead of private
  - Fixed inheritance in `EncryptionKeyRepository`, `OrganizationRepository`, and `RefreshTokenRepository`
- Updated `InfrastructureServiceRegistration.cs` to register the correct repository implementations
- Fixed rate limiting configuration in Program.cs:
  - Removed invalid reference to `Microsoft.AspNetCore.RateLimiting` version 9.0.0 (doesn't exist)
  - Updated to use the built-in ASP.NET Core rate limiting functionality 
- Updated `StripePaymentGateway` implementation to match the current `PaymentRequest` model
- Added automatic ID generation to `BaseEntity` using MongoDB.Bson's ObjectId.GenerateNewId()

### Dependencies
- Added MongoDB.Bson reference to the Domain project to support ObjectId generation
- Added MongoDB.Driver reference to the Infrastructure project for MongoDB repository implementation

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