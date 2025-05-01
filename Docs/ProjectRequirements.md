# RaiseYourVoice (RYV) Platform - Project Requirements

## Core Concept
* Social platform connecting people with activism opportunities
* Focus on engagement, accessibility, and community building
* Registration optional for basic browsing, required for posting

## App Structure

### 1. Social Feed Tab
* Posts about social movements and activism
* Like/thumbs-up functionality
* Comment system
* Registered users can create posts

### 2. Opportunities Tab
* Listings for activism events
* Youth funding opportunities
* MUN (Model United Nations) events
* Mobility programs

### 3. Success Stories Tab
* Featured activist profiles
* NGO spotlights
* "Activist of the Month/Week/Day" feature

### 4. Profile Settings Tab
* User information (name, email, age, gender)
* Preference management
* Account settings

## User Roles
* **Admin**: Full system access, role assignment capabilities
* **Moderator**:
  * Content moderation, own post creation
  * Send push notifications to users (segmented or broadcast)
  * Verify organization accounts
  * Manage featured content
* **Activist**: Create/edit own posts, engage with others' content
* **Organization**: Special account type for NGOs and activist groups

## Technical Stack

### Mobile
* Native iOS app using Xcode project structure (xcodeproj)
  * Store tokens in iOS Keychain for secure storage
* Native Android app
  * Store tokens in Android Keystore/SharedPreferences with encryption
* Handle deeplinks for both platforms to support direct navigation

### Web
* Next.js landing page and admin dashboard (potentially single project)
* Implement PWA support for web applications

### Backend
* Latest C# (.NET 8/9) with clean architecture
* Database: MongoDB
* Caching: Redis for performance optimization

### Communication
* gRPC with encryption for mobile apps
* REST API for web applications
* Enhanced Security: Encrypted API paths for additional security
* Data Integration: Use direct backend data without mocks for authentic user experience

### Deployment
* Docker Compose on fresh Ubuntu VPS
* CI/CD pipeline for automated deployments

### Security & Authentication
* JWT with refresh tokens, Apple/Google Sign-In
* HTTPS, API rate limiting, data encryption at rest and in transit, OWASP compliance
* Domain: 'raiseyourvoice.al'

## Data Models

### Post Entity
* Id, Title, Content, MediaUrls, PostType (activism/opportunity/success)
* AuthorId, CreatedAt, UpdatedAt, LikeCount, CommentCount
* Tags, Location, EventDate (for opportunities)
* Status (published/draft/removed)

### User Entity
* Id, Name, Email, PasswordHash, Role
* ProfilePicture, Bio, JoinDate, LastLogin
* PreferredLanguage, NotificationSettings
* ExternalAuthProviders (Google/Apple)
* DeviceTokens (for push notifications)

### Comment Entity
* Id, PostId, AuthorId, Content
* CreatedAt, UpdatedAt, LikeCount
* ParentCommentId (for threaded replies)

### Organization Entity
* Id, Name, Description, LogoUrl
* Website, SocialMediaLinks, ContactInfo
* VerificationStatus, VerifiedBy, VerificationDate
* FoundingDate, MissionStatement, VisionStatement
* HeadquartersLocation, OperatingRegions
* OrganizationType (NGO, Non-profit, Advocacy group, etc.)
* RegistrationNumber, TaxIdentificationNumber
* LegalDocuments, VerificationDocuments
* TeamMembers (leadership structure)
* PastProjects, ImpactMetrics

### Notification Entity
* Id, Title, Content, Type
* SentBy, SentAt, ExpiresAt
* TargetAudience (segmentation data)
* DeliveryStatus, ReadStatus

## Additional Features and Systems

### Search System
* Full-text search across all content
* Advanced filters (by topic, location, date)
* Relevance ranking algorithms

### Analytics & Reporting
* Admin dashboards with KPIs
* User engagement metrics
* Content performance tracking
* Impact measurement for campaigns

### Content Moderation Tools
* AI-assisted content screening
* User reporting system
* Moderation queues
* Banned keyword detection

### Social Sharing
* Integration with major social platforms
* Custom share cards/previews
* Attribution tracking

### Accessibility Features
* WCAG 2.1 AA compliance
* Screen reader optimization
* Keyboard navigation
* Color contrast options

### Email Notification System
* Transactional emails for user registration
* Notification emails for system events
* Custom email templates for different event types
* Email delivery tracking and analytics

### Pagination
* Implement server-side pagination for all entity collections
* Consistent pagination interfaces across frontend and backend
* Support for cursor-based and offset-based pagination options

## Design Requirements
* iOS-inspired native design system
* Primary color: iOS black (#212124)
* Tailwind CSS for web components (no custom CSS)
* Apple-style animations throughout
* Single-page scrolling landing page (Apple website style)
* SEO optimization for landing page
* Custom icon and animated splash screen related to activism

## Multilingual Support
* English and Albanian languages
* Backend-driven translations with language header support
  * Client applications send preferred language in request headers
  * Backend returns all string messages pre-translated in the requested language
  * Centralized translation management on the server side
* Localization infrastructure for easy addition of languages
* Context-aware translations
* Use Redis for distributed caching across services
* Content delivery optimization (CDN integration)
* Rate limiting and request throttling
* Encrypted API Paths: Implement URL encryption for API endpoints to prevent endpoint enumeration
* Direct Data Integration: No mock data - all components connect directly to backend services
* Implement database encryption at highest security level:
  * Field-level encryption for all PII and sensitive data
  * AES-256 encryption for data at rest
  * TLS 1.3 for data in transit
  * Secure key management with HSM (Hardware Security Module)
  * Regular encryption key rotation
  * Separate encryption keys for different data categories
  * Implement envelope encryption pattern
  * Store encryption keys outside the database in a secure vault
  * Encrypt data before storing and decrypt after retrieving from database
* DDoS protection mechanisms
* GDPR and data privacy compliance
* Implement proper logging and monitoring (without logging sensitive data)
* Responsive design for all screen sizes and devices
* Secure CI/CD pipeline with code scanning for vulnerabilities
* Regular penetration testing and security audits

## Deployment Architecture
* Fresh Ubuntu VPS setup with hardened security:
  * Firewall configuration (UFW)
  * SSH key-based authentication only
  * Fail2ban for intrusion prevention
  * Regular security updates
* Docker-based deployment:
  * Application containers
  * Database container with persistence
  * Redis container for caching
  * Nginx container for reverse proxy
  * Let's Encrypt container for SSL
* Database backups with encryption
* Monitoring stack (Prometheus/Grafana)
* Log aggregation system
* CI/CD pipeline for automated testing and deployment

## Disaster Recovery & Business Continuity
* Automated daily backups with encryption
* Off-site backup storage
* Documented recovery procedures
* Redundancy for critical components
* High availability configuration for production

## API Documentation
* OpenAPI/Swagger specification
* Interactive API documentation
* API version management
* Developer portal for partners/integrations
* Secure API Documentation: Access controls for sensitive endpoint documentation

## Enhanced Security Considerations
* API Path Encryption: Implementation of obfuscated API paths using path encryption or token-based path resolution
* Direct Data Strategy: Architecture design to support direct data consumption from backend services with proper caching and performance optimization
* Mobile Security: Secure storage of authentication tokens in iOS Keychain and Android secure storage
* Deeplink Handling: Secure implementation of deeplinks with proper validation in mobile apps
* Data Encryption: End-to-end encryption for sensitive data with proper key management