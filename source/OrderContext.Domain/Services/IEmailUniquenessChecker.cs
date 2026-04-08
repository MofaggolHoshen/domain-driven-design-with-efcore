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
