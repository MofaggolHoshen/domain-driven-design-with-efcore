using OrderContext.Domain.Common;

namespace OrderContext.Domain.Services;

/// <summary>
/// Domain service for handling complex client operations
/// that span multiple entities or require business validation.
/// 
/// Implementation lives in DOMAIN LAYER because:
/// - Only depends on IEmailUniquenessChecker (domain interface)
/// - Only works with Client entity and Email value object
/// - No infrastructure dependencies (no DbContext, no external APIs)
/// </summary>
public class ClientTransferService : IClientTransferService
{
    private readonly IEmailUniquenessChecker _emailChecker;

    public ClientTransferService(IEmailUniquenessChecker emailChecker)
    {
        _emailChecker = emailChecker ?? throw new ArgumentNullException(nameof(emailChecker));
    }

    /// <inheritdoc />
    public void UpdateClientEmail(Client client, Email newEmail)
    {
        if (client == null)
            throw new ArgumentNullException(nameof(client));
        if (newEmail == null)
            throw new ArgumentNullException(nameof(newEmail));

        // Skip check if email hasn't changed
        if (client.Email == newEmail)
            return;

        // Domain rule: New email must be unique (excluding current client)
        if (!_emailChecker.IsEmailUnique(newEmail, client.Id))
        {
            throw new DomainException($"Cannot update email: '{newEmail.Value}' is already in use.");
        }

        // Update the client's email
        client.UpdateEmail(newEmail);
    }
}
