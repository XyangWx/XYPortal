using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using XYPortal.LinkBoard.Entities;
using XYPortal.LinkBoard.Permissions;
using XYPortal.LinkBoard.Repositories;

namespace XYPortal.LinkBoard.LinkCategories;

[Authorize(LinkBoardPermissions.LinkCategoryManager)]
public class LinkCategoryAppService : LinkBoardAppService, ILinkCategoryAppService
{
    private readonly ILinkCategoryRepository _categoryRepository;
    private readonly ILogger<LinkCategoryAppService> _logger;

    public LinkCategoryAppService(ILinkCategoryRepository categoryRepository, ILogger<LinkCategoryAppService> logger)
    {
        _categoryRepository = categoryRepository;
        _logger = logger;
    }

    public virtual async Task<LinkCategoryDto> GetAsync(Guid id)
    {
        var entity = await _categoryRepository.GetAsync(id);
        EnsureReadAccess(entity);
        return MapToDto(entity);
    }

    public virtual async Task<PagedResultDto<LinkCategoryDto>> GetListAsync(GetLinkCategoryListInput input)
    {
        var input_string = JsonSerializer.Serialize(
            new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                IncludeFields = true,
            });
        
        _logger.LogDebug($"Input => {input_string}");
        
        var isAdmin = await IsAdminAsync();

        var totalCount = await _categoryRepository.GetCountAsync(
            input.Filter, input.Status, input.IsPublic,
            CurrentUser.Id, isAdmin);

        var items = await _categoryRepository.GetListAsync(
            input.Filter, input.Status, input.IsPublic,
            CurrentUser.Id, isAdmin,
            input.Sorting ?? nameof(LinkCategory.SortOrder),
            input.SkipCount, input.MaxResultCount);

