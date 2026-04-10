using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
using EntityNotFoundException = Volo.Abp.Domain.Entities.EntityNotFoundException;

namespace XYPortal.PasswordBook.Application.PasswordBooks;

/// <summary>
/// PasswordBook Application Service Implementation
/// </summary>
public class PasswordBookAppService : CrudAppService<PasswordBookEntity, PasswordBookDto, Guid, PagedAndSortedResultRequestDto, CreateUpdatePasswordBookDto, CreateUpdatePasswordBookDto>, IPasswordBookAppService
{
    private readonly PasswordBookManager _passwordBookManager;
    private readonly PermissionChecker _permissionChecker;
    private readonly ILogger<PasswordBookAppService> _logger;
    private static readonly Random _random = new Random(DateTime.Now.Millisecond);

    public PasswordBookAppService(
        IRepository<PasswordBookEntity, Guid> repository,
        PasswordBookManager passwordBookManager,
        PermissionChecker permissionChecker,
        ILogger<PasswordBookAppService> logger)
        : base(repository)
    {
        _passwordBookManager = passwordBookManager;
        _permissionChecker = permissionChecker;
        _logger = logger;
    }

    /// <summary>
    /// Check PASSWORDBOOKUSER permission
    /// </summary>
    private async Task CheckPasswordBookPermissionAsync()
    {
        var hasPermission = await _permissionChecker.IsGrantedAsync(PasswordBookPermissions.PassWordBookUser);
        _logger.LogInformation("[CheckPermission] PassWordBookUser={HasPermission}", hasPermission);
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
        
        // Use repository with Include to load navigation properties
        var query = await Repository.GetQueryableAsync();
        var queryWithIncludes = query
            .Include(x => x.PasswordEntries)
                .ThenInclude(e => e.PasswordHistories)
            .Where(x => x.OwnerId == userId && !x.IsDeleted);
        
        var books = await EntityFrameworkQueryableExtensions.ToListAsync(queryWithIncludes);

        var dtos = ObjectMapper.Map<List<PasswordBookEntity>, List<PasswordBookDto>>(books);
        foreach (var (dto, entity) in dtos.Zip(books))
        {
            dto.PopulateComplexFields(entity);
        }

        return new ListResultDto<PasswordBookDto>(dtos);
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

        // Use repository with Include to load navigation properties
        var query = await Repository.GetQueryableAsync();
        var queryWithIncludes = query
            .Include(x => x.PasswordEntries)
                .ThenInclude(e => e.PasswordHistories)
            .Where(x => x.Id == id);
            
        var passwordBook = await EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(queryWithIncludes);
        
        if (passwordBook == null)
        {
            throw new EntityNotFoundException(typeof(PasswordBookEntity), id);
        }
        
        var dto = ObjectMapper.Map<PasswordBookEntity, PasswordBookDto>(passwordBook);
        dto.PopulateComplexFields(passwordBook);
        return dto;
    }

    /// <summary>
    /// Add PasswordEntry
    /// </summary>
    public async Task<PasswordEntryDto?> AddPasswordEntryAsync(Guid passwordBookId, CreatePasswordEntryDto input)
    {
        var create_payload = JsonSerializer.Serialize(
            input,
            new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            }) ?? "{}";
        
        _logger.LogDebug(create_payload);
        
        await CheckPasswordBookPermissionAsync();

        var userId = CurrentUser.GetId();
        if (!await _passwordBookManager.HasAccessPermissionAsync(userId, passwordBookId))
        {
            throw new UnauthorizedAccessException("You do not have permission to add entries to this PasswordBook");
        }

        var passwordBook = await _passwordBookManager.GetByIdAsync(passwordBookId);
        
        _logger.LogDebug("[AddPasswordEntryAsync] DTO WeakLevel={WeakLevel}, PasswordType={PasswordType}", input.WeakLevel, input.PasswordType);
        var entry = passwordBook?.AddPasswordEntry(
            input.Title,
            input.HasUsername ?? false,
            input.Username,
            input.PasswordType,
            input.WeakLevel,
            input.Password,
            input.Remark
        );

