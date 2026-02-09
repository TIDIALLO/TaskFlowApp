using MediatR;
using TaskFlow.Shared.Kernel.Results;
using TaskFlow.Users.Application.DTOs;

namespace TaskFlow.Users.Application.Commands.Login;

public sealed record LoginCommand(string Email, string Password) : IRequest<Result<AuthResponse>>;