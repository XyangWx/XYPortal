namespace XYPortal.Features;

public static class XYPortalFeatures
{
    public const string GroupName = "XYPortal";

    public static class Test
    {
        public const string Default = GroupName + ".Test";
        public const string Enable = Default + ".Enable";
        public const string NumbValue = Default + ".NumbValue";
        public const string StringValue = Default + ".StringValue";
    }

	public static class Test_1
	{
		public const string Default = GroupName + ".Test_1";
		public const string Enable = Default + ".Enable";
		public const string NumbValue = Default + ".NumbValue";
		public const string StringValue = Default + ".StringValue";
        public const string BooleanValue = Default + ".BoleanValue";
	}
}
