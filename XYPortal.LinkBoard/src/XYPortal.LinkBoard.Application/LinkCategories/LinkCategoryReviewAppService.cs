using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using XYPortal.LinkBoard.Entities;
using XYPortal.LinkBoard.Permissions;
using XYPortal.LinkBoard.Repositories;

namespace XYPortal.LinkBoard.LinkCategories;

[Authorize(LinkBoardPermissions.LinkCategoryReview)]
[Route("/api/app/link-category-review")]
public class LinkCategoryReviewAppService : LinkBoardAppService, ILinkCategoryReviewAppService
{
    private readonly ILinkCategoryRepository _categoryRepository;

    public LinkCategoryReviewAppService(ILinkCategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    [HttpGet]
    public virtual async Task<PagedResultDto<LinkCategoryDto>> GetListAsync(GetLinkCategoryListInput input)
    {
        var totalCount = await _categoryRepository.GetCountAsync(
            input.Filter, input.Status, input.IsPublic,
            currentUserId: null, isAdmin: true);

        var items = await _categoryRepository.GetListAsync(
            input.Filter, input.Status, input.IsPublic,
            currentUserId: null, isAdmin: true,
            input.Sorting ?? nameof(LinkCategory.SortOrder),
            input.SkipCount, input.MaxResultCount);

        return new PagedResultDto<LinkCategoryDto>(
            totalCount,
            items.Select(MapToDto).ToList());
    }

    [HttpPost("{id}/review")]
    public virtual async Task ReviewAsync([FromRoute] Guid id, [FromBody] ReviewInput input)
    {
        var entity = await _categoryRepository.GetAsync(id);

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
            await _categoryRepository.UpdateAsync(entity);
        }
    }

    private async Task HandleApprovalAsync(LinkCategory entity, string? reviewComment)
    {
        if (entity.DraftOfId.HasValue)
        {
            // This is a draft of an approved category - replace the original
            var original = await _categoryRepository.FindAsync(entity.DraftOfId.Value);
            if (original != null)
            {
                // Update original with draft's content
                original.DisplayName = entity.DisplayName;
                original.Description = entity.Description;
                original.Icon = entity.Icon;
                original.SortOrder = entity.SortOrder;
                original.ReviewComment = reviewComment;
                await _categoryRepository.UpdateAsync(original);

                // Delete the draft
                await _categoryRepository.DeleteAsync(entity);
            }
            else
            {
                // Original was deleted, promote draft to standalone
                entity.DraftOfId = null;
                entity.Status = ReviewStatus.Approved;
                entity.ReviewComment = reviewComment;
                await _categoryRepository.UpdateAsync(entity);
            }
        }
        else
        {
            // Regular approval (new category)
            entity.Status = ReviewStatus.Approved;
            entity.ReviewComment = reviewComment;

            // Remove any private duplicate from the same creator
            if (entity.CreatorId.HasValue)
            {
                var privateCategory = await _categoryRepository.FindPrivateByNameAndCreatorAsync(
                    entity.Name, entity.CreatorId.Value);
                if (privateCategory != null)
                {
                    await _categoryRepository.DeleteAsync(privateCategory);
                }
            }

            await _categoryRepository.UpdateAsync(entity);
        }
    }

    private static LinkCategoryDto MapToDto(LinkCategory entity)
    {
        return new LinkCategoryDto
        {
            Id = entity.Id,
            Name = entity.Name,
            DisplayName = entity.DisplayName,
            Description = entity.Description,
            Icon = entity.Icon,
            SortOrder = entity.SortOrder,
            IsPublic = entity.IsPublic,
            Status = entity.Status,
            ReviewComment = entity.ReviewComment,
            DraftOfId = entity.DraftOfId,
            IsDefault = entity.IsDefault,
            CreatorId = entity.CreatorId,
            CreationTime = entity.CreationTime
        };
    }
}
