using Application.DTOs.Auth;
using Application.Interfaces;
using Application.Services;
using Domain.Entities;
using Domain.Enums;
using Domain.Repositories;
using FluentValidation;
using Moq;

namespace UnitTests;

public sealed class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IPasswordHasher> _passwordHasher = new();
    private readonly Mock<ITokenService> _tokenService = new();
    private readonly IValidator<RegisterRequestDto> _registerValidator = new Application.Validators.RegisterRequestValidator();
    private readonly IValidator<LoginRequestDto> _loginValidator = new Application.Validators.LoginRequestValidator();

    [Fact]
    public async Task RegisterAsync_ShouldReturnSuccess_WhenRequestIsValid()
    {
        var service = CreateService();
        var request = new RegisterRequestDto { Email = "user@test.com", Password = "Password1!" };

        _userRepository.Setup(x => x.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        _passwordHasher.Setup(x => x.Hash(request.Password)).Returns("hashed");
        _tokenService.Setup(x => x.GenerateToken(It.IsAny<User>())).Returns(("token", DateTime.UtcNow.AddHours(1)));

        var result = await service.RegisterAsync(request, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("token", result.Value!.Token);
        _userRepository.Verify(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_ShouldReturnFailure_WhenEmailAlreadyExists()
    {
        var service = CreateService();
        var request = new RegisterRequestDto { Email = "user@test.com", Password = "Password1!" };

        _userRepository.Setup(x => x.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User());

        var result = await service.RegisterAsync(request, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Email is already registered.", result.Error);
    }

    [Fact]
    public async Task RegisterAsync_ShouldThrowValidationException_WhenEmailIsInvalid()
    {
        var service = CreateService();
        var request = new RegisterRequestDto { Email = "invalid", Password = "Password1!" };

        await Assert.ThrowsAsync<ValidationException>(() => service.RegisterAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnSuccess_WhenCredentialsAreValid()
    {
        var service = CreateService();
        var request = new LoginRequestDto { Email = "user@test.com", Password = "Password1!" };
        var user = new User { Email = request.Email, PasswordHash = "hash", Role = UserRole.User };

        _userRepository.Setup(x => x.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _passwordHasher.Setup(x => x.Verify(request.Password, user.PasswordHash)).Returns(true);
        _tokenService.Setup(x => x.GenerateToken(user)).Returns(("token", DateTime.UtcNow.AddMinutes(60)));

        var result = await service.LoginAsync(request, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("token", result.Value!.Token);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnFailure_WhenUserDoesNotExist()
    {
        var service = CreateService();
        var request = new LoginRequestDto { Email = "user@test.com", Password = "Password1!" };

        _userRepository.Setup(x => x.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await service.LoginAsync(request, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Invalid credentials.", result.Error);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnFailure_WhenPasswordIsInvalid()
    {
        var service = CreateService();
        var request = new LoginRequestDto { Email = "user@test.com", Password = "Password1!" };
        var user = new User { Email = request.Email, PasswordHash = "hash" };

        _userRepository.Setup(x => x.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _passwordHasher.Setup(x => x.Verify(request.Password, user.PasswordHash)).Returns(false);

        var result = await service.LoginAsync(request, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Invalid credentials.", result.Error);
    }

    private UserService CreateService()
    {
        return new UserService(
            _userRepository.Object,
            _passwordHasher.Object,
            _tokenService.Object,
            _registerValidator,
            _loginValidator);
    }
}
