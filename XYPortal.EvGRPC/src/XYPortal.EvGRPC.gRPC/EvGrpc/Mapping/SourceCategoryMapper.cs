using System;
using XYPortal.EvGRPC.SourceCategories;

namespace XYPortal.EvGRPC.EvGrpc.Mapping;

/// <summary>
/// Bidirectional mapping between the Domain
/// <see cref="SourceCategory"/> entity and the proto-generated
/// <c>Evgrpc.SourceCategory</c> message.
///
/// This mapper is intentionally tiny — SourceCategory has just two
/// scalar fields (id, name) and no behavior. The phase 2.3 mapper
/// fallback pattern (private-ctor + direct field assignment when
/// the public ctor throws) is unnecessary here because the wire
/// fields match the entity shape exactly.
/// </summary>
public static class SourceCategoryMapper
{
    public static SourceCategory ToDomain(this Evgrpc.SourceCategory proto)
    {
        ArgumentNullException.ThrowIfNull(proto);
        return new SourceCategory(proto.Id, proto.Name);
    }

    public static Evgrpc.SourceCategory ToProto(this SourceCategory entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        return new Evgrpc.SourceCategory
        {
            Id = entity.Id,
            Name = entity.Name,
        };
    }
}
