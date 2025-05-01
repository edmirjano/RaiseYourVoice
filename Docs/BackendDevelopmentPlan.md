# RaiseYourVoice - Backend Development Plan

## Overview
This document outlines the development plan for the backend services of the RaiseYourVoice activism platform. The backend is built with C# (.NET 8) using Clean Architecture principles, providing both REST API endpoints for the web application and gRPC services for the mobile clients.

## Technical Stack & Architecture

- **Framework**: .NET 9
- **Architecture**: Clean Architecture
- **Database**: MongoDB
- **Caching**: Redis
- **API Styles**: 
  - REST API for web applications
  - gRPC for mobile applications
- **Authentication**: JWT with refresh tokens
- **Containerization**: Docker
- **CI/CD**: GitHub Actions
- **Localization**: Server-side translation with language header support and Redis caching
- **Key Libraries & Frameworks**:
  - MediatR for CQRS pattern
  - FluentValidation for validation
  - AutoMapper for object mapping
  - Serilog for structured logging
  - Polly for resilience and transient fault handling
  - MongoDB.Driver for database operations
  - StackExchange.Redis for caching
  - Google.Protobuf and Grpc.Tools for gRPC support
  - Microsoft.AspNetCore.Authentication.JwtBearer for JWT authentication

## Project Structure

The backend follows Clean Architecture principles with the following layers:

```
Backend/
├── RaiseYourVoice.Api/
│   ├── Controllers/
│   ├── Middleware/
│   ├── Filters/
│   ├── gRPC/
│   ├── Protos/
│   ├── Program.cs
│   ├── appsettings.json
│
├── RaiseYourVoice.Application/
│   ├── Common/
│   ├── Features/
│   ├── Interfaces/
│   ├── Models/
│   ├── ApplicationServiceRegistration.cs
│
├── RaiseYourVoice.Domain/
│   ├── Common/
│   ├── Entities/
│   ├── Enums/
│   ├── Events/
│
├── RaiseYourVoice.Infrastructure/
│   ├── Persistence/
│   ├── Services/
│   ├── Security/
│   ├── InfrastructureServiceRegistration.cs
│
├── RaiseYourVoice.UnitTests/
│   ├── Application/
│   ├── Domain/
│   ├── Infrastructure/
│   ├── TestFixtures/
│
├── RaiseYourVoice.IntegrationTests/
│   ├── API/
│   ├── Application/
│   ├── Infrastructure/
│   ├── TestFixtures/
│
├── Dockerfile
├── .dockerignore
├── RaiseYourVoice.Backend.sln
```

## Implementation Plan (16 Weeks)

### Phase 1: Foundation and Core Infrastructure (Weeks 1-4)

- **Week 1**: Project setup and architecture
  - Set up project structure with Clean Architecture
  - Configure MongoDB integration
  - Implement domain entities and common classes
  - Set up dependency injection framework
  - Implement basic API controllers

- **Week 2**: Authentication and security
  - Implement JWT authentication with refresh tokens
  - Set up role-based authorization
  - Create user registration and login flows
  - Build token service and password hashing
  - Implement security middleware

- **Week 3**: Data access layer
  - Complete MongoDB repositories implementation
  - Set up MongoDB context and configuration
  - Implement pagination and filtering capabilities
  - Create data seeding mechanism
  - Build data validation pipeline

- **Week 4**: Cross-cutting concerns
  - Implement error handling middleware
  - Set up logging infrastructure with Serilog
  - Configure environment-specific settings
  - Create health check endpoints
  - Implement request/response logging
  - Create localization middleware for language header processing

### Phase 2: Core Business Logic (Weeks 5-8)

- **Week 5**: Social feed functionality
  - Implement post creation and management
  - Build comment system and threading
  - Create like functionality
  - Implement media file handling
  - Create post filtering and search
  - Add WebP/WebM conversion pipeline for uploaded media
  - Implement CDN integration for media delivery

- **Week 6**: Organizations and opportunities
  - Implement organization profile management
  - Create organization verification workflow
  - Build opportunities listing and filtering
  - Implement event management
  - Create location-based search

- **Week 7**: Success stories and profiles
  - Implement user profile management
  - Create success story publishing flow
  - Build featured content selection
  - Implement user settings and preferences
  - Create profile visibility controls

- **Week 8**: Campaign and donation management
  - Implement campaign creation and management
  - Build donation processing with payment gateway
  - Create funding goal tracking
  - Implement campaign metrics
  - Build donor management

### Phase 3: Advanced Features (Weeks 9-12)

- **Week 9**: Notification system
  - Implement push notification service
  - Build notification preferences
  - Create notification templates
  - Implement real-time notifications
  - Build notification analytics

- **Week 10**: gRPC services for mobile
  - Set up gRPC service infrastructure
  - Define protocol buffers
  - Implement mobile-specific endpoints
  - Create optimized data transfer objects
  - Build service descriptors
  - Add support for binary data streaming
  - Implement compression for media transfers

