using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using CSharpFunctionalExtensions;
using Epam.ItMarathon.ApiService.Domain.Abstract;
using Epam.ItMarathon.ApiService.Domain.Entities.User;
using Epam.ItMarathon.ApiService.Domain.Shared.ValidationErrors;
using Epam.ItMarathon.ApiService.Infrastructure.Database;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Epam.ItMarathon.ApiService.Infrastructure.Repositories
{
    internal class UserRepository(AppDbContext context, IMapper mapper, ILogger<UserRepository> logger) : IUserRepository
    {

        /// <inheritdoc/>
        public async Task<Result<User, ValidationResult>> GetByCodeAsync(string userCode,
            CancellationToken cancellationToken, bool includeRoom = false, bool includeWishes = false)
        {
            var userQuery = context.Users.AsQueryable();
            if (includeRoom)
            {
                userQuery = userQuery.Include(user => user.Room);
            }

            if (includeWishes)
            {
                userQuery = userQuery.Include(user => user.Wishes);
            }

            var userEf = await userQuery.FirstOrDefaultAsync(user => user.AuthCode.Equals(userCode), cancellationToken);
            var result = userEf == null
                ? Result.Failure<User, ValidationResult>(new NotFoundError([
                    new ValidationFailure(nameof(userCode), "User with such code not found")
                ]))
                : mapper.Map<User>(userEf);
            return result;
        }

        /// <inheritdoc/>
        public async Task<Result<User, ValidationResult>> GetByIdAsync(ulong id, CancellationToken cancellationToken,
            bool includeRoom = false, bool includeWishes = false)
        {
            var userQuery = context.Users.AsQueryable();
            if (includeRoom)
            {
                userQuery = userQuery.Include(user => user.Room);
            }

            if (includeWishes)
            {
                userQuery = userQuery.Include(user => user.Wishes);
            }

            var userEf = await userQuery.FirstOrDefaultAsync(user => user.Id.Equals(id), cancellationToken);
            var result = userEf == null
                ? Result.Failure<User, ValidationResult>(new NotFoundError([
                    new ValidationFailure(nameof(id), "User with such id not found")
                ]))
                : mapper.Map<User>(userEf);
            return result;
        }

        /// <inheritdoc/>
        public async Task<Result<List<User>, ValidationResult>> GetUsersByGiftRecipientIdAsync(
            ulong giftRecipientUserId,
            CancellationToken cancellationToken)
        {
            var usersEf = await context.Users
                .Where(user => user.GiftRecipientUserId == giftRecipientUserId)
                .ToListAsync(cancellationToken);

            return Result.Success<List<User>, ValidationResult>(mapper.Map<List<User>>(usersEf));
        }

        /// <inheritdoc/>
        public async Task<Result<User, ValidationResult>> UpdateAsync(User user, CancellationToken cancellationToken)
        {
            var userEf = await context.Users
                .FirstOrDefaultAsync(u => u.Id == user.Id, cancellationToken);

            if (userEf == null)
            {
                return Result.Failure<User, ValidationResult>(new NotFoundError([
                    new ValidationFailure(nameof(user.Id), "User not found")
                ]));
            }

            // Update properties
            userEf.FirstName = user.FirstName;
            userEf.LastName = user.LastName;
            userEf.Phone = user.Phone;
            userEf.Email = user.Email;
            userEf.DeliveryInfo = user.DeliveryInfo;
            userEf.GiftRecipientUserId = user.GiftRecipientUserId;
            userEf.WantSurprise = user.WantSurprise;
            userEf.Interests = user.Interests;
            userEf.ModifiedOn = DateTime.UtcNow;

            try
            {
                await context.SaveChangesAsync(cancellationToken);
                return mapper.Map<User>(userEf);
            }
            catch (DbUpdateException ex)
            {
                logger.LogError(ex.ToString());
                throw;
            }
        }

        public async Task<Result<bool, ValidationResult>> DeleteAsync(User user, CancellationToken cancellationToken)
        {
            var userEf = await context.Users
                .Include(u => u.Wishes)
                .FirstOrDefaultAsync(u => u.Id == user.Id, cancellationToken);

            if (userEf == null)
            {
                return Result.Failure<bool, ValidationResult>(new NotFoundError([
                    new ValidationFailure(nameof(user.Id), "User not found")
                ]));
            }

            try
            {
                // Clear gift recipient relationships for users who were assigned to send gifts to this user
                var usersWithThisRecipient = await context.Users
                    .Where(u => u.GiftRecipientUserId == user.Id)
                    .ToListAsync(cancellationToken);

                foreach (var u in usersWithThisRecipient)
                {
                    u.GiftRecipientUserId = null;
                    u.ModifiedOn = DateTime.UtcNow;
                }

                // Clear this user's gift recipient assignment if exists
                if (userEf.GiftRecipientUserId.HasValue)
                {
                    userEf.GiftRecipientUserId = null;
                }

                // Remove user from room (but keep in database)
                userEf.ModifiedOn = DateTime.UtcNow;

                // Delete wishes when leaving room
                if (userEf.Wishes != null && userEf.Wishes.Count >= 0)
                {
                    context.Gifts.RemoveRange(userEf.Wishes);
                }
                context.Users.Remove(userEf);

                await context.SaveChangesAsync(cancellationToken);

                logger.LogInformation("User with ID {UserId} successfully removeddd from room {RoomId}", user.Id, user.RoomId);

                return Result.Success<bool, ValidationResult>(true);
            }
            catch (DbUpdateException exception)
            {
                logger.LogError(exception.ToString());
                throw;
            }
        }


    }
}
