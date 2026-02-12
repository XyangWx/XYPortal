using System.Threading.Tasks;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Identity;
using Volo.Abp.PermissionManagement;
using Volo.Abp.Uow;
using XYPortal.LinkBoard.Permissions;

namespace XYPortal.LinkBoard.Data;

public class LinkBoardPermissionDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private readonly IPermissionDataSeeder _permissionDataSeeder;

    public LinkBoardPermissionDataSeedContributor(IPermissionDataSeeder permissionDataSeeder)
    {
        _permissionDataSeeder = permissionDataSeeder;
    }

    [UnitOfWork]
    public virtual async Task SeedAsync(DataSeedContext context)
    {
        // Grant all LinkBoard permissions to admin role
        var permissions = LinkBoardPermissions.GetAll();

        await _permissionDataSeeder.SeedAsync(
            RolePermissionValueProvider.ProviderName,
            "admin",
            permissions,
            context.TenantId
        );
    }
}
