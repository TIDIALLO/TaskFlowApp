using MediatR;
using TaskFlow.Shared.Kernel.Results;
using TaskFlow.Users.Application.DTOs;
using TaskFlow.Users.Application.Interfaces;

namespace TaskFlow.Users.Application.Queries.GetAllUsers;

public sealed class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, Result<List<UserResponse>>>
{
    private readonly IUserRepository _userRepository;

    public GetAllUsersQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<List<UserResponse>>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await _userRepository.GetAllAsync(cancellationToken);

        var response = users.Select(user => new UserResponse(
            user.Id,
            user.Email.Value,
            user.FullName.FirstName,
            user.FullName.LastName,
            user.IsActive,
            user.CreatedAt)).ToList();

        return Result<List<UserResponse>>.Success(response);
    }
}