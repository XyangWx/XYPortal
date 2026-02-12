namespace XYPortal.LinkBoard;

public static class LinkBoardDbProperties
{
    public static string DbTablePrefix { get; set; } = "LinkBoard";

    public static string? DbSchema { get; set; } = null;

    public const string ConnectionStringName = "LinkBoard";
}
