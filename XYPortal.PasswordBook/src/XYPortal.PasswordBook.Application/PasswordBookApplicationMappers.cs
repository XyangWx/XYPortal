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
    private partial void MapToDtoIgnoreComplexFields(PasswordBookEntity source, PasswordBookDto destination);

    public override partial void Map(PasswordBookEntity source, PasswordBookDto destination);

    public override partial PasswordBookDto Map(PasswordBookEntity source);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class PasswordEntryMapper : MapperBase<PasswordEntry, PasswordEntryDto>
{
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
    }
}