        if (passwordBook != null)
        {
            await Repository.UpdateAsync(passwordBook);


            return ObjectMapper.Map<PasswordEntry, PasswordEntryDto>(entry!);
        }
        else
        {
            return null;
        }
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
        passwordBook?.UpdatePasswordValue(entryId, input.NewPassword);
        
        if (passwordBook != null)
        {
            await Repository.UpdateAsync(passwordBook);
        }
    }

    /// <summary>
    /// Delete PasswordEntry (Soft Delete)
    /// </summary>
    public async Task DeletePasswordEntryAsync(Guid passwordBookId, Guid entryId, int queryKind = 0)
    {
        await CheckPasswordBookPermissionAsync();

        var userId = CurrentUser.GetId();
        if (!await _passwordBookManager.HasAccessPermissionAsync(userId, passwordBookId))
        {
            throw new UnauthorizedAccessException("You do not have permission to delete this password");
        }

        var passwordBook = await _passwordBookManager.GetByIdAsync(passwordBookId);
        passwordBook?.RemovePasswordEntry(entryId, queryKind);
        
        if (passwordBook != null)
        {
            await Repository.UpdateAsync(passwordBook);
        }
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
        passwordBook?.RestorePasswordEntry(entryId);
        
        if (passwordBook != null)
        {
            await Repository.UpdateAsync(passwordBook);
        }
    }

    /// <summary>
    /// Get a single PasswordEntry by ID (includes CurrentPassword)
    /// </summary>
    public async Task<PasswordEntryDto> GetPasswordEntryAsync(Guid passwordBookId, Guid entryId, int queryKind = 0)
    {
        await CheckPasswordBookPermissionAsync();

        var userId = CurrentUser.GetId();
        if (!await _passwordBookManager.HasAccessPermissionAsync(userId, passwordBookId))
        {
            throw new UnauthorizedAccessException("You do not have permission to access this password entry");
        }

        var passwordBook = await _passwordBookManager.GetByIdAsync(passwordBookId);
        if (passwordBook == null)
        {
            throw new EntityNotFoundException("Password book not found");
        }

        PasswordEntry? entry = queryKind switch
        {
            0 => passwordBook.PasswordEntries.FirstOrDefault(e => e.Id == entryId && !e.IsDeleted),
            1 => passwordBook.PasswordEntries.FirstOrDefault(e => e.Id == entryId && e.IsDeleted),
            _ => passwordBook.PasswordEntries.FirstOrDefault(e => e.Id == entryId)
        };
        if (entry == null)
        {
            throw new EntityNotFoundException("Password entry not found");
        }

        return new PasswordEntryDto
        {
            Id = entry.Id,
            PasswordBookId = passwordBookId,
            Title = entry.Title ?? string.Empty,
            HasUsername = entry.HasUsername,
            Username = entry.Username,
            PasswordType = entry.PasswordType,
            WeakLevel = entry.WeakLevel,
            CurrentPassword = entry.CurrentPassword,
            Remark = entry.Remark,
            CreationTime = entry.CreationTime,
            LastModificationTime = entry.LastModificationTime,
            IsDeleted = entry.IsDeleted
        };
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
        _logger.LogInformation("[DeleteAsync] Called with id={Id}", id);

        await CheckPasswordBookPermissionAsync();
        _logger.LogInformation("[DeleteAsync] Permission check passed");

        var userId = CurrentUser.GetId();
        _logger.LogInformation("[DeleteAsync] CurrentUserId={UserId}", userId);

        if (!await _passwordBookManager.HasAccessPermissionAsync(userId, id))
        {
            _logger.LogWarning("[DeleteAsync] Access denied for user={UserId} on PasswordBook={Id}", userId, id);
            throw new UnauthorizedAccessException("You do not have permission to delete this PasswordBook");
        }

        _logger.LogInformation("[DeleteAsync] Calling SoftDeleteAsync for id={Id}", id);
        await _passwordBookManager.SoftDeleteAsync(id);
        _logger.LogInformation("[DeleteAsync] SoftDeleteAsync completed for id={Id}", id);
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

        var dto = ObjectMapper.Map<PasswordBookEntity, PasswordBookDto>(passwordBook);
        dto.PopulateComplexFields(passwordBook);
        return dto;
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
        passwordBook?.UpdateInfo(input.Name, input.Description);

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
        
        passwordBook?.UpdatePasswordFormat(passwordFormat);

        if (passwordBook != null)
        {
            await Repository.UpdateAsync(passwordBook);

            var dto = ObjectMapper.Map<PasswordBookEntity, PasswordBookDto>(passwordBook);
            dto.PopulateComplexFields(passwordBook);
            return dto;
        }

        return null;
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
        //var format = passwordBook?.GetPasswordFormat();

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

    /// <summary>
    /// Generate Random Password from Password Weak Level
    /// </summary>
    public async Task<GenerateRandomPasswordFromWeakLevelResult> GenerateRandomPasswordFromWeakLevelAsync(GenerateRandomPasswordFromWeakLevelDto input)
    {
        // Determine length and character types based on target weak level
        var (length, characterTypes) = GetPasswordParamsForWeakLevel(input.WeakLevel, input.MinLength, input.MaxLength, input.PasswordType);
        
        _logger.LogDebug($"[Random Password Debug] (length) => {length}");
        _logger.LogDebug($"[Random Password Debug] (characterTypes) => {characterTypes})");

        var input_1 = new GenerateRandomPasswordDto
        {
            PasswordBookId = input.PasswordBookId,
            CharacterTypes = characterTypes,
            Length = length,
            IsOnlyOnce = false,
        };

        var output = await GenerateRandomPasswordAsync(input_1);

        return new GenerateRandomPasswordFromWeakLevelResult
        {
            Password = output.Password,
        };
    }

    /// <summary>
    /// Get password parameters (length and character types) for a target weak level
    /// </summary>
    private static (int Length, PasswordCharacterType CharacterTypes) GetPasswordParamsForWeakLevel(
        PasswordWeakLevel targetLevel, int minLength, int maxLength, PasswordType passwordType = PasswordType.General)
    {
        int length = _random.Next(minLength, maxLength + 1);

        switch (passwordType)
        {
            case PasswordType.General:
                return targetLevel switch
                {
                    PasswordWeakLevel.VeryWeak => (
                        Math.Max(4, minLength),
                        PasswordCharacterType.LowercaseLetters | PasswordCharacterType.ArabicNumerals
                    ),
                    PasswordWeakLevel.Weak => (
                        Math.Max(6, minLength),
                        PasswordCharacterType.LowercaseLetters | PasswordCharacterType.ArabicNumerals
                    ),
                    PasswordWeakLevel.Medium => (
                        Math.Max(8, minLength),
                        PasswordCharacterType.LowercaseLetters | PasswordCharacterType.UppercaseLetters | PasswordCharacterType.ArabicNumerals
                    ),
                    PasswordWeakLevel.Strong => (
                        Math.Min(Math.Max(10, minLength), maxLength),
                        PasswordCharacterType.All
                    ),
                    PasswordWeakLevel.VeryStrong => (
                        Math.Min(20, maxLength),
                        PasswordCharacterType.All
                    ),
                    _ => (length, PasswordCharacterType.All)
                };
            case PasswordType.NumericOnly:
                return (length, PasswordCharacterType.ArabicNumerals);
            default:
                throw new ArgumentOutOfRangeException(nameof(passwordType), passwordType, null);
        }
        
    }

    /// <summary>
    /// Check if actual level matches target or is acceptably close
    /// </summary>
    private static bool IsLevelMatchOrClose(PasswordWeakLevel actual, PasswordWeakLevel target)
    {
        // Exact match
        if (actual == target) return true;

        // For VeryStrong, accept Strong as close enough
        if (target == PasswordWeakLevel.VeryStrong && actual == PasswordWeakLevel.Strong) return true;

        // For Strong, accept Medium as close enough (within 1 level)
        if (target == PasswordWeakLevel.Strong && (int)actual >= (int)PasswordWeakLevel.Medium) return true;

        // For Medium, accept Weak as close enough
        if (target == PasswordWeakLevel.Medium && actual >= PasswordWeakLevel.Weak) return true;

        return false;
    }
}
