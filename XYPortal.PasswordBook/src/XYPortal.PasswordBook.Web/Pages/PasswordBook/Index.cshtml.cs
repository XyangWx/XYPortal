using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XYPortal.PasswordBook.Application.Contracts.Dtos;
using XYPortal.PasswordBook.Application.Contracts.PasswordBooks;
using XYPortal.PasswordBook.Enums;

namespace XYPortal.PasswordBook.Web.Pages.PasswordBook;

[Authorize]
public class IndexModel : PasswordBookPageModel
{
    private readonly IPasswordBookAppService _passwordBookAppService;

    [BindProperty]
    public CreateUpdatePasswordBookDto CreateInput { get; set; } = new() { Name = "" };

    public List<PasswordBookDto> PasswordBooks { get; set; } = new();

    public IndexModel(IPasswordBookAppService passwordBookAppService)
    {
        _passwordBookAppService = passwordBookAppService;
    }

    public async Task OnGetAsync()
    {
        var result = await _passwordBookAppService.GetListByOwnerAsync();
        PasswordBooks = new List<PasswordBookDto>(result.Items);
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        await _passwordBookAppService.CreateAsync(CreateInput);
        return NoContent();
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
    {
        await _passwordBookAppService.DeleteAsync(id);
        return NoContent();
    }
}
