namespace OrderContext.Domain.Services;

/// <summary>
/// Domain service interface for client registration business logic.
/// </summary>
public interface IClientRegistrationService
{
    /// <summary>
    /// Registers a new client with the given name and email.
    /// Validates that the email is unique across all clients.
    /// </summary>
    /// <param name="name">The client's name</param>
    /// <param name="email">The client's email (must be unique)</param>
    /// <returns>The newly created client</returns>
    Client RegisterClient(string name, Email email);
}
