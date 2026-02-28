using Domain.Entities;

namespace Application.Interfaces;

public interface ITokenService
{
    (string token, DateTime expiresAtUtc) GenerateToken(User user);
}
