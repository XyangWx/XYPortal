namespace XYPortal.EvGRPC;

public static class EvGRPCDbProperties
{
    public static string DbTablePrefix { get; set; } = "EvGRPC";

    public static string? DbSchema { get; set; } = null;

    public const string ConnectionStringName = "EvGRPC";
}