- **Week 11**: Caching and performance
  - Implement Redis caching layer
  - Create cache invalidation strategies
  - Build performance monitoring
  - Optimize database queries
  - Implement rate limiting
  - Set up Redis caching for server-side translations
  - Add CDN caching policies for media assets
  - Create optimized media delivery pipeline

- **Week 12**: Search and analytics
  - Implement full-text search capabilities
  - Create analytics aggregation pipelines
  - Build reporting endpoints
  - Implement trend detection
  - Create activity tracking

### Phase 4: Security, Scalability, and Finalization (Weeks 13-16)

- **Week 13**: Security hardening
  - Implement field-level encryption
  - Create audit logging
  - Build API path encryption
  - Implement OWASP security measures
  - Create security monitoring

- **Week 14**: API documentation and testing
  - Create OpenAPI/Swagger documentation
  - Implement integration tests
  - Build performance tests
  - Create API versioning
  - Build API client libraries

- **Week 15**: Deployment and CI/CD
  - Create Docker containerization
  - Set up CI/CD pipeline
  - Implement environment configurations
  - Build deployment scripts
  - Create monitoring dashboards

- **Week 16**: Final touches and optimization
  - Conduct performance tuning
  - Complete documentation
  - Create disaster recovery procedures
  - Build system health monitoring
  - Implement final security review

## API Design Principles

- RESTful API design for web clients
- gRPC service design for mobile clients
- Consistent error handling and status codes
- Proper versioning strategy
- Comprehensive documentation
- Authentication for all endpoints
- Rate limiting and throttling
- Payload validation
- Caching headers

## Security Implementation

### Authentication & Authorization
- JWT with secure token handling
- Refresh token rotation
- Role-based authorization
- Permission-based access control
- Secure password hashing
- OAuth integrations for social login
- Two-factor authentication option

### Data Protection
- Field-level encryption for sensitive data
  - AES-256 encryption for sensitive fields
  - Entity-level attribute markers with `[Encrypted]` attribute
  - MongoDB serialization support through custom conventions
  - Versioned encryption keys for secure key rotation
- Automatic key rotation system
  - Versioned encryption keys stored in MongoDB
  - Transparent encryption/decryption with key version awareness
  - Background service for scheduled key rotation
  - Configurable rotation intervals and policies
  - Key activation and deactivation controls
- Encryption at rest for database
- TLS for all communications
- API path encryption
- PII data handling compliance
- GDPR compliance mechanisms
- Secure audit trails

### API Security
- Rate limiting
- Request validation
- CSRF protection
- API key management
- IP restriction capabilities
- Request logging and monitoring
- Brute force protection

## MongoDB Implementation

- Document design optimized for access patterns
- Indexing strategy for performance
- Sharding preparation for future scaling
- Compound indexes for complex queries
- Change streams for real-time features
- Aggregation pipelines for analytics
- Transactions for multi-document operations

## Redis Caching Strategy

- Distributed caching layer
- Cache-aside pattern implementation
- Automatic cache invalidation
- Time-based expiration policies
- Cache warming mechanisms
- Cache entry compression
- Cache hit/miss monitoring
- Rate limiting implementation
- Translation caching with language-based keys

## Localization Implementation

- Server-side translation system with MongoDB storage
- Accept-Language header processing via middleware
- Multi-level caching strategy:
  - In-memory cache for ultra-fast access
  - Redis distributed cache with expiration policies
- MongoDB collection with compound indexes for key-language lookups
- Language fallback mechanism (to English)
- Category-based translation organization
- Centralized translation management API endpoints
- Role-based access control for translation updates

## File Storage and CDN Strategy

- **Implementation Timeline**: 
  - Week 5-6: Basic file upload/download API
  - Week 7-8: Media optimization pipeline (WebP/WebM conversion)
  - Week 9-10: CDN integration and caching policies
  - Week 11-12: Advanced features (signed URLs, access control)

- **Storage**:
  - Use a dedicated file storage system (e.g., AWS S3, Azure Blob Storage, or a self-hosted MinIO instance).
  - Store files (images, documents, videos) in a structured folder hierarchy based on entity types (e.g., `/posts/images/`, `/organizations/docs/`).
  - Generate unique file names to avoid conflicts and ensure immutability.

- **Serving Files**:
  - Serve files through a CDN (e.g., AWS CloudFront, Azure CDN, or a self-hosted Nginx-based CDN).
  - Use signed URLs for secure access to private files.
  - Cache public files (e.g., images, videos) at the CDN edge for faster delivery.

- **Integration**:
  - Add a file upload API in the backend to handle file uploads securely.
  - Validate file types and sizes during upload.
  - Store metadata (e.g., file type, size, upload date) in the database for tracking.

