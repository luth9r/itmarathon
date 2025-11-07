using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Epam.ItMarathon.ApiService.Application.UseCases.User.Commands;
using Epam.ItMarathon.ApiService.Domain.Abstract;
using Epam.ItMarathon.ApiService.Domain.Shared.ValidationErrors;
using FluentValidation.Results;
using MediatR;

namespace Epam.ItMarathon.ApiService.Application.UseCases.User.Handlers
{
    /// <summary>
    /// Handler for Delete user command.
    /// </summary>
    /// <param name="userRepository">Implementation of <see cref="IUserRepository"/> for operating with database.</param>
    /// <param name="roomRepository">Implementation of <see cref="IRoomRepository"/> for operating with database.</param>
    public class DeleteUserCommandHandler(IUserRepository userRepository, IRoomRepository roomRepository) :
        IRequestHandler<DeleteUserCommand, IResult<bool, ValidationResult>>
    {
        /// <inheritdoc/>
        public async Task<IResult<bool, ValidationResult>> Handle(DeleteUserCommand request,
    CancellationToken cancellationToken)
        {
            var adminUserCode = request.AdminUserCode;
            var userId = request.UserId;

            Console.WriteLine($"[DeleteUserCommandHandler] Starting deletion - UserId: {userId}, AdminCode: {adminUserCode}");

            // 1. Validate admin user by auth code
            var adminUserResult = await userRepository.GetByCodeAsync(adminUserCode, cancellationToken);
            if (adminUserResult.IsFailure)
            {
                Console.WriteLine($"[DeleteUserCommandHandler] Admin user not found with code: {adminUserCode}");
                return Result.Failure<bool, ValidationResult>(new NotFoundError([
                    new ValidationFailure(nameof(adminUserCode), "User with provided authorization code not found.")
                ]));
            }

            var adminUser = adminUserResult.Value;
            Console.WriteLine($"[DeleteUserCommandHandler] Admin user found - Id: {adminUser.Id}, RoomId: {adminUser.RoomId}");

            // 2. Get user to delete
            var userToDeleteResult = await userRepository.GetByIdAsync(userId, cancellationToken);
            if (userToDeleteResult.IsFailure)
            {
                Console.WriteLine($"[DeleteUserCommandHandler] User to delete not found - Id: {userId}");
                return Result.Failure<bool, ValidationResult>(new NotFoundError([
                    new ValidationFailure(nameof(userId), $"User with ID {userId} not found.")
                ]));
            }

            var userToDelete = userToDeleteResult.Value;
            Console.WriteLine($"[DeleteUserCommandHandler] User to delete found - Id: {userToDelete.Id}, RoomId: {userToDelete.RoomId}");

            // 3. Check if users are in the same room
            if (userToDelete.RoomId != adminUser.RoomId)
            {
                Console.WriteLine($"[DeleteUserCommandHandler] Users in different rooms - Admin RoomId: {adminUser.RoomId}, User RoomId: {userToDelete.RoomId}");
                return Result.Failure<bool, ValidationResult>(new ForbiddenError([
                    new ValidationFailure(nameof(userId), "Administrator and user belong to different rooms.")
                ]));
            }

            Console.WriteLine($"[DeleteUserCommandHandler] Users in same room: {adminUser.RoomId}");

            // 4. Get room by user code
            var roomResult = await roomRepository.GetByUserCodeAsync(adminUser.AuthCode, cancellationToken);
            if (roomResult.IsFailure)
            {
                Console.WriteLine($"[DeleteUserCommandHandler] Room not found for admin code: {adminUser.AuthCode}");
                return roomResult.ConvertFailure<bool>();
            }

            var room = roomResult.Value;
            Console.WriteLine($"[DeleteUserCommandHandler] Room found - Id: {room.Id}, UsersCount: {room.Users.Count}");

            // 5. Check if admin user is in room Users list
            var adminInRoom = room.Users.FirstOrDefault(u => u.AuthCode == adminUser.AuthCode);
            if (adminInRoom is null)
            {
                Console.WriteLine($"[DeleteUserCommandHandler] Admin user not found in room Users list");
                return Result.Failure<bool, ValidationResult>(new ForbiddenError([
                    new ValidationFailure(nameof(adminUserCode), "Only room administrators can delete users.")
                ]));
            }

            Console.WriteLine($"[DeleteUserCommandHandler] Admin user found in room - Id: {adminInRoom.Id}");

            // 6. Prevent admin from deleting themselves
            if (userToDelete.Id == adminUser.Id)
            {
                Console.WriteLine($"[DeleteUserCommandHandler] Admin trying to delete themselves - Id: {adminUser.Id}");
                return Result.Failure<bool, ValidationResult>(new BadRequestError([
                    new ValidationFailure(nameof(userId), "Administrator cannot delete themselves.")
                ]));
            }

            // 7. Prevent deleting room administrator
            var roomAdmin = room.Users.FirstOrDefault(u => u.Id == userToDelete.Id);
            if (roomAdmin != null && room.Users.Count(u => u.Id == roomAdmin.Id) > 0)
            {
                // Если это единственный пользователь или последний админ
                Console.WriteLine($"[DeleteUserCommandHandler] Checking if user to delete is room admin");

                // Проверьте, есть ли другие пользователи кроме удаляемого
                var otherUsersCount = room.Users.Count(u => u.Id != userToDelete.Id);
                if (otherUsersCount == 0)
                {
                    Console.WriteLine($"[DeleteUserCommandHandler] Cannot delete last user in room");
                    return Result.Failure<bool, ValidationResult>(new BadRequestError([
                        new ValidationFailure(nameof(userId), "Cannot delete the last user in room.")
                    ]));
                }
            }

            // 8. Check if room is closed
            if (room.ClosedOn.HasValue)
            {
                Console.WriteLine($"[DeleteUserCommandHandler] Room is closed - ClosedOn: {room.ClosedOn}");
                return Result.Failure<bool, ValidationResult>(new BadRequestError([
                    new ValidationFailure("Room", "Cannot delete users from a closed room.")
                ]));
            }

            // 9. Delete user
            Console.WriteLine($"[DeleteUserCommandHandler] Deleting user - Id: {userToDelete.Id}");
            var deleteResult = await userRepository.DeleteAsync(userToDelete, cancellationToken);

            if (deleteResult.IsSuccess)
            {
                Console.WriteLine($"[DeleteUserCommandHandler] User deleted successfully - Id: {userId}");
            }
            else
            {
                Console.WriteLine($"[DeleteUserCommandHandler] Failed to delete user - Id: {userId}");
            }

            return deleteResult;
        }

    }
}

