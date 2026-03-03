using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace XYPortal.RandomStringProvider.EntityFrameworkCore;

[ConnectionStringName(RandomStringProviderDbProperties.ConnectionStringName)]
public interface IRandomStringProviderDbContext : IEfCoreDbContext
{
    /* Add DbSet for each Aggregate Root here. Example:
     * DbSet<Question> Questions { get; }
     */
}
