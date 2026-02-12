using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace XYPortal.LinkBoard.EntityFrameworkCore;

[ConnectionStringName(LinkBoardDbProperties.ConnectionStringName)]
public class LinkBoardDbContext : AbpDbContext<LinkBoardDbContext>, ILinkBoardDbContext
{
    /* Add DbSet for each Aggregate Root here. Example:
     * public DbSet<Question> Questions { get; set; }
     */

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
