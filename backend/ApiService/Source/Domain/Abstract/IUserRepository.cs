using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Epam.ItMarathon.ApiService.Domain.Entities.User;
using FluentValidation.Results;

namespace Epam.ItMarathon.ApiService.Domain.Abstract
{

    /// <summary>
    /// Repository interface for User entity operations.
    /// Provides methods for CRUD operations on User entities with functional result handling.
    /// </summary>
    public interface IUserRepository
    {
        /// <summary>
        /// Retrieves a user by their authorization code.
        /// </summary>
        /// <param name="userCode">The unique authorization code of the user.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> that can be used to cancel operation.</param>
        /// <param name="includeRoom">Whether to include the related Room entity. Default is false.</param>
        /// <param name="includeWishes">Whether to include the related Wishes collection. Default is false.</param>
        /// <returns>Returns <see cref="User"/> if found, otherwise <see cref="ValidationResult"/></returns>
        Task<Result<User, ValidationResult>> GetByCodeAsync(
            string userCode,
            CancellationToken cancellationToken,
            bool includeRoom = false,
            bool includeWishes = false);

        /// <summary>
        /// Retrieves a user by their unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the user.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> that can be used to cancel operation.</param>
        /// <param name="includeRoom">Whether to include the related Room entity. Default is false.</param>
        /// <param name="includeWishes">Whether to include the related Wishes collection. Default is false.</param>
        /// <returns>Returns <see cref="User"/> if found, otherwise <see cref="ValidationResult"/></returns>
        Task<Result<User, ValidationResult>> GetByIdAsync(
            ulong id,
            CancellationToken cancellationToken,
            bool includeRoom = false,
            bool includeWishes = false);

        /// <summary>
        /// Retrieves all users who are assigned to send gifts to a specific recipient user.
        /// </summary>
        /// <param name="giftRecipientUserId">The unique identifier of the gift recipient user.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> that can be used to cancel operation.</param>
        /// <returns>Returns list of <see cref="User"/> if found, otherwise <see cref="ValidationResult"/>.</returns>
        Task<Result<List<User>, ValidationResult>> GetUsersByGiftRecipientIdAsync(
            ulong giftRecipientUserId,
            CancellationToken cancellationToken);

        /// <summary>
        /// Updates an existing user in the database with new values.
        /// </summary>
        /// <param name="user">The User entity containing updated values.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> that can be used to cancel operation.</param>
        /// <returns>Returns <see cref="User"/> if found, otherwise <see cref="ValidationResult"/></returns>
        Task<Result<User, ValidationResult>> UpdateAsync(
            User user,
            CancellationToken cancellationToken);

        /// <summary>
        /// Deletes a user from the database.
        /// </summary>
        /// <param name="user">The User entity to delete.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> that can be used to cancel operation.</param>
        /// <returns>Returns <see cref="bool"/> if found, otherwise <see cref="ValidationResult"/></returns>
        Task<Result<bool, ValidationResult>> DeleteAsync(
            User user,
            CancellationToken cancellationToken);
    }
}
