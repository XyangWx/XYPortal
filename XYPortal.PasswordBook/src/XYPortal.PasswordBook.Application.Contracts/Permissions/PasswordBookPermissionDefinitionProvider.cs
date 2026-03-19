using XYPortal.PasswordBook.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace XYPortal.PasswordBook.Permissions;

public class PasswordBookPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var passwordBookGroup = context.AddGroup(PasswordBookPermissions.GroupName, L("Permission:PasswordBook"));

        // 使用密码本功能权限
        var passwordBookUserPermission = passwordBookGroup.AddPermission(
            PasswordBookPermissions.PassWordBookUser,
            L("Permission:PasswordBookUser")
        );

        // 管理权限
        var managePermission = passwordBookGroup.AddPermission(
            PasswordBookPermissions.Manage,
            L("Permission:PasswordBookManage")
        );
        managePermission.AddChild(PasswordBookPermissions.Create, L("Permission:Create"));
        managePermission.AddChild(PasswordBookPermissions.Update, L("Permission:Update"));
        managePermission.AddChild(PasswordBookPermissions.Delete, L("Permission:Delete"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<PasswordBookResource>(name);
    }
}

public static class PasswordBookPermissions
{
    public const string GroupName = "PasswordBook";

    /// <summary>
    /// 使用密码本功能（PASSWORDBOOKUSER权限）
    /// </summary>
    public const string PassWordBookUser = "PasswordBook.User";

    /// <summary>
    /// 管理密码本
    /// </summary>
    public const string Manage = "PasswordBook.Manage";

    /// <summary>
    /// 创建密码本
    /// </summary>
    public const string Create = "PasswordBook.Manage.Create";

    /// <summary>
    /// 更新密码本
    /// </summary>
    public const string Update = "PasswordBook.Manage.Update";

    /// <summary>
    /// 删除密码本
    /// </summary>
    public const string Delete = "PasswordBook.Manage.Delete";
}
