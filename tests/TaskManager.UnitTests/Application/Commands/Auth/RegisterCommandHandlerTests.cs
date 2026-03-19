using FluentAssertions;
using Moq;
using TaskManager.Application.Commands.Auth;
using TaskManager.Application.DTOs;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Interfaces;

namespace TaskManager.UnitTests.Application.Commands.Auth;

public class RegisterCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly RegisterCommandHandler _handler;

    public RegisterCommandHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _handler = new RegisterCommandHandler(_userRepositoryMock.Object, _passwordHasherMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsSuccessWithUserDto()
    {
        var dto = new RegisterDto("test@example.com", "Password1", "John", "Doe");
        var command = new RegisterCommand(dto);
        _userRepositoryMock.Setup(r => r.EmailExistsAsync(dto.Email)).ReturnsAsync(false);
        _passwordHasherMock.Setup(p => p.HashPassword(dto.Password)).Returns("hashed");

        var result = await _handler.Handle(command);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Email.Should().Be("test@example.com");
        result.Value.FirstName.Should().Be("John");
        _userRepositoryMock.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Once);
        _userRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_DuplicateEmail_ReturnsFailure()
    {
        var dto = new RegisterDto("existing@example.com", "Password1", "John", "Doe");
        var command = new RegisterCommand(dto);
        _userRepositoryMock.Setup(r => r.EmailExistsAsync(dto.Email)).ReturnsAsync(true);

        var result = await _handler.Handle(command);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("already registered");
    }
}
