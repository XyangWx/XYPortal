using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using XYPortal.LinkBoard.Entities;
using XYPortal.LinkBoard.Permissions;
using XYPortal.LinkBoard.Repositories;

namespace XYPortal.LinkBoard.Links;

[Authorize(LinkBoardPermissions.LinkReview)]
public class LinkReviewAppService : LinkBoardAppService, ILinkReviewAppService
{
    private readonly ILinkRepository _linkRepository;

    public LinkReviewAppService(ILinkRepository linkRepository)
    {
        _linkRepository = linkRepository;
    }

    public virtual async Task<PagedResultDto<LinkDto>> GetListAsync(GetLinkListInput input)
    {
        var totalCount = await _linkRepository.GetCountAsync(
            input.CategoryId, input.Filter, input.Status, input.IsPublic,
            currentUserId: null, isAdmin: true);

        var items = await _linkRepository.GetListAsync(
            input.CategoryId, input.Filter, input.Status, input.IsPublic,
            currentUserId: null, isAdmin: true,
            input.Sorting ?? nameof(Link.SortOrder),
            input.SkipCount, input.MaxResultCount);

        return new PagedResultDto<LinkDto>(
            totalCount,
            items.Select(MapToDto).ToList());
    }

    public virtual async Task ReviewAsync(Guid id, ReviewInput input)
    {
        var entity = await _linkRepository.GetAsync(id);

        if (!entity.IsPublic)
        {
            throw new BusinessException(LinkBoardErrorCodes.PrivateNoReview);
        }

        if (entity.Status != ReviewStatus.Pending)
        {
            throw new BusinessException(LinkBoardErrorCodes.InvalidStatusTransition);
        }

        if (input.Status != ReviewStatus.Approved && input.Status != ReviewStatus.Rejected)
        {
            throw new BusinessException(LinkBoardErrorCodes.InvalidStatusTransition);
        }

        if (input.Status == ReviewStatus.Approved)
        {
            await HandleApprovalAsync(entity, input.ReviewComment);
        }
        else
        {
            // Rejected
            entity.Status = ReviewStatus.Rejected;
            entity.ReviewComment = input.ReviewComment;
            await _linkRepository.UpdateAsync(entity);
        }
    }

    private async Task HandleApprovalAsync(Link entity, string? reviewComment)
    {
        if (entity.DraftOfId.HasValue)
        {
            // This is a draft of an approved link - replace the original
            var original = await _linkRepository.FindAsync(entity.DraftOfId.Value);
            if (original != null)
            {
                // Update original with draft's content
                original.CategoryId = entity.CategoryId;
                original.Title = entity.Title;
                original.Url = entity.Url;
                original.Description = entity.Description;
                original.Icon = entity.Icon;
                original.SortOrder = entity.SortOrder;
                original.ReviewComment = reviewComment;
                await _linkRepository.UpdateAsync(original);

                // Delete the draft
                await _linkRepository.DeleteAsync(entity);
            }
            else
            {
                // Original was deleted, promote draft to standalone
                entity.DraftOfId = null;
                entity.Status = ReviewStatus.Approved;
                entity.ReviewComment = reviewComment;
                await _linkRepository.UpdateAsync(entity);
            }
        }
        else
        {
            // Regular approval (new link)
            entity.Status = ReviewStatus.Approved;
            entity.ReviewComment = reviewComment;

            // Remove any private duplicate from the same creator
            if (entity.CreatorId.HasValue)
            {
                var privateLink = await _linkRepository.FindPrivateByUrlAndCreatorAsync(
                    entity.Url, entity.CreatorId.Value);
                if (privateLink != null)
                {
                    await _linkRepository.DeleteAsync(privateLink);
                }
            }

            await _linkRepository.UpdateAsync(entity);
        }
    }

    private static LinkDto MapToDto(Link entity)
    {
        return new LinkDto
        {
            Id = entity.Id,
            CategoryId = entity.CategoryId,
            CategoryName = entity.Category?.Name,
            Title = entity.Title,
            Url = entity.Url,
            Description = entity.Description,
            Icon = entity.Icon,
            SortOrder = entity.SortOrder,
            IsPublic = entity.IsPublic,
            Status = entity.Status,
            ReviewComment = entity.ReviewComment,
            DraftOfId = entity.DraftOfId,
            CreatorId = entity.CreatorId,
            CreationTime = entity.CreationTime
        };
    }
}
