using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using OpenIddict.Abstractions;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.OpenIddict.Applications;
using XYPortal.Permissions;

namespace XYPortal.OpenIddict;

[Authorize(XYPortalPermissions.OpenIdDictApplicationManager)]
public class OpenIddictApplicationAppService : XYPortalAppService, IOpenIddictApplicationAppService
{
    private readonly IOpenIddictApplicationRepository _applicationRepository;
    private readonly IAbpApplicationManager _applicationManager;

    /// <inheritdoc />
    public OpenIddictApplicationAppService(
        IOpenIddictApplicationRepository applicationRepository,
        IAbpApplicationManager applicationManager)
    {
        _applicationRepository = applicationRepository;
        _applicationManager = applicationManager;
    }

    public virtual async Task<OpenIddictApplicationDto> GetAsync(Guid id)
    {
        var entity = await _applicationRepository.GetAsync(id);
        return MapToDto(entity);
    }

    public virtual async Task<PagedResultDto<OpenIddictApplicationDto>> GetListAsync(GetOpenIddictApplicationListInput input)
    {
        var totalCount = await _applicationRepository.GetCountAsync(input.Filter);
        var items = await _applicationRepository.GetListAsync(
            input.Sorting ?? nameof(OpenIddictApplication.ClientId),
            input.SkipCount,
            input.MaxResultCount,
            input.Filter);

        return new PagedResultDto<OpenIddictApplicationDto>(
            totalCount,
            items.Select(MapToDto).ToList());
    }

    [Authorize(XYPortalPermissions.OpenIdDictApplicationCreate)]
    public virtual async Task<OpenIddictApplicationDto> CreateAsync(CreateOpenIddictApplicationDto input)
    {
        ValidateClientTypeAndSecret(input.ClientType, input.ClientSecret);

        var existing = await _applicationRepository.FindByClientIdAsync(input.ClientId);
        if (existing != null)
        {
            throw new BusinessException("XYPortal:DuplicateClientId")
                .WithData("ClientId", input.ClientId);
        }

        var descriptor = BuildApplicationDescriptor(
            clientId: input.ClientId,
            clientType: input.ClientType,
            consentType: input.ConsentType,
            displayName: input.DisplayName,
            clientSecret: input.ClientSecret,
            clientUri: input.ClientUri,
            grantTypes: input.GrantTypes,
            scopes: input.Scopes,
            redirectUris: input.RedirectUris,
            postLogoutRedirectUris: input.PostLogoutRedirectUris);

        await _applicationManager.CreateAsync(descriptor);

        var created = await _applicationRepository.FindByClientIdAsync(input.ClientId);
        return MapToDto(created!);
    }

    [Authorize(XYPortalPermissions.OpenIdDictApplicationEdit)]
    public virtual async Task<OpenIddictApplicationDto> UpdateAsync(Guid id, UpdateOpenIddictApplicationDto input)
    {
        var entity = await _applicationRepository.GetAsync(id);

        ValidateClientTypeAndSecret(input.ClientType, input.ClientSecret);

        var descriptor = BuildApplicationDescriptor(
            clientId: entity.ClientId!,
            clientType: input.ClientType,
            consentType: input.ConsentType,
            displayName: input.DisplayName,
            clientSecret: input.ClientSecret,
            clientUri: input.ClientUri,
            grantTypes: input.GrantTypes,
            scopes: input.Scopes,
            redirectUris: input.RedirectUris,
            postLogoutRedirectUris: input.PostLogoutRedirectUris);

        // Apply descriptor properties to existing entity
        entity.ClientType = descriptor.ClientType;
        entity.ConsentType = descriptor.ConsentType;
        entity.DisplayName = descriptor.DisplayName;
        entity.ClientUri = descriptor.ClientUri;
        entity.Permissions = JsonSerializer.Serialize(descriptor.Permissions.Select(p => p.ToString()));
        entity.RedirectUris = JsonSerializer.Serialize(descriptor.RedirectUris.Select(u => u.ToString().TrimEnd('/')));
        entity.PostLogoutRedirectUris = JsonSerializer.Serialize(descriptor.PostLogoutRedirectUris.Select(u => u.ToString().TrimEnd('/')));

        await _applicationManager.UpdateAsync(entity.ToModel());

        // Re-fetch to get updated data
        entity = await _applicationRepository.GetAsync(id);
        return MapToDto(entity);
    }

    [Authorize(XYPortalPermissions.OpenIdDictApplicationDelete)]
    public virtual async Task DeleteAsync(Guid id)
    {
        var entity = await _applicationRepository.GetAsync(id);
        await _applicationManager.DeleteAsync(entity.ToModel());
    }

