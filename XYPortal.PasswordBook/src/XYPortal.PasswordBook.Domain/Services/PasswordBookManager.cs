using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using XYPortal.PasswordBook.AggregateRoots;
using XYPortal.PasswordBook.Entities;
using XYPortal.PasswordBook.Enums;
using XYPortal.PasswordBook.ValueObjects;
using PasswordBookEntity = XYPortal.PasswordBook.AggregateRoots.PasswordBook;
using EntityNotFoundException = Volo.Abp.Domain.Entities.EntityNotFoundException;

namespace XYPortal.PasswordBook.Domain.Services;

/// <summary>
/// PasswordBook Domain Service
/// </summary>
public class PasswordBookManager : DomainService
{
    private readonly IRepository<PasswordBookEntity, Guid> _passwordBookRepository;

    public PasswordBookManager(IRepository<PasswordBookEntity, Guid> passwordBookRepository)
    {
        _passwordBookRepository = passwordBookRepository;
    }

    /// <summary>
    /// Create PasswordBook
    /// </summary>
    public async Task<PasswordBookEntity> CreateAsync(
        Guid ownerId,
        string name,
        string? description,
        PasswordFormatRequirement? passwordFormat = null)
    {
        var passwordBook = new PasswordBookEntity(
            Guid.NewGuid(),
            ownerId,
            name,
            description,
            passwordFormat ?? PasswordFormatRequirement.DefaultGeneral
        );

        await _passwordBookRepository.InsertAsync(passwordBook);
        return passwordBook;
    }

    /// <summary>
    /// Get user's PasswordBook list
    /// </summary>
    public async Task<List<PasswordBookEntity>> GetListByOwnerAsync(Guid ownerId, bool includeDeleted = false)
    {
        var books = await _passwordBookRepository.GetListAsync(x => x.OwnerId == ownerId);
        
        if (!includeDeleted)
        {
            books = books.Where(x => !x.IsDeleted).ToList();
        }

        return books;
    }

    /// <summary>
    /// Get PasswordBook details
    /// </summary>
    public async Task<PasswordBookEntity> GetByIdAsync(Guid id)
    {
        return await _passwordBookRepository.GetAsync(id);
    }

    /// <summary>
    /// Soft Delete PasswordBook
    /// </summary>
    public async Task SoftDeleteAsync(Guid id)
    {
        var passwordBook = await _passwordBookRepository.GetAsync(id);
        passwordBook.SoftDelete();
        await _passwordBookRepository.UpdateAsync(passwordBook);
    }

    /// <summary>
    /// Restore PasswordBook
    /// </summary>
    public async Task RestoreAsync(Guid id)
    {
        var passwordBook = await _passwordBookRepository.FirstOrDefaultAsync(x => x.Id == id);
        
        if (passwordBook == null || !passwordBook.IsDeleted)
        {
            throw new BusinessException("PasswordBook:NotFoundOrNotDeleted");
        }

        passwordBook.Restore();
        await _passwordBookRepository.UpdateAsync(passwordBook);
    }

    /// <summary>
    /// Hard Delete PasswordBook
    /// </summary>
    public async Task HardDeleteAsync(Guid id)
    {
        var passwordBook = await _passwordBookRepository.FirstOrDefaultAsync(x => x.Id == id);
        
        if (passwordBook == null)
        {
            throw new EntityNotFoundException("PasswordBook not found");
        }

        passwordBook.HardDelete();
        await _passwordBookRepository.DeleteAsync(passwordBook);
    }

    /// <summary>
    /// Evaluate Password Strength
    /// </summary>
    public PasswordWeakLevel EvaluatePasswordStrength(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return PasswordWeakLevel.VeryWeak;

        int score = 0;

        if (password.Length >= 8) score++;
        if (password.Length >= 12) score++;
        if (password.Length >= 16) score++;

        if (Regex.IsMatch(password, "[a-z]")) score++;
        if (Regex.IsMatch(password, "[A-Z]")) score++;
        if (Regex.IsMatch(password, "[0-9]")) score++;
        if (Regex.IsMatch(password, "[!@#$%^&*()_+\\-=\\[\\]{}|;:,.<>?]")) score++;

        if (Regex.IsMatch(password, @"(.)\1{2,}")) score--;
        if (Regex.IsMatch(password, @"(012|123|234|345|456|567|678|789|890)")) score--;

        return score switch
        {
            <= 2 => PasswordWeakLevel.VeryWeak,
            3 => PasswordWeakLevel.Weak,
            4 or 5 => PasswordWeakLevel.Medium,
            6 or 7 => PasswordWeakLevel.Strong,
            _ => PasswordWeakLevel.VeryStrong
        };
    }

    /// <summary>
    /// Check if user has permission to access PasswordBook
    /// </summary>
    public async Task<bool> HasAccessPermissionAsync(Guid userId, Guid passwordBookId)
    {
        var passwordBook = await _passwordBookRepository.GetAsync(passwordBookId);
        return passwordBook.OwnerId == userId;
    }
}
