using Application.Common.Results;
using Application.DTOs.Auth;

namespace Application.Interfaces.Services;

public interface IUserService
{
    Task<Result<AuthResponseDto>> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken);
    Task<Result<AuthResponseDto>> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken);
}
