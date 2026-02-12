using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using OpenIddict.Abstractions;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.OpenIddict.Scopes;
using XYPortal.Permissions;

namespace XYPortal.OpenIddict;

[Authorize(XYPortalPermissions.OpenIdDictScopeManager)]
public class OpenIddictScopeAppService : XYPortalAppService, IOpenIddictScopeAppService
{
    private readonly IOpenIddictScopeRepository _scopeRepository;
    private readonly IOpenIddictScopeManager _scopeManager;

    public OpenIddictScopeAppService(
        IOpenIddictScopeRepository scopeRepository,
        IOpenIddictScopeManager scopeManager)
    {
        _scopeRepository = scopeRepository;
        _scopeManager = scopeManager;
    }

    public virtual async Task<OpenIddictScopeDto> GetAsync(Guid id)
    {
        var entity = await _scopeRepository.GetAsync(id);
        return MapToDto(entity);
    }

    public virtual async Task<PagedResultDto<OpenIddictScopeDto>> GetListAsync(GetOpenIddictScopeListInput input)
    {
        var totalCount = await _scopeRepository.GetCountAsync(input.Filter);
        var items = await _scopeRepository.GetListAsync(
            input.Sorting ?? nameof(OpenIddictScope.Name),
            input.SkipCount,
            input.MaxResultCount,
            input.Filter);

        return new PagedResultDto<OpenIddictScopeDto>(
            totalCount,
            items.Select(MapToDto).ToList());
    }

    [Authorize(XYPortalPermissions.OpenIdDictScopeCreate)]
    public virtual async Task<OpenIddictScopeDto> CreateAsync(CreateOpenIddictScopeDto input)
    {
        var existing = await _scopeRepository.FindByNameAsync(input.Name);
        if (existing != null)
        {
            throw new BusinessException("XYPortal:DuplicateScopeName")
                .WithData("Name", input.Name);
        }

        var descriptor = new OpenIddictScopeDescriptor
        {
            Name = input.Name,
            DisplayName = input.DisplayName,
            Description = input.Description,
        };

        foreach (var resource in input.Resources)
        {
            descriptor.Resources.Add(resource);
        }

        await _scopeManager.CreateAsync(descriptor);

        var created = await _scopeRepository.FindByNameAsync(input.Name);
        return MapToDto(created!);
    }

    [Authorize(XYPortalPermissions.OpenIdDictScopeEdit)]
    public virtual async Task<OpenIddictScopeDto> UpdateAsync(Guid id, UpdateOpenIddictScopeDto input)
    {
        var entity = await _scopeRepository.GetAsync(id);

        entity.DisplayName = input.DisplayName;
        entity.Description = input.Description;
        entity.Resources = JsonSerializer.Serialize(input.Resources);

        await _scopeManager.UpdateAsync(entity.ToModel());

        entity = await _scopeRepository.GetAsync(id);
        return MapToDto(entity);
    }

    [Authorize(XYPortalPermissions.OpenIdDictScopeDelete)]
    public virtual async Task DeleteAsync(Guid id)
    {
        var entity = await _scopeRepository.GetAsync(id);
        await _scopeManager.DeleteAsync(entity.ToModel());
    }

    private static OpenIddictScopeDto MapToDto(OpenIddictScope entity)
    {
        var dto = new OpenIddictScopeDto
        {
            Id = entity.Id,
            Name = entity.Name!,
            DisplayName = entity.DisplayName,
            Description = entity.Description,
        };

        if (!string.IsNullOrWhiteSpace(entity.Resources))
        {
            try
            {
                dto.Resources = JsonSerializer.Deserialize<List<string>>(entity.Resources) ?? [];
            }
            catch
            {
                dto.Resources = [];
            }
        }

        return dto;
    }
}
