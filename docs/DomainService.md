# Domain Service in Domain-Driven Design

> **Status**: ✅ Complete  
> **Branch**: `domain-service-in-ef`  
> **Last Updated**: 2025

## 📑 Table of Contents

- [What is a Domain Service?](#-what-is-a-domain-service)
- [When to Use Domain Services](#-when-to-use-domain-services)
- [Domain Service vs Application Service](#-domain-service-vs-application-service)
- [Designing Domain Services](#️-designing-domain-services)
- [Example: Email Uniqueness Checker](#-example-email-uniqueness-checker)
- [Example: Client Transfer Service](#-example-client-transfer-service)
- [Dependency Injection Setup](#-dependency-injection-setup)
- [Testing Domain Services](#-testing-domain-services)
- [Best Practices Summary](#-best-practices-summary)
- [Related Patterns](#-related-patterns)
- [Further Reading](#-further-reading)

---

## 📖 What is a Domain Service?

A **Domain Service** is a stateless operation that contains domain logic which doesn't naturally fit within an entity or value object. It typically operates on multiple aggregates or represents a significant business process.

Domain Services are a fundamental building block in Domain-Driven Design (DDD) that help keep your domain model clean and focused. They encapsulate business logic that:

- Spans multiple entities or aggregates
- Doesn't belong to a single entity's responsibility
- Represents a significant domain operation or business process

### Key Characteristics

| Characteristic | Description |
|---------------|-------------|
| **Stateless** | Domain services don't hold any state between operations |
| **Domain Logic** | Contains pure business rules, not infrastructure concerns |
| **Multi-Entity Operations** | Works with multiple domain objects to accomplish a task |
| **Business Concept** | Named after domain concepts, not technical terms |
| **Domain Layer** | Lives alongside entities and value objects in the domain |

---

## 🎯 When to Use Domain Services

Use a Domain Service when:

### ✅ Good Use Cases

1. **The operation spans multiple aggregates**
   - Transferring money between two accounts
   - Calculating order totals with client-specific discounts

2. **The logic doesn't naturally belong to any entity**
   - Email uniqueness validation across all clients
   - Complex business rule validation

3. **The operation represents a significant domain concept**
   - Payment processing
   - Order fulfillment

4. **You need to avoid bloating an entity**
   - When adding logic would make an entity too large or unfocused

### ❌ When NOT to Use Domain Services

- **Simple CRUD operations** → Use repositories
- **Infrastructure concerns** (sending emails, logging) → Use Application Services
- **Logic that belongs to a single entity** → Put it in the entity itself
- **Orchestration of multiple services** → Use Application Services

---

## 🔄 Domain Service vs Application Service

Understanding the difference is crucial:

| Aspect | Domain Service | Application Service |
|--------|---------------|---------------------|
| **Purpose** | Business logic | Use case orchestration |
| **State** | Stateless | Stateless |
| **Dependencies** | Domain objects only | Repositories, external services |
| **Location** | Domain Layer | Application Layer |
| **Knowledge** | Business rules | Application workflow |
| **Example** | `ClientRegistrationService` | `RegisterClientUseCase` |

### Layered Architecture Position

```
┌─────────────────────────────────────────┐
│           Presentation Layer            │
│         (Controllers, Views)            │
├─────────────────────────────────────────┤
│           Application Layer             │
│    (Application Services, Use Cases)    │
│         ↓ orchestrates ↓                │
├─────────────────────────────────────────┤
│             Domain Layer                │
│  ┌─────────────────────────────────┐   │
│  │   Entities    │  Value Objects  │   │
│  ├───────────────┴─────────────────┤   │
│  │       Domain Services           │   │
│  │  (Interfaces + Pure Impls)      │   │
│  └─────────────────────────────────┘   │
├─────────────────────────────────────────┤
│         Infrastructure Layer            │
│  (Repositories, External APIs,          │
│   Domain Service Implementations        │
│   that need infrastructure access)      │
└─────────────────────────────────────────┘
```

---

## 🏗️ Designing Domain Services

### Naming Conventions

Domain services should be named using domain terminology:

```csharp
// ✅ Good - Uses domain language
public class ClientRegistrationService { }
public class EmailUniquenessChecker { }
public class OrderPricingService { }

// ❌ Bad - Technical or vague names
public class ClientHelper { }
public class ClientManager { }
public class ClientUtils { }
```

### 🔑 Interface vs Implementation: Where Do They Belong?

This is a critical concept. **Interfaces** always belong in the Domain Layer, but **implementations** depend on what they need:

```
┌─────────────────────────────────────────────────────────────────┐
│                        DOMAIN LAYER                             │
│  ┌───────────────────────────────────────────────────────────┐ │
│  │  Interfaces (always here):                                 │ │
│  │  • IEmailUniquenessChecker                                 │ │
│  │  • IClientRegistrationService                              │ │
│  │  • IClientTransferService                                  │ │
│  ├───────────────────────────────────────────────────────────┤ │
│  │  Pure Implementations (only domain dependencies):          │ │
│  │  • ClientRegistrationService (depends on interface only)  │ │
│  │  • ClientTransferService (depends on interface only)      │ │
│  └───────────────────────────────────────────────────────────┘ │
├─────────────────────────────────────────────────────────────────┤
│                    INFRASTRUCTURE LAYER                         │
│  ┌───────────────────────────────────────────────────────────┐ │
│  │  Implementations needing infrastructure:                   │ │
│  │  • EmailUniquenessChecker (needs DbContext)               │ │
│  │  • PaymentGatewayService (needs external API)             │ │
│  └───────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────┘
```

#### Decision Table: Where Does the Implementation Go?

| Implementation Needs | Layer | Example |
|---------------------|-------|---------|
| Only domain objects (entities, value objects, other domain interfaces) | **Domain** | `ClientTransferService` |
| Database access (DbContext, repositories) | **Infrastructure** | `EmailUniquenessChecker` |
| External APIs (payment, email, SMS) | **Infrastructure** | `PaymentGatewayService` |
| File system, network, hardware | **Infrastructure** | `DocumentStorageService` |

#### Why This Matters

```csharp
// ✅ ClientTransferService → DOMAIN LAYER
// Only depends on IEmailUniquenessChecker (a domain interface)
// and Client entity - no infrastructure!
public class ClientTransferService : IClientTransferService
{
    private readonly IEmailUniquenessChecker _emailChecker; // Domain interface
    // ...
}

// ✅ EmailUniquenessChecker → INFRASTRUCTURE LAYER  
// Depends on DbContext - that's infrastructure!
public class EmailUniquenessChecker : IEmailUniquenessChecker
{
    private readonly OrderDbContext _context; // Infrastructure dependency
    // ...
}
```

> **💡 Key Insight**: The Domain Layer defines WHAT needs to happen (interfaces).
> The Infrastructure Layer defines HOW it happens (implementations needing external resources).

### Interface Design

Define interfaces for domain services to enable testing and dependency injection:

```csharp
namespace OrderContext.Domain.Services;

public interface IEmailUniquenessChecker
{
    bool IsEmailUnique(Email email);
    Task<bool> IsEmailUniqueAsync(Email email, CancellationToken cancellationToken = default);
}

public interface IClientRegistrationService
{
    Client RegisterClient(string name, Email email);
    Task<Client> RegisterClientAsync(string name, Email email, CancellationToken cancellationToken = default);
}
```

---

## 📝 Example: Email Uniqueness Checker

> **📍 Layer Placement**: Interface in **Domain**, Implementation in **Infrastructure** (needs DbContext)

Let's create a domain service that checks if an email is unique across all clients. This is a perfect candidate for a domain service because:

- It operates across multiple `Client` entities
- It's a domain rule (business requirement)
- It doesn't belong to a single `Client` entity

### Step 1: Define the Interface (Domain Layer)

```csharp
namespace OrderContext.Domain.Services;

/// <summary>
/// Domain service interface for checking email uniqueness.
/// This interface lives in the domain layer, but implementations
/// may live in the infrastructure layer.
/// </summary>
public interface IEmailUniquenessChecker
{
    /// <summary>
    /// Checks if the given email is unique (not used by any existing client).
    /// </summary>
    /// <param name="email">The email to check</param>
    /// <returns>True if the email is unique, false otherwise</returns>
    bool IsEmailUnique(Email email);

    /// <summary>
    /// Checks if the given email is unique, excluding a specific client.
    /// Useful when updating a client's email.
    /// </summary>
    /// <param name="email">The email to check</param>
    /// <param name="excludeClientId">The client ID to exclude from the check</param>
    /// <returns>True if the email is unique, false otherwise</returns>
    bool IsEmailUnique(Email email, Guid excludeClientId);
}
```

### Step 2: Implement the Service (Infrastructure Layer)

```csharp
using OrderContext.Domain;
using OrderContext.Domain.Services;
using Microsoft.EntityFrameworkCore;

namespace OrderContext.Infrastructure.Services;

/// <summary>
/// Infrastructure implementation of IEmailUniquenessChecker.
/// Uses EF Core to query the database.
/// </summary>
public class EmailUniquenessChecker : IEmailUniquenessChecker
{
    private readonly OrderDbContext _context;

    public EmailUniquenessChecker(OrderDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public bool IsEmailUnique(Email email)
    {
        if (email == null) throw new ArgumentNullException(nameof(email));

        return !_context.Clients.Any(c => c.Email.Value == email.Value);
    }

    public bool IsEmailUnique(Email email, Guid excludeClientId)
    {
        if (email == null) throw new ArgumentNullException(nameof(email));

        return !_context.Clients
            .Where(c => c.Id != excludeClientId)
            .Any(c => c.Email.Value == email.Value);
    }
}
```

### Step 3: Use in Client Registration Service

> **📍 Layer**: This service lives in the **Domain Layer** because it only depends on `IEmailUniquenessChecker` (a domain interface) and `Client` (an entity).

```csharp
namespace OrderContext.Domain.Services;

/// <summary>
/// Domain service for client registration business logic.
/// Lives in Domain Layer - only depends on domain interfaces and entities.
/// </summary>
public class ClientRegistrationService : IClientRegistrationService
{
    private readonly IEmailUniquenessChecker _emailChecker;

    public ClientRegistrationService(IEmailUniquenessChecker emailChecker)
    {
        _emailChecker = emailChecker ?? throw new ArgumentNullException(nameof(emailChecker));
    }

    public Client RegisterClient(string name, Email email)
    {
        // Domain rule: Email must be unique across all clients
        if (!_emailChecker.IsEmailUnique(email))
        {
            throw new DomainException($"A client with email '{email.Value}' already exists.");
        }

        // Create the client using the factory method
        return Client.Create(name, email);
    }
}
```

---

## 📝 Example: Client Transfer Service

> **📍 Layer Placement**: Interface AND Implementation both in **Domain Layer** (only needs domain interfaces)

Here's another example of a domain service that handles transferring a client from one category to another (if your domain has client categories):

```csharp
namespace OrderContext.Domain.Services;

/// <summary>
/// Domain service for handling complex client operations
/// that span multiple entities or require business validation.
/// Lives in Domain Layer - only depends on domain interfaces.
/// </summary>
public interface IClientTransferService
{
    /// <summary>
    /// Updates a client's email with uniqueness validation.
    /// </summary>
    void UpdateClientEmail(Client client, Email newEmail);
}

/// <summary>
/// Implementation lives in DOMAIN LAYER because:
/// - Only depends on IEmailUniquenessChecker (domain interface)
/// - Only works with Client entity and Email value object
/// - No infrastructure dependencies (no DbContext, no external APIs)
/// </summary>
public class ClientTransferService : IClientTransferService
{
    private readonly IEmailUniquenessChecker _emailChecker; // Domain interface, not DbContext!

    public ClientTransferService(IEmailUniquenessChecker emailChecker)
    {
        _emailChecker = emailChecker ?? throw new ArgumentNullException(nameof(emailChecker));
    }

    public void UpdateClientEmail(Client client, Email newEmail)
    {
        if (client == null) throw new ArgumentNullException(nameof(client));
        if (newEmail == null) throw new ArgumentNullException(nameof(newEmail));

        // Skip check if email hasn't changed
        if (client.Email == newEmail) return;

        // Domain rule: New email must be unique (excluding current client)
        if (!_emailChecker.IsEmailUnique(newEmail, client.Id))
        {
            throw new DomainException($"Cannot update email: '{newEmail.Value}' is already in use.");
        }

        // Update the client's email
        client.UpdateEmail(newEmail);
    }
}
```

---

## 💉 Dependency Injection Setup

Register your domain services in the DI container:

### In ASP.NET Core

```csharp
// In Program.cs or Startup.cs

// Register infrastructure implementations
builder.Services.AddScoped<IEmailUniquenessChecker, EmailUniquenessChecker>();

// Register domain services
builder.Services.AddScoped<IClientRegistrationService, ClientRegistrationService>();
builder.Services.AddScoped<IClientTransferService, ClientTransferService>();
```

### Usage in Application Service

```csharp
namespace OrderContext.Application.Services;

public class ClientApplicationService
{
    private readonly IClientRegistrationService _registrationService;
    private readonly IClientRepository _clientRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ClientApplicationService(
        IClientRegistrationService registrationService,
        IClientRepository clientRepository,
        IUnitOfWork unitOfWork)
    {
        _registrationService = registrationService;
        _clientRepository = clientRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> RegisterClientAsync(RegisterClientCommand command)
    {
        // Create email value object
        var email = Email.Create(command.Email);

        // Use domain service for business logic
        var client = _registrationService.RegisterClient(command.Name, email);

        // Persist using repository
        await _clientRepository.AddAsync(client);
        await _unitOfWork.SaveChangesAsync();

        return client.Id;
    }
}
```

---

## 🧪 Testing Domain Services

Domain services are easy to test because they're stateless and use interfaces:

```csharp
using Moq;
using Xunit;
using OrderContext.Domain;
using OrderContext.Domain.Services;

namespace OrderContext.Tests.Services;

public class ClientRegistrationServiceTests
{
    private readonly Mock<IEmailUniquenessChecker> _emailCheckerMock;
    private readonly ClientRegistrationService _service;

    public ClientRegistrationServiceTests()
    {
        _emailCheckerMock = new Mock<IEmailUniquenessChecker>();
        _service = new ClientRegistrationService(_emailCheckerMock.Object);
    }

    [Fact]
    public void RegisterClient_WithUniqueEmail_ShouldCreateClient()
    {
        // Arrange
        var email = Email.Create("test@example.com");
        _emailCheckerMock
            .Setup(x => x.IsEmailUnique(email))
            .Returns(true);

        // Act
        var client = _service.RegisterClient("John Doe", email);

        // Assert
        Assert.NotNull(client);
        Assert.Equal("John Doe", client.Name);
        Assert.Equal(email, client.Email);
    }

    [Fact]
    public void RegisterClient_WithDuplicateEmail_ShouldThrowDomainException()
    {
        // Arrange
        var email = Email.Create("existing@example.com");
        _emailCheckerMock
            .Setup(x => x.IsEmailUnique(email))
            .Returns(false);

        // Act & Assert
        var exception = Assert.Throws<DomainException>(
            () => _service.RegisterClient("Jane Doe", email));

        Assert.Contains("already exists", exception.Message);
    }
}
```

---

## 📋 Best Practices Summary

### ✅ Do

- **Keep services stateless** - No instance fields that change between calls
- **Use domain language** - Name services after business concepts
- **Define interfaces** - Enable testing and loose coupling
- **Single responsibility** - Each service should have one clear purpose
- **Inject dependencies** - Use constructor injection for other domain services

### ❌ Don't

- **Don't access infrastructure directly** - Use interfaces/abstractions
- **Don't handle transactions** - Leave that to application services
- **Don't bloat services** - If a service grows too large, split it
- **Don't mix concerns** - Keep domain logic separate from infrastructure

---

## 🔗 Related Patterns

- **[Entity](Entity.md)** - When logic belongs to a single aggregate root
- **[Value Object](ValueObject.md)** - For immutable domain concepts like `Email`
- **[Aggregate](Aggregate.md)** - Cluster of entities and value objects
- **[Repository](Repository.md)** - For data access abstraction
- **[Domain Event](DomainEvent.md)** - For cross-aggregate communication

---

## 📚 Further Reading

- [Domain-Driven Design by Eric Evans](https://www.domainlanguage.com/ddd/) - The original DDD book
- [Implementing Domain-Driven Design by Vaughn Vernon](https://www.oreilly.com/library/view/implementing-domain-driven-design/9780133039900/) - Practical DDD implementation
- [Microsoft DDD Guide](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/) - DDD in .NET microservices
