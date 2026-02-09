using TaskFlow.Users.Domain.Entities;

namespace TaskFlow.Users.Application.Interfaces;

public interface IJwtService
{
    string GenerateToken(User user);
}