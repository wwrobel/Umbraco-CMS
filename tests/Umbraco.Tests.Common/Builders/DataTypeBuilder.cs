// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Text.Json;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Infrastructure.Serialization;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Builders.Interfaces;

namespace Umbraco.Cms.Tests.Common.Builders;

public class DataTypeBuilder
    : BuilderBase<DataType>,
        IWithIdBuilder,
        IWithKeyBuilder,
        IWithCreatorIdBuilder,
        IWithCreateDateBuilder,
        IWithUpdateDateBuilder,
        IWithDeleteDateBuilder,
        IWithNameBuilder,
        IWithParentIdBuilder,
        IWithTrashedBuilder,
        IWithLevelBuilder,
        IWithPathBuilder,
        IWithSortOrderBuilder
{
    private readonly DataEditorBuilder<DataTypeBuilder> _dataEditorBuilder;
    private DateTime? _createDate;
    private int? _creatorId;
    private ValueStorageType? _databaseType;
    private DateTime? _deleteDate;
    private int? _id;
    private Guid? _key;
    private int? _level;
    private string _name;
    private int? _parentId;
    private string _path;
    private int? _sortOrder;
    private bool? _trashed;
    private DateTime? _updateDate;

    public DataTypeBuilder() => _dataEditorBuilder = new DataEditorBuilder<DataTypeBuilder>(this);

    DateTime? IWithCreateDateBuilder.CreateDate
    {
        get => _createDate;
        set => _createDate = value;
    }

    int? IWithCreatorIdBuilder.CreatorId
    {
        get => _creatorId;
        set => _creatorId = value;
    }

    DateTime? IWithDeleteDateBuilder.DeleteDate
    {
        get => _deleteDate;
        set => _deleteDate = value;
    }

    int? IWithIdBuilder.Id
    {
        get => _id;
        set => _id = value;
    }

    Guid? IWithKeyBuilder.Key
    {
        get => _key;
        set => _key = value;
    }

    int? IWithLevelBuilder.Level
    {
        get => _level;
        set => _level = value;
    }

    string IWithNameBuilder.Name
    {
        get => _name;
        set => _name = value;
    }

    int? IWithParentIdBuilder.ParentId
    {
        get => _parentId;
        set => _parentId = value;
    }

    string IWithPathBuilder.Path
    {
        get => _path;
        set => _path = value;
    }

    int? IWithSortOrderBuilder.SortOrder
    {
        get => _sortOrder;
        set => _sortOrder = value;
    }

    bool? IWithTrashedBuilder.Trashed
    {
        get => _trashed;
        set => _trashed = value;
    }

    DateTime? IWithUpdateDateBuilder.UpdateDate
    {
        get => _updateDate;
        set => _updateDate = value;
    }

    public DataTypeBuilder WithDatabaseType(ValueStorageType databaseType)
    {
        _databaseType = databaseType;
        return this;
    }

    public DataEditorBuilder<DataTypeBuilder> AddEditor() => _dataEditorBuilder;

    public override DataType Build()
    {
        var editor = _dataEditorBuilder.Build();
        var parentId = _parentId ?? -1;
        var id = _id ?? 1;
        var key = _key ?? Guid.NewGuid();
        var createDate = _createDate ?? DateTime.Now;
        var updateDate = _updateDate ?? DateTime.Now;
        var deleteDate = _deleteDate;
        var name = _name ?? Guid.NewGuid().ToString();
        var level = _level ?? 0;
        var path = _path ?? $"-1,{id}";
        var creatorId = _creatorId ?? 1;
        var databaseType = _databaseType ?? ValueStorageType.Ntext;
        var sortOrder = _sortOrder ?? 0;
        var serializer = new ConfigurationEditorJsonSerializer();

        return new DataType(editor, serializer, parentId)
        {
            Id = id,
            Key = key,
            CreateDate = createDate,
            UpdateDate = updateDate,
            DeleteDate = deleteDate,
            Name = name,
            Trashed = _trashed ?? false,
            Level = level,
            Path = path,
            CreatorId = creatorId,
            DatabaseType = databaseType,
            SortOrder = sortOrder
        };
    }

    public static DataType CreateDataType(string dataTypeName, string editorAlias)
    {
        var builder = new DataTypeBuilder();
        return (DataType)builder
            .WithId(0)
            .WithName(dataTypeName)
            .AddEditor()
                .WithName(dataTypeName)
                .WithAlias(editorAlias)
            .Done()
            .Build();
    }

    public static DataType CreateBlockGridEditorDataTypeWithElement(Guid elementTypeKey, IJsonSerializer jsonSerializer, string dataTypeName = "BlockGridTestEditor")
    {
        var dataType = CreateDataType(dataTypeName, Constants.PropertyEditors.Aliases.BlockGrid);

        Dictionary<string, object>? blockConfig = jsonSerializer.Deserialize<Dictionary<string, object>>(
            jsonSerializer.Serialize(new BlockGridConfiguration
            {
                Blocks = new[]
                {
                    new BlockGridConfiguration.BlockGridBlockConfiguration
                    {
                        ContentElementTypeKey = elementTypeKey,
                    },
                },
            }));

        dataType.Configuration = blockConfig;
        return dataType;
    }

    public static DataType CreateBlockListEditorDataTypeWithElement(Guid elementTypeKey, IJsonSerializer jsonSerializer, string dataTypeName = "BlockListTestEditor")
    {
        var dataType = CreateDataType(dataTypeName, Constants.PropertyEditors.Aliases.BlockList);

        Dictionary<string, object>? blockConfig = jsonSerializer.Deserialize<Dictionary<string, object>>(
            jsonSerializer.Serialize(new BlockListConfiguration()
            {
                Blocks = new[]
                {
                    new BlockListConfiguration.BlockConfiguration()
                    {
                        ContentElementTypeKey = elementTypeKey,
                    },
                },
            }));

        dataType.Configuration = blockConfig;
        return dataType;
    }
}
