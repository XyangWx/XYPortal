using System;
using Google.Protobuf.WellKnownTypes;
using Shouldly;
using Xunit;
using XYPortal.EvGRPC.EvGrpc.Mapping;
using DomainSourceCategory = XYPortal.EvGRPC.SourceCategories.SourceCategory;

namespace XYPortal.EvGRPC.Mapping;

public class SourceCategoryMapper_Tests
{
    [Fact]
    public void ToDomain_round_trips_id_and_name()
    {
        var proto = new Evgrpc.SourceCategory { Id = "cat-1", Name = "home" };
        var domain = proto.ToDomain();
        domain.Id.ShouldBe("cat-1");
        domain.Name.ShouldBe("home");

        var proto2 = domain.ToProto();
        proto2.Id.ShouldBe("cat-1");
        proto2.Name.ShouldBe("home");
        proto2.Equals(proto).ShouldBeTrue();
    }

    [Fact]
    public void ToDomain_rejects_blank_id_at_entity_layer()
    {
        var proto = new Evgrpc.SourceCategory { Id = "", Name = "x" };
        Should.Throw<ArgumentException>(() => proto.ToDomain());
    }

    [Fact]
    public void ToDomain_rejects_name_longer_than_36_chars()
    {
        var proto = new Evgrpc.SourceCategory
        {
            Id = "cat-x",
            Name = new string('a', 37),
        };
        Should.Throw<ArgumentException>(() => proto.ToDomain());
    }
}
