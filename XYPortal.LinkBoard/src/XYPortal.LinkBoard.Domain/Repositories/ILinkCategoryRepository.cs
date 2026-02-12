using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using XYPortal.LinkBoard.Entities;

namespace XYPortal.LinkBoard.Repositories;

public interface ILinkCategoryRepository : IRepository<LinkCategory, Guid>
{
    Task<LinkCategory?> FindByNameAsync(string name, CancellationToken cancellationToken = default);

    Task<LinkCategory?> FindPrivateByNameAndCreatorAsync(string name, Guid creatorId, CancellationToken cancellationToken = default);

    Task<bool> ExistsPublicApprovedByNameAsync(string name, CancellationToken cancellationToken = default);

    Task<LinkCategory?> FindDraftByOriginalIdAsync(Guid originalId, CancellationToken cancellationToken = default);

    Task<List<LinkCategory>> GetListAsync(
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
        string? filter,
        ReviewStatus? status,
        bool? isPublic,
        Guid? currentUserId,
        bool isAdmin,
        CancellationToken cancellationToken = default);

    Task<bool> HasLinksAsync(Guid categoryId, CancellationToken cancellationToken = default);
}