        return new PagedResultDto<LinkCategoryDto>(
            totalCount,
            items.Select(MapToDto).ToList());
    }

    [AllowAnonymous]
    public virtual async Task<List<LinkCategoryDto>> GetPublicListAsync()
    {
        var items = await _categoryRepository.GetListAsync(
            filter: null,
            status: ReviewStatus.Approved,
            isPublic: true,
            currentUserId: null,
            isAdmin: true,
            sorting: nameof(LinkCategory.SortOrder),
            skipCount: 0,
            maxResultCount: 100);

        return items.Select(MapToDto).ToList();
    }

    [Authorize(LinkBoardPermissions.LinkCategoryCreate)]
    public virtual async Task<LinkCategoryDto> CreateAsync(CreateLinkCategoryDto input)
    {
        // Check duplicate name
        var existing = await _categoryRepository.FindByNameAsync(input.Name);
        if (existing != null)
        {
            throw new BusinessException(LinkBoardErrorCodes.DuplicateCategoryName)
                .WithData("Name", input.Name);
        }

        // If public, check against public approved
        if (input.IsPublic)
        {
            var existsPublic = await _categoryRepository.ExistsPublicApprovedByNameAsync(input.Name);
            if (existsPublic)
            {
                throw new BusinessException(LinkBoardErrorCodes.CategoryNameExistsInPublic)
                    .WithData("Name", input.Name);
            }
        }

        var entity = new LinkCategory(GuidGenerator.Create(), input.Name, input.IsPublic)
        {
            DisplayName = input.DisplayName,
            Description = input.Description,
            Icon = input.Icon,
            SortOrder = input.SortOrder
        };

        await _categoryRepository.InsertAsync(entity);
        return MapToDto(entity);
    }

    [Authorize(LinkBoardPermissions.LinkCategoryModify)]
    public virtual async Task<LinkCategoryDto> UpdateAsync(Guid id, UpdateLinkCategoryDto input)
    {
        var entity = await _categoryRepository.GetAsync(id);
        EnsureOwnership(entity);

        // Cannot modify default category
        if (entity.IsDefault)
        {
            throw new BusinessException(LinkBoardErrorCodes.CannotModifyDefaultCategory);
        }

        // If this is an approved public category (not a draft), create/update draft version
        if (entity.IsPublic && entity.Status == ReviewStatus.Approved && entity.DraftOfId == null)
        {
            return await CreateOrUpdateDraftAsync(entity, input);
        }

        // Otherwise, update directly (draft, private, or non-approved public)
        entity.DisplayName = input.DisplayName;
        entity.Description = input.Description;
        entity.Icon = input.Icon;
        entity.SortOrder = input.SortOrder;

        await _categoryRepository.UpdateAsync(entity);
        return MapToDto(entity);
    }

    [Authorize(LinkBoardPermissions.LinkCategoryDelete)]
    public virtual async Task DeleteAsync(Guid id)
    {
        var entity = await _categoryRepository.GetAsync(id);
        EnsureOwnership(entity);

        // Cannot delete default category
        if (entity.IsDefault)
        {
            throw new BusinessException(LinkBoardErrorCodes.CannotDeleteDefaultCategory);
        }

        // If deleting a draft version, just delete it
        if (entity.DraftOfId != null)
        {
            await _categoryRepository.DeleteAsync(entity);
            return;
        }

        // Check for links before deleting
        var hasLinks = await _categoryRepository.HasLinksAsync(id);
        if (hasLinks)
        {
            throw new BusinessException(LinkBoardErrorCodes.CategoryHasLinks);
        }

        // Also delete any draft versions of this category
        var draft = await _categoryRepository.FindDraftByOriginalIdAsync(id);
        if (draft != null)
        {
            await _categoryRepository.DeleteAsync(draft);
        }

        await _categoryRepository.DeleteAsync(entity);
    }

    [Authorize(LinkBoardPermissions.LinkCategorySubmit)]
    public virtual async Task SubmitAsync(Guid id)
    {
        var entity = await _categoryRepository.GetAsync(id);
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
        await _categoryRepository.UpdateAsync(entity);
    }

    public virtual async Task WithdrawAsync(Guid id)
    {
        var entity = await _categoryRepository.GetAsync(id);
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
        await _categoryRepository.UpdateAsync(entity);
    }

    private async Task<LinkCategoryDto> CreateOrUpdateDraftAsync(LinkCategory original, UpdateLinkCategoryDto input)
    {
        var draft = await _categoryRepository.FindDraftByOriginalIdAsync(original.Id);

        if (draft != null)
        {
            // Update existing draft
            draft.DisplayName = input.DisplayName;
            draft.Description = input.Description;
            draft.Icon = input.Icon;
            draft.SortOrder = input.SortOrder;
            await _categoryRepository.UpdateAsync(draft);
            return MapToDto(draft);
        }

        // Create new draft
        draft = new LinkCategory(GuidGenerator.Create(), original.Name, isPublic: true)
        {
            DisplayName = input.DisplayName,
            Description = input.Description,
            Icon = input.Icon,
            SortOrder = input.SortOrder,
            DraftOfId = original.Id,
            Status = ReviewStatus.Draft
        };

        await _categoryRepository.InsertAsync(draft);
        return MapToDto(draft);
    }

    private void EnsureReadAccess(LinkCategory entity)
    {
        // Owner can always read their own
        if (entity.CreatorId == CurrentUser.Id)
        {
            return;
        }

        // Others can only see public approved (non-draft)
        if (!entity.IsPublic || entity.Status != ReviewStatus.Approved || entity.DraftOfId != null)
        {
            throw new Volo.Abp.Domain.Entities.EntityNotFoundException(typeof(LinkCategory), entity.Id);
        }
    }

    private void EnsureOwnership(LinkCategory entity)
    {
        if (entity.CreatorId != CurrentUser.Id)
        {
            throw new Volo.Abp.Domain.Entities.EntityNotFoundException(typeof(LinkCategory), entity.Id);
        }
    }

    private async Task<bool> IsAdminAsync()
    {
        return await AuthorizationService.IsGrantedAsync(LinkBoardPermissions.LinkCategoryReview);
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
