﻿using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.HybridCache.Services;

namespace Umbraco.Cms.Infrastructure.HybridCache.NotificationHandlers;

internal sealed class CacheRefreshingNotificationHandler :
    INotificationAsyncHandler<ContentRefreshNotification>,
    INotificationAsyncHandler<ContentDeletedNotification>,
    INotificationAsyncHandler<MediaRefreshNotification>,
    INotificationAsyncHandler<MediaDeletedNotification>
{
    private readonly IContentCacheService _contentCacheService;
    private readonly IMediaCacheService _mediaCacheService;
    private readonly IPublishedSnapshotAccessor _publishedSnapshotAccessor;
    private readonly IRelationService _relationService;

    public CacheRefreshingNotificationHandler(
        IContentCacheService contentCacheService,
        IMediaCacheService mediaCacheService,
        IPublishedSnapshotAccessor publishedSnapshotAccessor,
        IRelationService relationService)
    {
        _contentCacheService = contentCacheService;
        _mediaCacheService = mediaCacheService;
        _publishedSnapshotAccessor = publishedSnapshotAccessor;
        _relationService = relationService;
    }

    public async Task HandleAsync(ContentRefreshNotification notification, CancellationToken cancellationToken)
    {
        await RefreshAsync(notification.Entity);

        await _contentCacheService.RefreshContentAsync(notification.Entity);
    }

    public async Task HandleAsync(ContentDeletedNotification notification, CancellationToken cancellationToken)
    {
        foreach (IContent deletedEntity in notification.DeletedEntities)
        {
            await RefreshAsync(deletedEntity);
            await _contentCacheService.DeleteItemAsync(deletedEntity.Id);
        }
    }

    public async Task HandleAsync(MediaRefreshNotification notification, CancellationToken cancellationToken)
    {
        await RefreshAsync(notification.Entity);
        await _mediaCacheService.RefreshMediaAsync(notification.Entity);
    }

    public async Task HandleAsync(MediaDeletedNotification notification, CancellationToken cancellationToken)
    {
        foreach (IMedia deletedEntity in notification.DeletedEntities)
        {
            await RefreshAsync(deletedEntity);
            await _mediaCacheService.DeleteItemAsync(deletedEntity.Id);
        }
    }

    private async Task RefreshAsync(IUmbracoEntity content)
    {
        IEnumerable<IRelation> parentRelations = _relationService.GetByParent(content)!;
        IEnumerable<IRelation> childRelations = _relationService.GetByChild(content);

        var ids = parentRelations.Select(x => x.ChildId).Concat(childRelations.Select(x => x.ParentId)).ToHashSet();
        foreach (var id in ids)
        {
            if (await _contentCacheService.HasContentByIdAsync(id) is false)
            {
                continue;
            }

            IPublishedContent? publishedContent = await _contentCacheService.GetByIdAsync(id);
            if (publishedContent is null)
            {
                continue;
            }

            foreach (IPublishedProperty publishedProperty in publishedContent.Properties)
            {
                var property = (PublishedProperty) publishedProperty;
                if (property.ReferenceCacheLevel != PropertyCacheLevel.Elements)
                {
                    continue;
                }

                if (_publishedSnapshotAccessor.TryGetPublishedSnapshot(out IPublishedSnapshot? snapshot))
                {
                    snapshot.ElementsCache?.ClearByKey(property.ValuesCacheKey);
                }

            }
        }
    }
}
