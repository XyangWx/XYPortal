using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using XYPortal.LinkBoard.Entities;

namespace XYPortal.LinkBoard.EntityFrameworkCore;

[ConnectionStringName(LinkBoardDbProperties.ConnectionStringName)]
public class LinkBoardDbContext : AbpDbContext<LinkBoardDbContext>, ILinkBoardDbContext
{
    public DbSet<LinkCategory> LinkCategories { get; set; }
    public DbSet<Link> Links { get; set; }

    public LinkBoardDbContext(DbContextOptions<LinkBoardDbContext> options)
        : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ConfigureLinkBoard();
    }
}
