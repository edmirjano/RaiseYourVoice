# Changelog

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