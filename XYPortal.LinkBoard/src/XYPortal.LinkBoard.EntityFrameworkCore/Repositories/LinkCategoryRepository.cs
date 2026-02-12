using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using XYPortal.LinkBoard.Entities;
using XYPortal.LinkBoard.EntityFrameworkCore;
using XYPortal.LinkBoard.Repositories;

namespace XYPortal.LinkBoard.EntityFrameworkCore.Repositories;

public class LinkCategoryRepository
    : EfCoreRepository<LinkBoardDbContext, LinkCategory, Guid>, ILinkCategoryRepository
{
    public LinkCategoryRepository(IDbContextProvider<LinkBoardDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
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
        var dbSet = await GetDbSetAsync();
        var query = ApplyFilter(dbSet, filter, status, isPublic, currentUserId, isAdmin);

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
        var dbSet = await GetDbSetAsync();
        var query = ApplyFilter(dbSet, filter, status, isPublic, currentUserId, isAdmin);
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
        bool isAdmin)
    {
        IQueryable<LinkCategory> query = dbSet;

        if (isAdmin)
        {
            // Admin sees all public records (including drafts of approved items)
            query = query.Where(x => x.IsPublic);
        }
        else if (currentUserId.HasValue)
        {
            // User sees own records + public approved (excluding draft versions)
            query = query.Where(x =>
                x.CreatorId == currentUserId.Value ||
                (x.IsPublic && x.Status == ReviewStatus.Approved && x.DraftOfId == null));
        }

        if (status.HasValue)
        {
            query = query.Where(x => x.Status == status.Value);
        }

        if (isPublic.HasValue)
        {
            query = query.Where(x => x.IsPublic == isPublic.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter))
        {
            query = query.Where(x =>
                x.Name.Contains(filter) ||
                (x.DisplayName != null && x.DisplayName.Contains(filter)));
        }

        return query;
    }
}
