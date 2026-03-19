using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;
using XYPortal.PasswordBook.Application.Contracts.Dtos;
using XYPortal.PasswordBook.Application.Contracts.PasswordBooks;
using XYPortal.PasswordBook.AggregateRoots;
using XYPortal.PasswordBook.Domain.Services;
using XYPortal.PasswordBook.Entities;
using XYPortal.PasswordBook.Enums;
using XYPortal.PasswordBook.Permissions;
using XYPortal.PasswordBook.ValueObjects;
using PasswordBookEntity = XYPortal.PasswordBook.AggregateRoots.PasswordBook;
using PermissionChecker = Volo.Abp.Authorization.Permissions.IPermissionChecker;
using RandomCategory = XYPortal.RandomStringProvider.RandomCategory;
using XYPortal.RandomStringProvider.RandomStringProvider;

namespace XYPortal.PasswordBook.Application.PasswordBooks;

/// <summary>
/// PasswordBook Application Service Implementation
/// </summary>
public class PasswordBookAppService : CrudAppService<PasswordBookEntity, PasswordBookDto, Guid, PagedAndSortedResultRequestDto, CreateUpdatePasswordBookDto, CreateUpdatePasswordBookDto>, IPasswordBookAppService
{
    private readonly PasswordBookManager _passwordBookManager;
    private readonly PermissionChecker _permissionChecker;

    public PasswordBookAppService(
        IRepository<PasswordBookEntity, Guid> repository,
        PasswordBookManager passwordBookManager,
        PermissionChecker permissionChecker)
        : base(repository)
    {
        _passwordBookManager = passwordBookManager;
        _permissionChecker = permissionChecker;
    }

    /// <summary>
    /// Check PASSWORDBOOKUSER permission
    /// </summary>
    private async Task CheckPasswordBookPermissionAsync()
    {
        var hasPermission = await _permissionChecker.IsGrantedAsync(PasswordBookPermissions.PassWordBookUser);
        if (!hasPermission)
        {
            throw new AbpAuthorizationException("You do not have permission to use PasswordBook feature (PASSWORDBOOKUSER permission required)");
        }
    }

    /// <summary>
    /// Get user's PasswordBook list
    /// </summary>
    public async Task<ListResultDto<PasswordBookDto>> GetListByOwnerAsync()
    {
        await CheckPasswordBookPermissionAsync();

        var userId = CurrentUser.GetId();
        var books = await _passwordBookManager.GetListByOwnerAsync(userId);

        return new ListResultDto<PasswordBookDto>(
            ObjectMapper.Map<List<PasswordBookEntity>, List<PasswordBookDto>>(books)
        );
    }

    /// <summary>
    /// Get PasswordBook details with entries
    /// </summary>
    public async Task<PasswordBookDto> GetWithEntriesAsync(Guid id)
    {
        await CheckPasswordBookPermissionAsync();

        var userId = CurrentUser.GetId();
        if (!await _passwordBookManager.HasAccessPermissionAsync(userId, id))
        {
            throw new UnauthorizedAccessException("You do not have permission to access this PasswordBook");
        }

        var passwordBook = await _passwordBookManager.GetByIdAsync(id);
        return ObjectMapper.Map<PasswordBookEntity, PasswordBookDto>(passwordBook);
    }

    /// <summary>
    /// Add PasswordEntry
    /// </summary>
    public async Task<PasswordEntryDto> AddPasswordEntryAsync(Guid passwordBookId, CreatePasswordEntryDto input)
    {
        await CheckPasswordBookPermissionAsync();

        var userId = CurrentUser.GetId();
        if (!await _passwordBookManager.HasAccessPermissionAsync(userId, passwordBookId))
        {
            throw new UnauthorizedAccessException("You do not have permission to add entries to this PasswordBook");
        }

        var passwordBook = await _passwordBookManager.GetByIdAsync(passwordBookId);
        
        var entry = passwordBook.AddPasswordEntry(
            input.Title,
            input.HasUsername,
            input.Username,
            input.PasswordType,
            input.WeakLevel,
            input.Password,
            input.Remark
        );

        await Repository.UpdateAsync(passwordBook);

        return ObjectMapper.Map<PasswordEntry, PasswordEntryDto>(entry);
    }

