namespace XYPortal.EvGRPC.Chargings;

/// <summary>
/// Maps 1:1 with the proto <c>enum ChargerType</c> in
/// <c>evgrpc/charging.proto</c>. Kept in the Domain layer because
/// "fast vs slow charger" is a domain concept the pricing logic
/// (<see cref="Charging.CostPerKwh"/>) needs.
/// </summary>
public enum ChargerType
{
    Unspecified = 0,
    Fast = 1,
    Slow = 2,
}
