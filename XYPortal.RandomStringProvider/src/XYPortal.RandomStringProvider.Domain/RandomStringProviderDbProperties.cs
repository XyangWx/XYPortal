namespace XYPortal.RandomStringProvider;

public static class RandomStringProviderDbProperties
{
    public static string DbTablePrefix { get; set; } = "RandomStringProvider";

    public static string? DbSchema { get; set; } = null;

    public const string ConnectionStringName = "RandomStringProvider";
}
