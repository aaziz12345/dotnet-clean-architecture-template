namespace Application.DTOs.Auth;

public sealed class AuthResponseDto
{
    public required string Token { get; init; }
    public required DateTime ExpiresAtUtc { get; init; }
    public required UserDto User { get; init; }
}
