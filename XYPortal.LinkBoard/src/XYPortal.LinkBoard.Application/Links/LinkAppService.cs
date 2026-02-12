using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using XYPortal.LinkBoard.Entities;
using XYPortal.LinkBoard.Permissions;
using XYPortal.LinkBoard.Repositories;

namespace XYPortal.LinkBoard.Links;

[Authorize(LinkBoardPermissions.LinkManager)]
public class LinkAppService : LinkBoardAppService, ILinkAppService
{
    private readonly ILinkRepository _linkRepository;
    private readonly ILinkCategoryRepository _categoryRepository;

    public LinkAppService(
        ILinkRepository linkRepository,
        ILinkCategoryRepository categoryRepository)
    {
        _linkRepository = linkRepository;
        _categoryRepository = categoryRepository;
    }

    public virtual async Task<LinkDto> GetAsync(Guid id)
    {
        var entity = await _linkRepository.GetAsync(id);
        EnsureReadAccess(entity);
        return MapToDto(entity);
    }

    public virtual async Task<PagedResultDto<LinkDto>> GetListAsync(GetLinkListInput input)
    {
        var isAdmin = await IsAdminAsync();

        var totalCount = await _linkRepository.GetCountAsync(
            input.CategoryId, input.Filter, input.Status, input.IsPublic,
            CurrentUser.Id, isAdmin);

        var items = await _linkRepository.GetListAsync(
            input.CategoryId, input.Filter, input.Status, input.IsPublic,
            CurrentUser.Id, isAdmin,
            input.Sorting ?? nameof(Link.SortOrder),
            input.SkipCount, input.MaxResultCount);

        return new PagedResultDto<LinkDto>(
            totalCount,
            items.Select(MapToDto).ToList());
    }

    [Authorize(LinkBoardPermissions.LinkCreate)]
    public virtual async Task<LinkDto> CreateAsync(CreateLinkDto input)
    {
        // Validate category exists
        var category = await _categoryRepository.GetAsync(input.CategoryId);

        // If creating a public link, category must be public and approved
        if (input.IsPublic && (!category.IsPublic || category.Status != ReviewStatus.Approved))
        {
            throw new BusinessException(LinkBoardErrorCodes.CategoryNotApproved);
        }

        // If public, check URL doesn't already exist in public approved
        if (input.IsPublic)
        {
            var existsPublic = await _linkRepository.ExistsPublicApprovedByUrlAsync(input.Url);
            if (existsPublic)
            {
                throw new BusinessException(LinkBoardErrorCodes.UrlExistsInPublic)
                    .WithData("Url", input.Url);
            }
        }

        var entity = new Link(GuidGenerator.Create(), input.CategoryId, input.Title, input.Url, input.IsPublic)
        {
            Description = input.Description,
            Icon = input.Icon,
            SortOrder = input.SortOrder
        };

        await _linkRepository.InsertAsync(entity);
        return MapToDto(entity);
    }

    [Authorize(LinkBoardPermissions.LinkModify)]
    public virtual async Task<LinkDto> UpdateAsync(Guid id, UpdateLinkDto input)
    {
        var entity = await _linkRepository.GetAsync(id);
        EnsureOwnership(entity);

        // Validate category exists
        await _categoryRepository.GetAsync(input.CategoryId);

        // If this is an approved public link (not a draft), create/update draft version
        if (entity.IsPublic && entity.Status == ReviewStatus.Approved && entity.DraftOfId == null)
        {
            return await CreateOrUpdateDraftAsync(entity, input);
        }

        // Otherwise, update directly (draft, private, or non-approved public)
        entity.CategoryId = input.CategoryId;
        entity.Title = input.Title;
        entity.Url = input.Url;
        entity.Description = input.Description;
        entity.Icon = input.Icon;
        entity.SortOrder = input.SortOrder;

        // If changing to public, validate
        if (input.IsPublic && !entity.IsPublic)
        {
            entity.IsPublic = true;
            entity.Status = ReviewStatus.Draft;
        }

        await _linkRepository.UpdateAsync(entity);
        return MapToDto(entity);
    }

