using MediatR;
using TaskFlow.Shared.Kernel.Results;
using TaskFlow.Users.Application.DTOs;

namespace TaskFlow.Users.Application.Queries.GetAllUsers;

public sealed record GetAllUsersQuery : IRequest<Result<List<UserResponse>>>;