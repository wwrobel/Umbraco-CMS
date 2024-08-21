﻿using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.PublishedCache;

public interface IPublishedContentHybridCache : IPublishedContentCache
{
    /// <summary>
    ///     Gets a content identified by its unique identifier.
    /// </summary>
    /// <param name="id">The content unique identifier.</param>
    /// <param name="preview">A value indicating whether to consider unpublished content.</param>
    /// <returns>The content, or null.</returns>
    /// <remarks>Considers published or unpublished content depending on defaults.</remarks>
    Task<IPublishedContent?> GetByIdAsync(int id, bool preview = false);

    /// <summary>
    ///     Gets a content identified by its unique identifier.
    /// </summary>
    /// <param name="key">The content unique identifier.</param>
    /// <param name="preview">A value indicating whether to consider unpublished content.</param>
    /// <returns>The content, or null.</returns>
    /// <remarks>Considers published or unpublished content depending on defaults.</remarks>
    Task<IPublishedContent?> GetByIdAsync(Guid key, bool preview = false);

    /// <summary>
    ///     Gets a value indicating whether the cache contains a specified content.
    /// </summary>
    /// <param name="id">The content unique identifier.</param>
    /// <param name="preview">A value indicating whether to consider unpublished content.</param>
    /// <returns>A value indicating whether to the cache contains the specified content.</returns>
    /// <remarks>The value of <paramref name="preview" /> overrides defaults.</remarks>
    Task<bool> HasByIdAsync(int id, bool preview = false);
}
