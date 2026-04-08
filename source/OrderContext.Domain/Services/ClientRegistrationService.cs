using OrderContext.Domain.Common;

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

    /// <inheritdoc />
    public Client RegisterClient(string name, Email email)
    {
        if (email == null)
            throw new ArgumentNullException(nameof(email));

        // Domain rule: Email must be unique across all clients
        if (!_emailChecker.IsEmailUnique(email))
        {
            throw new DomainException($"A client with email '{email.Value}' already exists.");
        }

        // Create the client using the factory method
        return Client.Create(name, email);
    }
}
