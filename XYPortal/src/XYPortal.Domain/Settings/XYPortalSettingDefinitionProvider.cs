using Volo.Abp.Settings;

namespace XYPortal.Settings;

public class XYPortalSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        //Define your own settings here. Example:
        //context.Add(new SettingDefinition(XYPortalSettings.MySetting1));
    }
}