    /// <summary>
    /// Update Password
    /// </summary>
    public async Task UpdatePasswordAsync(Guid passwordBookId, Guid entryId, UpdatePasswordDto input)
    {
        await CheckPasswordBookPermissionAsync();

        var userId = CurrentUser.GetId();
        if (!await _passwordBookManager.HasAccessPermissionAsync(userId, passwordBookId))
        {
            throw new UnauthorizedAccessException("You do not have permission to modify this password");
        }

        var passwordBook = await _passwordBookManager.GetByIdAsync(passwordBookId);
        passwordBook.UpdatePasswordValue(entryId, input.NewPassword);
        await Repository.UpdateAsync(passwordBook);
    }

    /// <summary>
    /// Delete PasswordEntry (Soft Delete)
    /// </summary>
    public async Task DeletePasswordEntryAsync(Guid passwordBookId, Guid entryId)
    {
        await CheckPasswordBookPermissionAsync();

        var userId = CurrentUser.GetId();
        if (!await _passwordBookManager.HasAccessPermissionAsync(userId, passwordBookId))
        {
            throw new UnauthorizedAccessException("You do not have permission to delete this password");
        }

        var passwordBook = await _passwordBookManager.GetByIdAsync(passwordBookId);
        passwordBook.RemovePasswordEntry(entryId);
        await Repository.UpdateAsync(passwordBook);
    }

    /// <summary>
    /// Restore PasswordEntry
    /// </summary>
    public async Task RestorePasswordEntryAsync(Guid passwordBookId, Guid entryId)
    {
        await CheckPasswordBookPermissionAsync();

        var userId = CurrentUser.GetId();
        if (!await _passwordBookManager.HasAccessPermissionAsync(userId, passwordBookId))
        {
            throw new UnauthorizedAccessException("You do not have permission to restore this password");
        }

        var passwordBook = await _passwordBookManager.GetByIdAsync(passwordBookId);
        passwordBook.RestorePasswordEntry(entryId);
        await Repository.UpdateAsync(passwordBook);
    }

    /// <summary>
    /// Evaluate Password Strength
    /// </summary>
    public Task<PasswordWeakLevel> EvaluatePasswordStrengthAsync(string password)
    {
        return Task.FromResult(_passwordBookManager.EvaluatePasswordStrength(password));
    }

    /// <summary>
    /// Soft Delete PasswordBook
    /// </summary>
    public override async Task DeleteAsync(Guid id)
    {
        await CheckPasswordBookPermissionAsync();

        var userId = CurrentUser.GetId();
        if (!await _passwordBookManager.HasAccessPermissionAsync(userId, id))
        {
            throw new UnauthorizedAccessException("You do not have permission to delete this PasswordBook");
        }

        await _passwordBookManager.SoftDeleteAsync(id);
    }

    /// <summary>
    /// Restore PasswordBook
    /// </summary>
    public async Task RestoreAsync(Guid id)
    {
        await CheckPasswordBookPermissionAsync();

        var userId = CurrentUser.GetId();
        if (!await _passwordBookManager.HasAccessPermissionAsync(userId, id))
        {
            throw new UnauthorizedAccessException("You do not have permission to restore this PasswordBook");
        }

        await _passwordBookManager.RestoreAsync(id);
    }

    /// <summary>
    /// Hard Delete PasswordBook
    /// </summary>
    public async Task HardDeleteAsync(Guid id)
    {
        await CheckPasswordBookPermissionAsync();

        var userId = CurrentUser.GetId();
        if (!await _passwordBookManager.HasAccessPermissionAsync(userId, id))
        {
            throw new UnauthorizedAccessException("You do not have permission to permanently delete this PasswordBook");
        }

        await _passwordBookManager.HardDeleteAsync(id);
    }

