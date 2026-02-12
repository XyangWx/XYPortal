using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace XYPortal.LinkBoard.EntityFrameworkCore;

[ConnectionStringName(LinkBoardDbProperties.ConnectionStringName)]
public interface ILinkBoardDbContext : IEfCoreDbContext
{
    /* Add DbSet for each Aggregate Root here. Example:
     * DbSet<Question> Questions { get; }
     */
}
