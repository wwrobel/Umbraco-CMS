﻿using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Manages user groups.
/// </summary>
public interface IUserGroupService
{
    /// <summary>
    /// Gets all user groups.
    /// </summary>
    /// <param name="skip">The amount of user groups to skip.</param>
    /// <param name="take">The amount of user groups to take.</param>
    /// <returns>All user groups as an enumerable list of <see cref="IUserGroup"/>.</returns>
    Task<PagedModel<IUserGroup>> GetAllAsync(int skip, int take);

    /// <summary>
    ///     Gets all UserGroups matching an ID in the parameter list.
    /// </summary>
    /// <param name="ids">Optional Ids of UserGroups to retrieve.</param>
    /// <returns>An enumerable list of <see cref="IUserGroup"/>.</returns>
    Task<IEnumerable<IUserGroup>> GetAsync(params int[] ids);

    /// <summary>
    ///     Gets all UserGroups matching an alias in the parameter list.
    /// </summary>
    /// <param name="aliases">Alias of the UserGroup to retrieve.</param>
    /// <returns>
    ///     <returns>An enumerable list of <see cref="IUserGroup"/>.</returns>
    /// </returns>
    Task<IEnumerable<IUserGroup>> GetAsync(params string[] aliases);

    /// <summary>
    ///     Gets a UserGroup by its Alias
    /// </summary>
    /// <param name="alias">Name of the UserGroup to retrieve.</param>
    /// <returns>
    ///     <see cref="IUserGroup" />
    /// </returns>
    Task<IUserGroup?> GetAsync(string alias);

    /// <summary>
    ///     Gets a UserGroup by its Id
    /// </summary>
    /// <param name="id">Id of the UserGroup to retrieve.</param>
    /// <returns>
    ///     <see cref="IUserGroup" />
    /// </returns>
    Task<IUserGroup?> GetAsync(int id);

    /// <summary>
    /// Gets a UserGroup by its key
    /// </summary>
    /// <param name="key">Key of the UserGroup to retrieve.</param>
    /// <returns>
    ///     <see cref="IUserGroup" />
    /// </returns>
    Task<IUserGroup?> GetAsync(Guid key);

    Task<IEnumerable<IUserGroup>> GetAsync(IEnumerable<Guid> keys);

    /// <summary>
    /// Persists a new user group.
    /// </summary>
    /// <param name="userGroup">The user group to create.</param>
    /// <param name="performingUserId">The ID of the user responsible for creating the group.</param>
    /// <param name="groupMembersUserIds">The IDs of the users that should be part of the group when created.</param>
    /// <returns>An attempt indicating if the operation was a success as well as a more detailed <see cref="UserGroupOperationStatus"/>.</returns>
    Task<Attempt<IUserGroup, UserGroupOperationStatus>> CreateAsync(IUserGroup userGroup, int performingUserId, int[]? groupMembersUserIds = null);

    /// <summary>
    /// Updates an existing user group.
    /// </summary>
    /// <param name="userGroup">The user group to update.</param>
    /// <param name="performingUserId">The ID of the user responsible for updating the group.</param>
    /// <returns>An attempt indicating if the operation was a success as well as a more detailed <see cref="UserGroupOperationStatus"/>.</returns>
    Task<Attempt<IUserGroup, UserGroupOperationStatus>> UpdateAsync(IUserGroup userGroup, int performingUserId);

    /// <summary>
    ///     Deletes a UserGroup
    /// </summary>
    /// <param name="key">The key of the user group to delete.</param>
    /// <returns>An attempt indicating if the operation was a success as well as a more detailed <see cref="UserGroupOperationStatus"/>.</returns>
    Task<Attempt<UserGroupOperationStatus>> DeleteAsync(Guid key);

    /// <summary>
    /// Updates the users to have the groups specified.
    /// </summary>
    /// <param name="userGroupKeys">The user groups the users should be part of.</param>
    /// <param name="userKeys">The user whose groups we want to alter.</param>
    /// <returns>An attempt indicating if the operation was a success as well as a more detailed <see cref="UserGroupOperationStatus"/>.</returns>
    Task UpdateUserGroupsOnUsers(ISet<Guid> userGroupKeys, ISet<Guid> userKeys);
}