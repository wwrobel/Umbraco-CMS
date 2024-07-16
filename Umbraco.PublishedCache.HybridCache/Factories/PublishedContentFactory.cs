﻿using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Infrastructure.HybridCache.Factories;

internal class PublishedContentFactory : IPublishedContentFactory
{
    private readonly PublishedContentTypeCache _contentTypeCache;
    private readonly IVariationContextAccessor _variationContextAccessor;


    public PublishedContentFactory(
        IVariationContextAccessor variationContextAccessor,
        IContentTypeService contentTypeService,
        IMemberTypeService memberTypeService,
        IMediaTypeService mediaTypeService,
        IPublishedContentTypeFactory publishedContentTypeFactory,
        ILoggerFactory loggerFactory)
    {
        _variationContextAccessor = variationContextAccessor;
        _contentTypeCache = new PublishedContentTypeCache(
            contentTypeService,
            mediaTypeService,
            memberTypeService,
            publishedContentTypeFactory,
            loggerFactory.CreateLogger<PublishedContentTypeCache>());
    }

    public IPublishedContent? ToIPublishedContent(ContentCacheNode contentCacheNode, bool preview)
    {
        var contentNode = new ContentNode(contentCacheNode.Id, contentCacheNode.Key, contentCacheNode.Path, contentCacheNode.SortOrder, contentCacheNode.CreateDate, contentCacheNode.CreatorId);
        IPublishedContentType contentType = _contentTypeCache.Get(PublishedItemType.Content, contentCacheNode.ContentTypeId);
        contentNode.SetContentTypeAndData(contentType, contentCacheNode.Draft, contentCacheNode.Published);

        return preview ? GetModel(contentNode, contentNode.DraftModel) ?? GetPublishedContentAsDraft(GetModel(contentNode, contentNode.PublishedModel)) : GetModel(contentNode, contentNode.PublishedModel);
    }

    public IPublishedContent? ToIPublishedMedia(ContentCacheNode contentCacheNode)
    {
        var contentNode = new ContentNode(contentCacheNode.Id, contentCacheNode.Key, contentCacheNode.Path, contentCacheNode.SortOrder, contentCacheNode.CreateDate, contentCacheNode.CreatorId);
        IPublishedContentType contentType = _contentTypeCache.Get(PublishedItemType.Media, contentCacheNode.ContentTypeId);
        contentNode.SetContentTypeAndData(contentType, contentCacheNode.Draft, contentCacheNode.Published);

        return GetModel(contentNode, contentNode.PublishedModel);
    }

    public IPublishedMember ToPublishedMember(IMember member)
    {
        IPublishedContentType contentType = _contentTypeCache.Get(PublishedItemType.Member, member.ContentTypeId);
        var d = new ContentData(member.Name, null, 0, member.UpdateDate, member.CreatorId, -1, false, GetPropertyValues(contentType, member), null);

        var n = new ContentNode(
            member.Id,
            member.Key,
            contentType,
            member.Path,
            member.SortOrder,
            member.CreateDate,
            member.CreatorId);

        return new PublishedMember(member, n, d, _variationContextAccessor);
    }

    private Dictionary<string, PropertyData[]> GetPropertyValues(IPublishedContentType contentType, IMember member)
    {
        var properties = member
            .Properties
            .ToDictionary(
                x => x.Alias,
                x => new[] { new PropertyData { Value = x.GetValue(), Culture = string.Empty, Segment = string.Empty } },
                StringComparer.OrdinalIgnoreCase);

        // see also PublishedContentType
        AddIf(contentType, properties, nameof(IMember.Email), member.Email);
        AddIf(contentType, properties, nameof(IMember.Username), member.Username);
        AddIf(contentType, properties, nameof(IMember.Comments), member.Comments);
        AddIf(contentType, properties, nameof(IMember.IsApproved), member.IsApproved);
        AddIf(contentType, properties, nameof(IMember.IsLockedOut), member.IsLockedOut);
        AddIf(contentType, properties, nameof(IMember.LastLockoutDate), member.LastLockoutDate);
        AddIf(contentType, properties, nameof(IMember.CreateDate), member.CreateDate);
        AddIf(contentType, properties, nameof(IMember.LastLoginDate), member.LastLoginDate);
        AddIf(contentType, properties, nameof(IMember.LastPasswordChangeDate), member.LastPasswordChangeDate);

        return properties;
    }

    private void AddIf(IPublishedContentType contentType, IDictionary<string, PropertyData[]> properties, string alias, object? value)
    {
        IPublishedPropertyType? propertyType = contentType.GetPropertyType(alias);
        if (propertyType == null || propertyType.IsUserProperty)
        {
            return;
        }

        properties[alias] = new[] { new PropertyData { Value = value, Culture = string.Empty, Segment = string.Empty } };
    }

    private IPublishedContent? GetModel(ContentNode node, ContentData? contentData) =>
        contentData == null
            ? null
            : new PublishedContent(
                node,
                contentData,
                _variationContextAccessor);

    private IPublishedContent? GetPublishedContentAsDraft(IPublishedContent? content) =>
        content == null ? null :
            // an object in the cache is either an IPublishedContentOrMedia,
            // or a model inheriting from PublishedContentExtended - in which
            // case we need to unwrap to get to the original IPublishedContentOrMedia.
            UnwrapIPublishedContent(content);

    private PublishedContent UnwrapIPublishedContent(IPublishedContent content)
    {
        while (content is PublishedContentWrapped wrapped)
        {
            content = wrapped.Unwrap();
        }

        if (!(content is PublishedContent inner))
        {
            throw new InvalidOperationException("Innermost content is not PublishedContent.");
        }

        return inner;
    }
}
