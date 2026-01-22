using Volo.Abp.Features;
using Volo.Abp.Localization;
using Volo.Abp.Validation.StringValues;
using XYPortal.Localization;

namespace XYPortal.Features;

public class XYPortalFeatureDefinitionProvider : FeatureDefinitionProvider
{
    public override void Define(IFeatureDefinitionContext context)
    {
        var group = context.AddGroup(
            XYPortalFeatures.GroupName,
            L("Feature:XYPortal"));

        var testFeature = group.AddFeature(
            XYPortalFeatures.Test.Default,
            defaultValue: "false",
            displayName: L("Feature:Test"),
            description: L("Feature:TestDescription"),
            valueType: new ToggleStringValueType()
        );

        testFeature.CreateChild(
            XYPortalFeatures.Test.Enable,
            defaultValue: "false",
            displayName: L("Feature:TestEnable"),
            description: L("Feature:TestEnableDescription"),
            valueType: new ToggleStringValueType()
        );

        testFeature.CreateChild(
            XYPortalFeatures.Test.NumbValue,
            defaultValue: "0",
            displayName: L("Feature:TestNumbValue"),
            description: L("Feature:TestNumbValueDescription"),
            valueType: new FreeTextStringValueType(new NumericValueValidator(0, 1000000))
        );

        testFeature.CreateChild(
            XYPortalFeatures.Test.StringValue,
            defaultValue: "",
            displayName: L("Feature:TestStringValue"),
            description: L("Feature:TestStringValueDescription"),
            valueType: new FreeTextStringValueType()
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<XYPortalResource>(name);
    }
}
