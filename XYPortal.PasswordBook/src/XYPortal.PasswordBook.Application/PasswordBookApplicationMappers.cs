using System;
using System.Collections.Generic;
using System.Linq;
using Riok.Mapperly.Abstractions;
using Volo.Abp.Mapperly;
using XYPortal.PasswordBook.AggregateRoots;
using XYPortal.PasswordBook.Application.Contracts.Dtos;
using XYPortal.PasswordBook.Entities;
using PasswordBookEntity = XYPortal.PasswordBook.AggregateRoots.PasswordBook;

namespace XYPortal.PasswordBook;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class PasswordBookApplicationMappers : MapperBase<PasswordBookEntity, PasswordBookDto>
{
    [MapperIgnoreTarget(nameof(PasswordBookDto.AllowedType))]
    [MapperIgnoreTarget(nameof(PasswordBookDto.MinLength))]
    [MapperIgnoreTarget(nameof(PasswordBookDto.MaxLength))]
    [MapperIgnoreTarget(nameof(PasswordBookDto.EntryCount))]
    [MapperIgnoreTarget(nameof(PasswordBookDto.PasswordEntries))]
    public override partial void Map(PasswordBookEntity source, PasswordBookDto destination);

    [MapperIgnoreTarget(nameof(PasswordBookDto.AllowedType))]
    [MapperIgnoreTarget(nameof(PasswordBookDto.MinLength))]
    [MapperIgnoreTarget(nameof(PasswordBookDto.MaxLength))]
    [MapperIgnoreTarget(nameof(PasswordBookDto.EntryCount))]
    [MapperIgnoreTarget(nameof(PasswordBookDto.PasswordEntries))]
    public override partial PasswordBookDto Map(PasswordBookEntity source);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class PasswordEntryMapper : MapperBase<PasswordEntry, PasswordEntryDto>
{
    [MapperIgnoreTarget(nameof(PasswordEntryDto.PasswordBookId))]
    [MapperIgnoreTarget(nameof(PasswordEntryDto.PasswordHistories))]
    private partial void MapToDtoIgnoreComplexFields(PasswordEntry source, PasswordEntryDto destination);

    public override partial PasswordEntryDto Map(PasswordEntry source);
    public override partial void Map(PasswordEntry source, PasswordEntryDto destination);
}

/// <summary>
/// Extension methods for populating complex fields that cannot be mapped by Mapperly
/// </summary>
public static class PasswordBookDtoExtensions
{
    public static void PopulateComplexFields(this PasswordBookDto dto, PasswordBookEntity entity)
    {
        var format = entity.GetPasswordFormat();
        dto.AllowedType = format.AllowedType;
        dto.MinLength = format.MinLength;
        dto.MaxLength = format.MaxLength;
        dto.EntryCount = entity.PasswordEntries.Count(e => !e.IsDeleted);
        dto.PasswordEntries = entity.PasswordEntries
            .Select(e => MapEntryToDto(e, dto.Id))
            .ToList();
    }

    private static PasswordEntryDto MapEntryToDto(PasswordEntry entry, Guid passwordBookId)
    {
        var dto = new PasswordEntryDto
        {
            Id = entry.Id,
            PasswordBookId = passwordBookId,
            Title = entry.Title ?? string.Empty,
            HasUsername = entry.HasUsername,
            Username = entry.Username,
            PasswordType = entry.PasswordType,
            WeakLevel = entry.WeakLevel,
            Remark = entry.Remark,
            CreationTime = entry.CreationTime,
            LastModificationTime = entry.LastModificationTime,
            IsDeleted = entry.IsDeleted,
            CurrentPassword = entry.CurrentPassword,
            PasswordHistories = entry.PasswordHistories
                .Select(h => new PasswordHistoryDto
                {
                    Id = h.Id,
                    PasswordEntryId = h.PasswordEntryId,
                    PasswordValue = h.PasswordValue,
                    IsCurrent = h.IsCurrent,
                    CreationTime = h.CreationTime
                })
                .ToList()
        };
        return dto;
    }
}
