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

public class LinkRepository
    : EfCoreRepository<LinkBoardDbContext, Link, Guid>, ILinkRepository
{
    public LinkRepository(IDbContextProvider<LinkBoardDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task<Link?> FindPrivateByUrlAndCreatorAsync(string url, Guid creatorId, CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet.FirstOrDefaultAsync(
            x => x.Url == url && !x.IsPublic && x.CreatorId == creatorId,
            cancellationToken);
    }

    public async Task<bool> ExistsPublicApprovedByUrlAsync(string url, CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet.AnyAsync(
            x => x.Url == url && x.IsPublic && x.Status == ReviewStatus.Approved,
            cancellationToken);
    }

    public async Task<Link?> FindDraftByOriginalIdAsync(Guid originalId, CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet.FirstOrDefaultAsync(x => x.DraftOfId == originalId, cancellationToken);
    }

    public async Task<List<Link>> GetListAsync(
        Guid? categoryId,
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
        var query = ApplyFilter(dbSet, categoryId, filter, status, isPublic, currentUserId, isAdmin);

        return await query
            .Include(x => x.Category)
            .OrderBy(string.IsNullOrWhiteSpace(sorting) ? nameof(Link.SortOrder) : sorting)
            .Skip(skipCount)
            .Take(maxResultCount)
            .ToListAsync(cancellationToken);
    }

    public async Task<long> GetCountAsync(
        Guid? categoryId,
        string? filter,
        ReviewStatus? status,
        bool? isPublic,
        Guid? currentUserId,
        bool isAdmin,
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        var query = ApplyFilter(dbSet, categoryId, filter, status, isPublic, currentUserId, isAdmin);
        return await query.LongCountAsync(cancellationToken);
    }

    public async Task<List<Link>> GetPublicBoardListAsync(
        Guid? currentUserId,
        Guid? categoryId,
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();

        // Public approved links (excluding draft versions - DraftOfId must be null)
        var publicQuery = dbSet.Where(x => x.IsPublic && x.Status == ReviewStatus.Approved && x.DraftOfId == null);

        if (categoryId.HasValue)
        {
            publicQuery = publicQuery.Where(x => x.CategoryId == categoryId.Value);
        }

        if (!currentUserId.HasValue)
        {
            return await publicQuery
                .Include(x => x.Category)
                .OrderBy(x => x.SortOrder)
                .ToListAsync(cancellationToken);
        }

        // Private links of current user (excluding draft versions)
        var privateQuery = dbSet.Where(x => !x.IsPublic && x.CreatorId == currentUserId.Value && x.DraftOfId == null);

        if (categoryId.HasValue)
        {
            privateQuery = privateQuery.Where(x => x.CategoryId == categoryId.Value);
        }

        // Get public URLs to exclude from private
        var publicUrls = publicQuery.Select(x => x.Url);
        privateQuery = privateQuery.Where(x => !publicUrls.Contains(x.Url));

        // Union public + deduplicated private
        var result = await publicQuery
            .Union(privateQuery)
            .Include(x => x.Category)
            .OrderBy(x => x.SortOrder)
            .ToListAsync(cancellationToken);

        return result;
    }

    private static IQueryable<Link> ApplyFilter(
        DbSet<Link> dbSet,
        Guid? categoryId,
        string? filter,
        ReviewStatus? status,
        bool? isPublic,
        Guid? currentUserId,
        bool isAdmin)
    {
        IQueryable<Link> query = dbSet;

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

        if (categoryId.HasValue)
        {
            query = query.Where(x => x.CategoryId == categoryId.Value);
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
                x.Title.Contains(filter) ||
                x.Url.Contains(filter));
        }

        return query;
    }
}