    [Authorize(LinkBoardPermissions.LinkDelete)]
    public virtual async Task DeleteAsync(Guid id)
    {
        var entity = await _linkRepository.GetAsync(id);
        EnsureOwnership(entity);

        // If deleting a draft version, just delete it
        if (entity.DraftOfId != null)
        {
            await _linkRepository.DeleteAsync(entity);
            return;
        }

        // Also delete any draft versions of this link
        var draft = await _linkRepository.FindDraftByOriginalIdAsync(id);
        if (draft != null)
        {
            await _linkRepository.DeleteAsync(draft);
        }

        await _linkRepository.DeleteAsync(entity);
    }

    public virtual async Task SubmitAsync(Guid id)
    {
        var entity = await _linkRepository.GetAsync(id);
        EnsureOwnership(entity);

        if (!entity.IsPublic)
        {
            throw new BusinessException(LinkBoardErrorCodes.PrivateNoReview);
        }

        if (entity.Status != ReviewStatus.Draft && entity.Status != ReviewStatus.Rejected)
        {
            throw new BusinessException(LinkBoardErrorCodes.InvalidStatusTransition);
        }

        entity.Status = ReviewStatus.Pending;
        entity.ReviewComment = null;
        await _linkRepository.UpdateAsync(entity);
    }

    public virtual async Task WithdrawAsync(Guid id)
    {
        var entity = await _linkRepository.GetAsync(id);
        EnsureOwnership(entity);

        if (!entity.IsPublic)
        {
            throw new BusinessException(LinkBoardErrorCodes.PrivateNoReview);
        }

        if (entity.Status != ReviewStatus.Pending)
        {
            throw new BusinessException(LinkBoardErrorCodes.InvalidStatusTransition);
        }

        entity.Status = ReviewStatus.Draft;
        await _linkRepository.UpdateAsync(entity);
    }

    [AllowAnonymous]
    public virtual async Task<List<LinkDto>> GetPublicBoardAsync(GetPublicBoardInput input)
    {
        var items = await _linkRepository.GetPublicBoardListAsync(CurrentUser.Id, input.CategoryId);
        return items.Select(MapToDto).ToList();
    }

    private async Task<LinkDto> CreateOrUpdateDraftAsync(Link original, UpdateLinkDto input)
    {
        var draft = await _linkRepository.FindDraftByOriginalIdAsync(original.Id);

        if (draft != null)
        {
            // Update existing draft
            draft.CategoryId = input.CategoryId;
            draft.Title = input.Title;
            draft.Url = input.Url;
            draft.Description = input.Description;
            draft.Icon = input.Icon;
            draft.SortOrder = input.SortOrder;
            await _linkRepository.UpdateAsync(draft);
            return MapToDto(draft);
        }

        // Create new draft
        draft = new Link(GuidGenerator.Create(), input.CategoryId, input.Title, input.Url, isPublic: true)
        {
            Description = input.Description,
            Icon = input.Icon,
            SortOrder = input.SortOrder,
            DraftOfId = original.Id,
            Status = ReviewStatus.Draft
        };

        await _linkRepository.InsertAsync(draft);
        return MapToDto(draft);
    }

    private void EnsureReadAccess(Link entity)
    {
        if (entity.CreatorId == CurrentUser.Id)
        {
            return;
        }

        // Others can only see public approved (non-draft)
        if (!entity.IsPublic || entity.Status != ReviewStatus.Approved || entity.DraftOfId != null)
        {
            throw new Volo.Abp.Domain.Entities.EntityNotFoundException(typeof(Link), entity.Id);
        }
    }

    private void EnsureOwnership(Link entity)
    {
        if (entity.CreatorId != CurrentUser.Id)
        {
            throw new Volo.Abp.Domain.Entities.EntityNotFoundException(typeof(Link), entity.Id);
        }
    }

    private async Task<bool> IsAdminAsync()
    {
        return await AuthorizationService.IsGrantedAsync(LinkBoardPermissions.LinkReview);
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
