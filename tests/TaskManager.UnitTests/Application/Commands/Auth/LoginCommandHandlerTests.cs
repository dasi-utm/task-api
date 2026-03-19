using FluentAssertions;
using Moq;
using TaskManager.Application.Commands.Auth;
using TaskManager.Application.DTOs;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Interfaces;

namespace TaskManager.UnitTests.Application.Commands.Auth;

public class LoginCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _tokenServiceMock = new Mock<ITokenService>();
        _handler = new LoginCommandHandler(
            _userRepositoryMock.Object,
            _passwordHasherMock.Object,
            _tokenServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCredentials_ReturnsSuccessWithToken()
    {
        var user = User.Create("test@example.com", "hashed", "John", "Doe");
        var dto = new LoginDto("test@example.com", "Password1");
        var command = new LoginCommand(dto);
        _userRepositoryMock.Setup(r => r.GetByEmailAsync(dto.Email)).ReturnsAsync(user);
        _passwordHasherMock.Setup(p => p.VerifyPassword(dto.Password, user.PasswordHash)).Returns(true);
        _tokenServiceMock.Setup(t => t.GenerateToken(user)).Returns("jwt-token");

        var result = await _handler.Handle(command);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Token.Should().Be("jwt-token");
        result.Value.User.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task Handle_InvalidEmail_ReturnsFailure()
    {
        var dto = new LoginDto("nonexistent@example.com", "Password1");
        var command = new LoginCommand(dto);
        _userRepositoryMock.Setup(r => r.GetByEmailAsync(dto.Email)).ReturnsAsync((User?)null);

        var result = await _handler.Handle(command);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Invalid email or password");
    }

    [Fact]
    public async Task Handle_WrongPassword_ReturnsFailure()
    {
        var user = User.Create("test@example.com", "hashed", "John", "Doe");
        var dto = new LoginDto("test@example.com", "WrongPassword1");
        var command = new LoginCommand(dto);
        _userRepositoryMock.Setup(r => r.GetByEmailAsync(dto.Email)).ReturnsAsync(user);
        _passwordHasherMock.Setup(p => p.VerifyPassword(dto.Password, user.PasswordHash)).Returns(false);

        var result = await _handler.Handle(command);

        result.IsFailure.Should().BeTrue();
    }
}
