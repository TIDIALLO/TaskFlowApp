using MediatR;
using TaskFlow.Shared.Kernel.Results;
using TaskFlow.Users.Application.DTOs;

namespace TaskFlow.Users.Application.Queries.GetUserById;

public sealed record GetUserByIdQuery(Guid Id) : IRequest<Result<UserResponse>>;