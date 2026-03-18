namespace XYPortal.PasswordBook;

public static class PasswordBookDbProperties
{
    public static string DbTablePrefix { get; set; } = "PasswordBook";

    public static string? DbSchema { get; set; } = null;

    public const string ConnectionStringName = "PasswordBook";
}