- **Performance**:
  - Optimize images and videos during upload (e.g., convert to WebP/WebM formats).
  - Generate multiple resolutions for responsive delivery.
  - Add metadata extraction for improved searchability.
  - Implement server-side media transformation API.

- **Security**:
  - Encrypt sensitive files at rest.
  - Use HTTPS for all file transfers.
  - Implement access control to restrict file access based on user roles.

- **Backup**:
  - Schedule regular backups of the file storage system.
  - Store backups in a separate location for disaster recovery.

- **Example Workflow**:
  1. User uploads a file via the frontend.
  2. The file is sent to the backend API.
  3. The backend validates the file and uploads it to the storage system.
  4. The backend returns a URL for accessing the file.
  5. The frontend uses the URL to display or download the file.

## Key Rotation Implementation

- Versioned encryption key management
  - Each encryption key has a version number and purpose
  - Keys stored securely in MongoDB with proper indexing
  - Multiple key versions can coexist, with only one active per purpose
  - Encrypted data includes key version metadata to allow decryption with the correct key

- Key lifecycle management
  - Keys move through creation, activation, expiration, and retirement phases
  - Grace periods allow for smooth key transitions without disrupting the application
  - Old keys remain available for decryption but are no longer used for encryption
  - Automatic periodic rotation based on configurable settings

- Scheduled background rotation service
  - Background hosted service that periodically checks for key rotation requirements
  - Automatic generation of new keys before old ones expire
  - Configurable rotation intervals based on security policies
  - Health monitoring and logging of rotation events

- Encryption format
  - Data encrypted with the format: `{keyVersion}:{encryptedBase64}` 
  - Format allows transparent key version identification during decryption
  - Backward compatibility with non-versioned encryption

- Key caching strategy
  - In-memory cache of decryption keys to minimize database lookups
  - Automatic cache invalidation on key changes
  - Thread-safe key access for high-performance scenarios

## gRPC Service Design

- Efficient serialization with Protocol Buffers
- Bidirectional streaming for real-time features
- Service definition structure
- Error handling strategy
- Authentication integration
- Performance optimization
- Mobile-specific optimizations
- Connection management
- Language preference handling for localized responses

## Monitoring and Logging

- Structured logging with Serilog
- Centralized log aggregation
- Application metrics collection
- Health check endpoints
- Performance monitoring
- Error tracking and alerting
- Audit logging for security events
- Resource utilization tracking

## Testing Strategy

- Unit testing with xUnit
- Integration testing for API endpoints
- Performance testing with benchmarks
- Load testing for scalability
- Security testing
- Mocking strategy for external dependencies
- Test data generation
- Continuous testing in CI pipeline

## Deployment Strategy

- **Containerization**: Use Docker for building images locally.
- **Orchestration**: Deploy using Kubernetes (K3s) for lightweight and efficient cluster management.
- **Secrets Management**:
  - Store sensitive data (e.g., database connection strings, API keys) in Kubernetes Secrets.
  - Use `appsettings.json` as a fallback for local development.
  - Mount secrets as environment variables or files in the container.
- **Image Deployment**:
  - Build Docker images locally.
  - Package images and deployment files into a zip archive.
  - Transfer the zip archive to the VPS using SSH (key-based authentication, no passwords).
  - Extract and deploy using bash scripts for smoother automation.
- **Domains**:
  - Use `raiseyourvoice.al` for the web application.
  - Use `api.raiseyourvoice.al` for the backend.
- **Ingress**:
  - Use Nginx Ingress Controller for routing traffic.
  - Configure Let's Encrypt for SSL certificates using Cert-Manager.
- **Pipeline**:
  - Use a shared CI/CD pipeline for both web and backend deployments.
  - Automate deployment steps with bash scripts for consistency.
- **Scaling**:
  - Configure Horizontal Pod Autoscaler (HPA) for dynamic scaling.
  - Use resource requests and limits for efficient resource allocation.
- **Backup and Recovery**:
  - Implement persistent volume backups for MongoDB and Redis.
  - Use Kubernetes CronJobs for scheduled backups.
- **K3s Maintenance**:
  - Regularly update K3s to ensure security and stability.

## Conclusion

This backend development plan provides a comprehensive roadmap for implementing the server-side components of the RaiseYourVoice platform. Following Clean Architecture principles and leveraging the latest .NET capabilities, this implementation will create a secure, scalable, and maintainable backend that meets all the specified requirements while providing optimal performance for both web and mobile clients. 

The server-side translation system with language header support enables seamless multilingual experiences across all client applications while maintaining centralized management of translations. The field-level encryption system with automatic key rotation provides strong security for sensitive data without compromising application performance or requiring manual key management processes.

With the field-level encryption, API path encryption, and key rotation features already implemented, the platform has a strong security foundation that exceeds industry standards for data protection and access control.