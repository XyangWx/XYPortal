using System;
using Shouldly;
using Xunit;

namespace XYPortal.EvGRPC.SourceCategories;

public class SourceCategory_Tests
{
    [Fact]
    public void Ctor_assigns_id_and_name()
    {
        var c = new SourceCategory("cat-1", "home");
        c.Id.ShouldBe("cat-1");
        c.Name.ShouldBe("home");
    }

    [Fact]
    public void Ctor_rejects_blank_id()
    {
        Should.Throw<ArgumentException>(() => new SourceCategory("", "home"));
    }

    [Fact]
    public void Ctor_rejects_blank_name()
    {
        Should.Throw<ArgumentException>(() => new SourceCategory("cat-1", ""));
    }

    [Fact]
    public void Ctor_rejects_name_longer_than_36_chars()
    {
        Should.Throw<ArgumentException>(() =>
            new SourceCategory("cat-1", new string('a', 37)));
    }

    [Fact]
    public void Create_factory_assigns_empty_id_and_keeps_name()
    {
        var c = SourceCategory.Create("office");
        c.Id.ShouldBe(string.Empty);
        c.Name.ShouldBe("office");
    }

    [Fact]
    public void SetName_updates_name()
    {
        var c = SourceCategory.Create("home");
        c.SetName("house");
        c.Name.ShouldBe("house");
    }
}
