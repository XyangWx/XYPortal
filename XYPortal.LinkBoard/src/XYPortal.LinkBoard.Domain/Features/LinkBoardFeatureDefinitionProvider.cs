using Volo.Abp.Features;
using Volo.Abp.Localization;
using Volo.Abp.Validation.StringValues;
using XYPortal.LinkBoard.Features;
using XYPortal.LinkBoard.Localization;

namespace XYPortal.LinkBoard;

public class LinkBoardFeatureDefinitionProvider : FeatureDefinitionProvider
{
    public override void Define(IFeatureDefinitionContext context)
    {
        var group = context.AddGroup(
            LinkBoardFeatures.GroupName,
            L("Feature:LinkBoard"));

        group.AddFeature(
            LinkBoardFeatures.MaxLinks,
            defaultValue: "15",
            displayName: L("Feature:LinkBoard.MaxLinks"),
            description: L("Feature:LinkBoard.MaxLinksDescription"),
            valueType: new FreeTextStringValueType(new NumericValueValidator(1, 1000))
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<LinkBoardResource>(name);
    }
}
