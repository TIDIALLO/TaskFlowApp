using TaskFlow.Users.Domain.Entities;
using TaskFlow.Users.Domain.ValueObjects;

namespace TaskFlow.Users.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default);
    Task<List<User>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Email email, CancellationToken cancellationToken = default);
    void Add(User user);
    void Update(User user);
}