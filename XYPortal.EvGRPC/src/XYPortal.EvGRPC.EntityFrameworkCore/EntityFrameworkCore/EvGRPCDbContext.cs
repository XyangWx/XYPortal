using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace XYPortal.EvGRPC.EntityFrameworkCore;

[ConnectionStringName(EvGRPCDbProperties.ConnectionStringName)]
public class EvGRPCDbContext : AbpDbContext<EvGRPCDbContext>, IEvGRPCDbContext
{
    /* Add DbSet for each Aggregate Root here. Example:
     * public DbSet<Question> Questions { get; set; }
     */

    public EvGRPCDbContext(DbContextOptions<EvGRPCDbContext> options)
        : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ConfigureEvGRPC();
    }
}
