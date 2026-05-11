using XYPortal.RandomStringProvider.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace XYPortal.RandomStringProvider.Permissions;

public class RandomStringProviderPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(RandomStringProviderPermissions.GroupName, L("Permission:RandomStringProvider"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<RandomStringProviderResource>(name);
    }
}