    /// <summary>
    /// Create PasswordBook
    /// </summary>
    public override async Task<PasswordBookDto> CreateAsync(CreateUpdatePasswordBookDto input)
    {
        await CheckPasswordBookPermissionAsync();

        var passwordFormat = new PasswordFormatRequirement(
            input.MinLength,
            input.MaxLength,
            input.RequireUppercase,
            input.RequireLowercase,
            input.RequireDigit,
            input.RequireSpecialChar,
            input.SpecialChars,
            input.AllowedType
        );

        var userId = CurrentUser.GetId();
        var passwordBook = await _passwordBookManager.CreateAsync(
            userId,
            input.Name,
            input.Description,
            passwordFormat
        );

        return ObjectMapper.Map<PasswordBookEntity, PasswordBookDto>(passwordBook);
    }

    /// <summary>
    /// Update PasswordBook
    /// </summary>
    public override async Task<PasswordBookDto> UpdateAsync(Guid id, CreateUpdatePasswordBookDto input)
    {
        await CheckPasswordBookPermissionAsync();

        var userId = CurrentUser.GetId();
        if (!await _passwordBookManager.HasAccessPermissionAsync(userId, id))
        {
            throw new UnauthorizedAccessException("You do not have permission to modify this PasswordBook");
        }

        var passwordBook = await _passwordBookManager.GetByIdAsync(id);
        passwordBook.UpdateInfo(input.Name, input.Description);

        var passwordFormat = new PasswordFormatRequirement(
            input.MinLength,
            input.MaxLength,
            input.RequireUppercase,
            input.RequireLowercase,
            input.RequireDigit,
            input.RequireSpecialChar,
            input.SpecialChars,
            input.AllowedType
        );
        passwordBook.UpdatePasswordFormat(passwordFormat);

        await Repository.UpdateAsync(passwordBook);

        return ObjectMapper.Map<PasswordBookEntity, PasswordBookDto>(passwordBook);
    }

    /// <summary>
    /// Generate Random Password
    /// </summary>
    public async Task<GenerateRandomPasswordResultDto> GenerateRandomPasswordAsync(GenerateRandomPasswordDto input)
    {
        await CheckPasswordBookPermissionAsync();

        var userId = CurrentUser.GetId();
        if (!await _passwordBookManager.HasAccessPermissionAsync(userId, input.PasswordBookId))
        {
            throw new UnauthorizedAccessException("You do not have permission to access this PasswordBook");
        }

        var passwordBook = await _passwordBookManager.GetByIdAsync(input.PasswordBookId);
        var format = passwordBook.GetPasswordFormat();

        // Map PasswordCharacterType to RandomCategory
        var category = MapToRandomCategory(input.CharacterTypes);

        var randomInput = new RandomStringInput
        {
            Length = input.Length,
            SymbolCategories = category,
            IsOnlyOnce = input.IsOnlyOnce,
            Prefix = input.Prefix,
            Suffix = input.Suffix
        };

        if (!string.IsNullOrEmpty(input.IgnoreChars))
        {
            randomInput.IgnoreChars = input.IgnoreChars.ToList();
        }

        var password = Provider.MakeRandomString(randomInput);
        var weakLevel = _passwordBookManager.EvaluatePasswordStrength(password);

        return new GenerateRandomPasswordResultDto
        {
            Password = password,
            WeakLevel = weakLevel
        };
    }

    private static RandomCategory MapToRandomCategory(PasswordCharacterType characterTypes)
    {
        RandomCategory category = 0;

        if (characterTypes.HasFlag(PasswordCharacterType.LowercaseLetters))
            category |= RandomCategory.LowercaseLetters;
        if (characterTypes.HasFlag(PasswordCharacterType.UppercaseLetters))
            category |= RandomCategory.UppercaseLetters;
        if (characterTypes.HasFlag(PasswordCharacterType.ArabicNumerals))
            category |= RandomCategory.ArabicNumerals;
        if (characterTypes.HasFlag(PasswordCharacterType.EnglishPunctuation))
            category |= RandomCategory.EnglishPunctuation;

        return category;
    }
}