    private static void ValidateClientTypeAndSecret(string clientType, string? clientSecret)
    {
        if (!string.IsNullOrEmpty(clientSecret) &&
            string.Equals(clientType, OpenIddictConstants.ClientTypes.Public, StringComparison.OrdinalIgnoreCase))
        {
            throw new BusinessException("XYPortal:NoClientSecretCanBeSetForPublicApplications");
        }

        if (string.IsNullOrEmpty(clientSecret) &&
            string.Equals(clientType, OpenIddictConstants.ClientTypes.Confidential, StringComparison.OrdinalIgnoreCase))
        {
            throw new BusinessException("XYPortal:TheClientSecretIsRequiredForConfidentialApplications");
        }
    }

    private static AbpApplicationDescriptor BuildApplicationDescriptor(
        string clientId,
        string clientType,
        string consentType,
        string displayName,
        string? clientSecret,
        string? clientUri,
        List<string> grantTypes,
        List<string> scopes,
        List<string>? redirectUris,
        List<string>? postLogoutRedirectUris)
    {
        var application = new AbpApplicationDescriptor
        {
            ClientId = clientId,
            ClientType = clientType,
            ClientSecret = clientSecret,
            ConsentType = consentType,
            DisplayName = displayName,
            ClientUri = clientUri,
        };

        // Handle combined AuthorizationCode + Implicit (hybrid flow)
        if (grantTypes.Contains(OpenIddictConstants.GrantTypes.AuthorizationCode) &&
            grantTypes.Contains(OpenIddictConstants.GrantTypes.Implicit))
        {
            application.Permissions.Add(OpenIddictConstants.Permissions.ResponseTypes.CodeIdToken);
            if (string.Equals(clientType, OpenIddictConstants.ClientTypes.Public, StringComparison.OrdinalIgnoreCase))
            {
                application.Permissions.Add(OpenIddictConstants.Permissions.ResponseTypes.CodeIdTokenToken);
                application.Permissions.Add(OpenIddictConstants.Permissions.ResponseTypes.CodeToken);
            }
        }

        // Add EndSession endpoint if redirect URIs are provided
        if (redirectUris is { Count: > 0 } ||
            postLogoutRedirectUris is { Count: > 0 })
        {
            application.Permissions.Add(OpenIddictConstants.Permissions.Endpoints.EndSession);
        }

        var builtInGrantTypes = new[]
        {
            OpenIddictConstants.GrantTypes.Implicit,
            OpenIddictConstants.GrantTypes.Password,
            OpenIddictConstants.GrantTypes.AuthorizationCode,
            OpenIddictConstants.GrantTypes.ClientCredentials,
            OpenIddictConstants.GrantTypes.DeviceCode,
            OpenIddictConstants.GrantTypes.RefreshToken
        };

        foreach (var grantType in grantTypes)
        {
            if (grantType == OpenIddictConstants.GrantTypes.AuthorizationCode)
            {
                application.Permissions.Add(OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode);
                application.Permissions.Add(OpenIddictConstants.Permissions.ResponseTypes.Code);
            }

            if (grantType == OpenIddictConstants.GrantTypes.AuthorizationCode ||
                grantType == OpenIddictConstants.GrantTypes.Implicit)
            {
                application.Permissions.Add(OpenIddictConstants.Permissions.Endpoints.Authorization);
            }

            if (grantType == OpenIddictConstants.GrantTypes.AuthorizationCode ||
                grantType == OpenIddictConstants.GrantTypes.ClientCredentials ||
                grantType == OpenIddictConstants.GrantTypes.Password ||
                grantType == OpenIddictConstants.GrantTypes.RefreshToken ||
                grantType == OpenIddictConstants.GrantTypes.DeviceCode)
            {
                application.Permissions.Add(OpenIddictConstants.Permissions.Endpoints.Token);
                application.Permissions.Add(OpenIddictConstants.Permissions.Endpoints.Revocation);
                application.Permissions.Add(OpenIddictConstants.Permissions.Endpoints.Introspection);
            }

            if (grantType == OpenIddictConstants.GrantTypes.ClientCredentials)
            {
                application.Permissions.Add(OpenIddictConstants.Permissions.GrantTypes.ClientCredentials);
            }

            if (grantType == OpenIddictConstants.GrantTypes.Implicit)
            {
                application.Permissions.Add(OpenIddictConstants.Permissions.GrantTypes.Implicit);
                application.Permissions.Add(OpenIddictConstants.Permissions.ResponseTypes.IdToken);
                if (string.Equals(clientType, OpenIddictConstants.ClientTypes.Public, StringComparison.OrdinalIgnoreCase))
                {
                    application.Permissions.Add(OpenIddictConstants.Permissions.ResponseTypes.IdTokenToken);
                    application.Permissions.Add(OpenIddictConstants.Permissions.ResponseTypes.Token);
                }
            }

            if (grantType == OpenIddictConstants.GrantTypes.Password)
            {
                application.Permissions.Add(OpenIddictConstants.Permissions.GrantTypes.Password);
            }

            if (grantType == OpenIddictConstants.GrantTypes.RefreshToken)
            {
                application.Permissions.Add(OpenIddictConstants.Permissions.GrantTypes.RefreshToken);
            }

            if (grantType == OpenIddictConstants.GrantTypes.DeviceCode)
            {
                application.Permissions.Add(OpenIddictConstants.Permissions.GrantTypes.DeviceCode);
                application.Permissions.Add(OpenIddictConstants.Permissions.Endpoints.DeviceAuthorization);
            }

            if (!builtInGrantTypes.Contains(grantType))
            {
                application.Permissions.Add(OpenIddictConstants.Permissions.Prefixes.GrantType + grantType);
            }
        }

        // Add scope permissions
        var builtInScopes = new[]
        {
            OpenIddictConstants.Permissions.Scopes.Address,
            OpenIddictConstants.Permissions.Scopes.Email,
            OpenIddictConstants.Permissions.Scopes.Phone,
            OpenIddictConstants.Permissions.Scopes.Profile,
            OpenIddictConstants.Permissions.Scopes.Roles
        };

        foreach (var scope in scopes)
        {
            if (builtInScopes.Contains(scope))
            {
                application.Permissions.Add(scope);
            }
            else
            {
                application.Permissions.Add(OpenIddictConstants.Permissions.Prefixes.Scope + scope);
            }
        }

        // Add redirect URIs
        if (redirectUris != null)
        {
            foreach (var uri in redirectUris)
            {
                if (!string.IsNullOrWhiteSpace(uri))
                {
                    if (!Uri.TryCreate(uri, UriKind.Absolute, out var parsedUri) || !parsedUri.IsWellFormedOriginalString())
                    {
                        throw new BusinessException("XYPortal:InvalidRedirectUri")
                            .WithData("Uri", uri);
                    }
                    application.RedirectUris.Add(parsedUri);
                }
            }
        }

        // Add post-logout redirect URIs
        if (postLogoutRedirectUris != null)
        {
            foreach (var uri in postLogoutRedirectUris)
            {
                if (!string.IsNullOrWhiteSpace(uri))
                {
                    if (!Uri.TryCreate(uri, UriKind.Absolute, out var parsedUri) || !parsedUri.IsWellFormedOriginalString())
                    {
                        throw new BusinessException("XYPortal:InvalidPostLogoutRedirectUri")
                            .WithData("Uri", uri);
                    }
                    application.PostLogoutRedirectUris.Add(parsedUri);
                }
            }
        }

        return application;
    }

