using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using XYPortal.LinkBoard.Entities;

namespace XYPortal.LinkBoard.Repositories;

public interface ILinkRepository : IRepository<Link, Guid>
{
    Task<Link?> FindPrivateByUrlAndCreatorAsync(string url, Guid creatorId, CancellationToken cancellationToken = default);

    Task<bool> ExistsPublicApprovedByUrlAsync(string url, CancellationToken cancellationToken = default);

    Task<Link?> FindDraftByOriginalIdAsync(Guid originalId, CancellationToken cancellationToken = default);

    Task<List<Link>> GetListAsync(
        Guid? categoryId,
        string? filter,
        ReviewStatus? status,
        bool? isPublic,
        Guid? currentUserId,
        bool isAdmin,
        string sorting,
        int skipCount,
        int maxResultCount,
        CancellationToken cancellationToken = default);

    Task<long> GetCountAsync(
        Guid? categoryId,
        string? filter,
        ReviewStatus? status,
        bool? isPublic,
        Guid? currentUserId,
        bool isAdmin,
        CancellationToken cancellationToken = default);

    Task<List<Link>> GetPublicBoardListAsync(
        Guid? currentUserId,
        Guid? categoryId,
        CancellationToken cancellationToken = default);
}
