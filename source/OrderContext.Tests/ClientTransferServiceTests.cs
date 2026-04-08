using Moq;
using OrderContext.Domain;
using OrderContext.Domain.Common;
using OrderContext.Domain.Services;

namespace OrderContext.Tests;

public class ClientTransferServiceTests
{
    private readonly Mock<IEmailUniquenessChecker> _emailCheckerMock;
    private readonly ClientTransferService _service;

    public ClientTransferServiceTests()
    {
        _emailCheckerMock = new Mock<IEmailUniquenessChecker>();
        _service = new ClientTransferService(_emailCheckerMock.Object);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullEmailChecker_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ClientTransferService(null!));
    }

    #endregion

    #region UpdateClientEmail Tests

    [Fact]
    public void UpdateClientEmail_WithUniqueEmail_UpdatesClientEmail()
    {
        // Arrange
        var originalEmail = Email.Create("original@example.com");
        var newEmail = Email.Create("new@example.com");
        var client = Client.Create("John Doe", originalEmail);

        _emailCheckerMock
            .Setup(x => x.IsEmailUnique(newEmail, client.Id))
            .Returns(true);

        // Act
        _service.UpdateClientEmail(client, newEmail);

        // Assert
        Assert.Equal(newEmail, client.Email);
    }

    [Fact]
    public void UpdateClientEmail_WithDuplicateEmail_ThrowsDomainException()
    {
        // Arrange
        var originalEmail = Email.Create("original@example.com");
        var newEmail = Email.Create("existing@example.com");
        var client = Client.Create("John Doe", originalEmail);

        _emailCheckerMock
            .Setup(x => x.IsEmailUnique(newEmail, client.Id))
            .Returns(false);

        // Act & Assert
        var exception = Assert.Throws<DomainException>(
            () => _service.UpdateClientEmail(client, newEmail));

        Assert.Contains("already in use", exception.Message);
        Assert.Contains(newEmail.Value, exception.Message);
    }

    [Fact]
    public void UpdateClientEmail_WithSameEmail_DoesNotCallUniquenessChecker()
    {
        // Arrange
        var email = Email.Create("same@example.com");
        var client = Client.Create("John Doe", email);

        // Act
        _service.UpdateClientEmail(client, email);

        // Assert
        _emailCheckerMock.Verify(
            x => x.IsEmailUnique(It.IsAny<Email>(), It.IsAny<Guid>()), 
            Times.Never);
    }

    [Fact]
    public void UpdateClientEmail_WithSameEmail_KeepsOriginalEmail()
    {
        // Arrange
        var email = Email.Create("same@example.com");
        var client = Client.Create("John Doe", email);

        // Act
        _service.UpdateClientEmail(client, email);

        // Assert
        Assert.Equal(email, client.Email);
    }

    [Fact]
    public void UpdateClientEmail_WithNullClient_ThrowsArgumentNullException()
    {
        // Arrange
        var newEmail = Email.Create("new@example.com");

        // Act & Assert
        Assert.Throws<ArgumentNullException>(
            () => _service.UpdateClientEmail(null!, newEmail));
    }

    [Fact]
    public void UpdateClientEmail_WithNullEmail_ThrowsArgumentNullException()
    {
        // Arrange
        var email = Email.Create("original@example.com");
        var client = Client.Create("John Doe", email);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(
            () => _service.UpdateClientEmail(client, null!));
    }

    [Fact]
    public void UpdateClientEmail_CallsEmailUniquenessCheckerWithClientId()
    {
        // Arrange
        var originalEmail = Email.Create("original@example.com");
        var newEmail = Email.Create("new@example.com");
        var client = Client.Create("John Doe", originalEmail);

        _emailCheckerMock
            .Setup(x => x.IsEmailUnique(newEmail, client.Id))
            .Returns(true);

        // Act
        _service.UpdateClientEmail(client, newEmail);

        // Assert
        _emailCheckerMock.Verify(
            x => x.IsEmailUnique(newEmail, client.Id), 
            Times.Once);
    }

    [Fact]
    public void UpdateClientEmail_WithDuplicateEmail_DoesNotUpdateClient()
    {
        // Arrange
        var originalEmail = Email.Create("original@example.com");
        var newEmail = Email.Create("existing@example.com");
        var client = Client.Create("John Doe", originalEmail);

        _emailCheckerMock
            .Setup(x => x.IsEmailUnique(newEmail, client.Id))
            .Returns(false);

        // Act & Assert
        Assert.Throws<DomainException>(
            () => _service.UpdateClientEmail(client, newEmail));

        // Email should remain unchanged
        Assert.Equal(originalEmail, client.Email);
    }

    [Fact]
    public void UpdateClientEmail_WithEquivalentEmailDifferentCase_DoesNotCallUniquenessChecker()
    {
        // Arrange - Email value object normalizes to lowercase
        var originalEmail = Email.Create("Test@Example.com");
        var sameEmailDifferentCase = Email.Create("test@example.com");
        var client = Client.Create("John Doe", originalEmail);

        // Act
        _service.UpdateClientEmail(client, sameEmailDifferentCase);

        // Assert - Should skip check because emails are equal after normalization
        _emailCheckerMock.Verify(
            x => x.IsEmailUnique(It.IsAny<Email>(), It.IsAny<Guid>()), 
            Times.Never);
    }

    #endregion
}
