using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Application.Dtos;
using XYPortal.PasswordBook.Application.Contracts.Dtos;
using XYPortal.PasswordBook.Application.Contracts.PasswordBooks;
using XYPortal.PasswordBook.Enums;

namespace XYPortal.PasswordBook.HttpApi.Controllers;

[Area(PasswordBookRemoteServiceConsts.ModuleName)]
[RemoteService(Name = PasswordBookRemoteServiceConsts.RemoteServiceName)]
[Route("api/password-book")]
public class PasswordBookController : AbpControllerBase, IPasswordBookAppService
{
    private readonly IPasswordBookAppService _passwordBookAppService;

    public PasswordBookController(IPasswordBookAppService passwordBookAppService)
    {
        _passwordBookAppService = passwordBookAppService;
    }

    [HttpGet]
    public Task<ListResultDto<PasswordBookDto>> GetListByOwnerAsync()
    {
        return _passwordBookAppService.GetListByOwnerAsync();
    }

    [HttpGet]
    [Route("{id}")]
    public Task<PasswordBookDto> GetAsync(Guid id)
    {
        return _passwordBookAppService.GetAsync(id);
    }

    [HttpGet]
    [Route("all")]
    public Task<PagedResultDto<PasswordBookDto>> GetListAsync(PagedAndSortedResultRequestDto input)
    {
        return _passwordBookAppService.GetListAsync(input);
    }

    [HttpGet]
    [Route("{id}/with-entries")]
    public Task<PasswordBookDto> GetWithEntriesAsync(Guid id)
    {
        return _passwordBookAppService.GetWithEntriesAsync(id);
    }

    [HttpPost]
    public Task<PasswordBookDto> CreateAsync(CreateUpdatePasswordBookDto input)
    {
        return _passwordBookAppService.CreateAsync(input);
    }

    [HttpPut]
    [Route("{id}")]
    public Task<PasswordBookDto> UpdateAsync(Guid id, CreateUpdatePasswordBookDto input)
    {
        return _passwordBookAppService.UpdateAsync(id, input);
    }

    [HttpDelete]
    [Route("{id}")]
    public Task DeleteAsync(Guid id)
    {
        return _passwordBookAppService.DeleteAsync(id);
    }

    [HttpPost]
    [Route("{passwordBookId}/restore/{id}")]
    public Task RestoreAsync(Guid id)
    {
        return _passwordBookAppService.RestoreAsync(id);
    }

    [HttpDelete]
    [Route("{id}/hard")]
    public Task HardDeleteAsync(Guid id)
    {
        return _passwordBookAppService.HardDeleteAsync(id);
    }

    [HttpPost]
    [Route("{passwordBookId}/entries")]
    public Task<PasswordEntryDto> AddPasswordEntryAsync(Guid passwordBookId, CreatePasswordEntryDto input)
    {
        return _passwordBookAppService.AddPasswordEntryAsync(passwordBookId, input);
    }

    [HttpPut]
    [Route("{passwordBookId}/entries/{entryId}/password")]
    public Task UpdatePasswordAsync(Guid passwordBookId, Guid entryId, UpdatePasswordDto input)
    {
        return _passwordBookAppService.UpdatePasswordAsync(passwordBookId, entryId, input);
    }

    [HttpDelete]
    [Route("{passwordBookId}/entries/{entryId}")]
    public Task DeletePasswordEntryAsync(Guid passwordBookId, Guid entryId)
    {
        return _passwordBookAppService.DeletePasswordEntryAsync(passwordBookId, entryId);
    }

    [HttpGet]
    [Route("{passwordBookId}/entries/{entryId}")]
    public Task<PasswordEntryDto> GetPasswordEntryAsync(Guid passwordBookId, Guid entryId)
    {
        return _passwordBookAppService.GetPasswordEntryAsync(passwordBookId, entryId);
    }

    [HttpPost]
    [Route("{passwordBookId}/entries/{entryId}/restore")]
    public Task RestorePasswordEntryAsync(Guid passwordBookId, Guid entryId)
    {
        return _passwordBookAppService.RestorePasswordEntryAsync(passwordBookId, entryId);
    }

    [HttpPost]
    [Route("evaluate-strength")]
    public Task<PasswordWeakLevel> EvaluatePasswordStrengthAsync(string password)
    {
        return _passwordBookAppService.EvaluatePasswordStrengthAsync(password);
    }

    [HttpPost]
    [Route("generate-random-password")]
    public Task<GenerateRandomPasswordResultDto> GenerateRandomPasswordAsync(GenerateRandomPasswordDto input)
    {
        return _passwordBookAppService.GenerateRandomPasswordAsync(input);
    }
}
