using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FluentValidation.Results;
using MediatR;

namespace Epam.ItMarathon.ApiService.Application.UseCases.User.Commands
{
    /// <summary>
    /// Command to delete user
    /// </summary>
    /// <param name="UserId">User id.</param>
    /// <param name="AdminUserCode">Special code to identify admin.</param>
    public record DeleteUserCommand(ulong UserId, string AdminUserCode)
        : IRequest<IResult<bool, ValidationResult>>;
}
