using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace XYPortal.EvGRPC.EntityFrameworkCore;

[ConnectionStringName(EvGRPCDbProperties.ConnectionStringName)]
public interface IEvGRPCDbContext : IEfCoreDbContext
{
    /* Add DbSet for each Aggregate Root here. Example:
     * DbSet<Question> Questions { get; }
     */
}
