using XYPortal.EvGRPC.Chargings;

namespace XYPortal.EvGRPC.EvGrpc.Mapping;

/// <summary>
/// Domain ↔ proto mapping for <see cref="ChargerType"/>. Lives in
/// the gRPC project so Domain stays free of any proto / WellKnownTypes
/// dependency (constraint C-2 from the brainstorm).
/// </summary>
public static class ChargerTypeMapper
{
    public static Evgrpc.ChargerType ToProto(this ChargerType value) => value switch
    {
        ChargerType.Fast       => Evgrpc.ChargerType.Fast,
        ChargerType.Slow       => Evgrpc.ChargerType.Slow,
        ChargerType.Unspecified => Evgrpc.ChargerType.Unspecified,
        _                       => Evgrpc.ChargerType.Unspecified,
    };

    public static ChargerType ToDomain(this Evgrpc.ChargerType value) => value switch
    {
        Evgrpc.ChargerType.Fast        => ChargerType.Fast,
        Evgrpc.ChargerType.Slow        => ChargerType.Slow,
        Evgrpc.ChargerType.Unspecified => ChargerType.Unspecified,
        _                               => ChargerType.Unspecified,
    };
}
