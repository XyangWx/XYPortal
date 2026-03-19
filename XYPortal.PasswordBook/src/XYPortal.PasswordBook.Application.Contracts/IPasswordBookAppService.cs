using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using XYPortal.PasswordBook.Application.Contracts.Dtos;
using XYPortal.PasswordBook.Enums;

namespace XYPortal.PasswordBook.Application.Contracts.PasswordBooks;

/// <summary>
/// PasswordBook Application Service Interface
/// </summary>
public interface IPasswordBookAppService : ICrudAppService<PasswordBookDto, Guid, PagedAndSortedResultRequestDto, CreateUpdatePasswordBookDto, CreateUpdatePasswordBookDto>
{
    /// <summary>
    /// Get user's PasswordBook list
    /// </summary>
    Task<ListResultDto<PasswordBookDto>> GetListByOwnerAsync();

    /// <summary>
    /// Get PasswordBook details with entries
    /// </summary>
    Task<PasswordBookDto> GetWithEntriesAsync(Guid id);

    /// <summary>
    /// Add PasswordEntry
    /// </summary>
    Task<PasswordEntryDto> AddPasswordEntryAsync(Guid passwordBookId, CreatePasswordEntryDto input);

    /// <summary>
    /// Update Password
    /// </summary>
    Task UpdatePasswordAsync(Guid passwordBookId, Guid entryId, UpdatePasswordDto input);

    /// <summary>
    /// Delete PasswordEntry (Soft Delete)
    /// </summary>
    Task DeletePasswordEntryAsync(Guid passwordBookId, Guid entryId);

    /// <summary>
    /// Restore PasswordEntry
    /// </summary>
    Task RestorePasswordEntryAsync(Guid passwordBookId, Guid entryId);

    /// <summary>
    /// Evaluate Password Strength
    /// </summary>
    Task<PasswordWeakLevel> EvaluatePasswordStrengthAsync(string password);

    /// <summary>
    /// Soft Delete PasswordBook
    /// </summary>
    new Task DeleteAsync(Guid id);

    /// <summary>
    /// Restore PasswordBook
    /// </summary>
    Task RestoreAsync(Guid id);

    /// <summary>
    /// Hard Delete PasswordBook
    /// </summary>
    Task HardDeleteAsync(Guid id);

    /// <summary>
    /// Generate Random Password
    /// </summary>
    Task<GenerateRandomPasswordResultDto> GenerateRandomPasswordAsync(GenerateRandomPasswordDto input);
}