    private static OpenIddictApplicationDto MapToDto(OpenIddictApplication entity)
    {
        var dto = new OpenIddictApplicationDto
        {
            Id = entity.Id,
            ClientId = entity.ClientId!,
            ClientType = entity.ClientType,
            ConsentType = entity.ConsentType,
            DisplayName = entity.DisplayName,
            ClientUri = entity.ClientUri,
        };

        // Deserialize RedirectUris
        if (!string.IsNullOrWhiteSpace(entity.RedirectUris))
        {
            try
            {
                dto.RedirectUris = JsonSerializer.Deserialize<List<string>>(entity.RedirectUris) ?? [];
            }
            catch
            {
                dto.RedirectUris = [];
            }
        }

        // Deserialize PostLogoutRedirectUris
        if (!string.IsNullOrWhiteSpace(entity.PostLogoutRedirectUris))
        {
            try
            {
                dto.PostLogoutRedirectUris = JsonSerializer.Deserialize<List<string>>(entity.PostLogoutRedirectUris) ?? [];
            }
            catch
            {
                dto.PostLogoutRedirectUris = [];
            }
        }

        // Parse Permissions JSON to extract GrantTypes and Scopes
        if (!string.IsNullOrWhiteSpace(entity.Permissions))
        {
            try
            {
                var permissions = JsonSerializer.Deserialize<List<string>>(entity.Permissions) ?? [];
                dto.GrantTypes = permissions
                    .Where(p => p.StartsWith(OpenIddictConstants.Permissions.Prefixes.GrantType))
                    .Select(p => p[OpenIddictConstants.Permissions.Prefixes.GrantType.Length..])
                    .ToList();
                dto.Scopes = permissions
                    .Where(p => p.StartsWith(OpenIddictConstants.Permissions.Prefixes.Scope))
                    .Select(p => p[OpenIddictConstants.Permissions.Prefixes.Scope.Length..])
                    .ToList();
            }
            catch
            {
                dto.GrantTypes = [];
                dto.Scopes = [];
            }
        }

        return dto;
    }
}
