using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using XYPortal.LinkBoard.Entities;
using XYPortal.LinkBoard.EntityFrameworkCore;
using XYPortal.LinkBoard.LinkCategories;
using XYPortal.LinkBoard.Repositories;

namespace XYPortal.LinkBoard.EntityFrameworkCore.Repositories;

public class LinkCategoryRepository
    : EfCoreRepository<LinkBoardDbContext, LinkCategory, Guid>, ILinkCategoryRepository
{
    private readonly ILogger<LinkCategoryRepository> _logger;
    
    public LinkCategoryRepository(IDbContextProvider<LinkBoardDbContext> dbContextProvider, ILogger<LinkCategoryRepository> logger)
        : base(dbContextProvider)
    {
        _logger = logger;
    }

    public async Task<LinkCategory?> FindByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet.FirstOrDefaultAsync(x => x.Name == name, cancellationToken);
    }

    public async Task<LinkCategory?> FindPrivateByNameAndCreatorAsync(string name, Guid creatorId, CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet.FirstOrDefaultAsync(
            x => x.Name == name && !x.IsPublic && x.CreatorId == creatorId,
            cancellationToken);
    }

    public async Task<bool> ExistsPublicApprovedByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet.AnyAsync(
            x => x.Name == name && x.IsPublic && x.Status == ReviewStatus.Approved,
            cancellationToken);
    }

    public async Task<LinkCategory?> FindDraftByOriginalIdAsync(Guid originalId, CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet.FirstOrDefaultAsync(x => x.DraftOfId == originalId, cancellationToken);
    }

    public async Task<List<LinkCategory>> GetListAsync(
        string? filter,
        ReviewStatus? status,
        bool? isPublic,
        Guid? currentUserId,
        bool isAdmin,
        string sorting,
        int skipCount,
        int maxResultCount,
        CancellationToken cancellationToken = default)
    {
#if DEBUG
        var objContent = SerializeQuery(status, isPublic, currentUserId, isAdmin);
        
        _logger.LogDebug($"GetListAsync.ApplyFilter: {objContent}");
#endif
        
        var dbSet = await GetDbSetAsync();
        
#if DEBUG
        var query = ApplyFilter(dbSet, filter, status, isPublic, currentUserId, isAdmin, _logger);
#else
        var query = ApplyFilter(dbSet, filter, status, isPublic, currentUserId, isAdmin);
#endif

        return await query
            .OrderBy(string.IsNullOrWhiteSpace(sorting) ? nameof(LinkCategory.SortOrder) : sorting)
            .Skip(skipCount)
            .Take(maxResultCount)
            .ToListAsync(cancellationToken);
    }

    public async Task<long> GetCountAsync(
        string? filter,
        ReviewStatus? status,
        bool? isPublic,
        Guid? currentUserId,
        bool isAdmin,
        CancellationToken cancellationToken = default)
    {
        
        
#if DEBUG
        var objContent = SerializeQuery(status, isPublic, currentUserId, isAdmin);
        
        _logger.LogDebug($"GetCountAsync.ApplyFilter: {objContent}");
#endif
        
        var dbSet = await GetDbSetAsync();
#if DEBUG
        var query = ApplyFilter(dbSet, filter, status, isPublic, currentUserId, isAdmin, _logger);
#else
        var query = ApplyFilter(dbSet, filter, status, isPublic, currentUserId, isAdmin);
#endif
        return await query.LongCountAsync(cancellationToken);
    }

    public async Task<bool> HasLinksAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        var dbContext = await GetDbContextAsync();
        return await dbContext.Links.AnyAsync(x => x.CategoryId == categoryId, cancellationToken);
    }

    private static IQueryable<LinkCategory> ApplyFilter(
        DbSet<LinkCategory> dbSet,
        string? filter,
        ReviewStatus? status,
        bool? isPublic,
        Guid? currentUserId,
        bool isAdmin,
#if DEBUG
        ILogger<LinkCategoryRepository>? logger = null
#endif
        )
    {
        IQueryable<LinkCategory> query = dbSet;

        if (isAdmin)
        {
#if DEBUG
            logger?.LogDebug("ApplyFilter => IsAdmin: {IsAdmin}", isAdmin);
#endif
            // Admin sees all public records (including drafts of approved items)
            query = query.Where(x => x.IsPublic);
        }
        else if (currentUserId.HasValue)
        {
#if DEBUG
            logger?.LogDebug("ApplyFilter => CurrentUserId: {currentUserId}", currentUserId.Value);
#endif
            // User sees own records + public approved (excluding draft versions)
            query = query.Where(x =>
                x.CreatorId == currentUserId.Value ||
                (x.IsPublic && x.Status == ReviewStatus.Approved && x.DraftOfId == null));
        }

        if (status.HasValue)
        {
#if DEBUG
            logger?.LogDebug("ApplyFilter => Status: {status}", status.Value);
#endif
            query = query.Where(x => x.Status == status.Value);
        }

        if (isPublic.HasValue)
        {
#if DEBUG
            logger?.LogDebug("ApplyFilter => IsPublic: {isPublic}", isPublic.Value);
#endif
            query = query.Where(x => x.IsPublic == isPublic.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter))
        {
#if DEBUG
            logger?.LogDebug("ApplyFilter => Filter: {filter}", filter);
#endif
            query = query.Where(x =>
                x.Name.Contains(filter) ||
                (x.DisplayName != null && x.DisplayName.Contains(filter)));
        }

        return query;
    }

#if DEBUG
    private static string SerializeQuery(
        ReviewStatus? status,
        bool? isPublic,
        Guid? currentUserId,
        bool isAdmin)
    {var obj = new
        {
            ReviewStatus = status,
            IsPublic = isPublic,
            CurrentUserId = currentUserId,
            IsAdmin = isAdmin,
        };

        var objContent = JsonSerializer.Serialize(
            obj,
            new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                IncludeFields = true,
            });

        return objContent;
    }
#endif
}
