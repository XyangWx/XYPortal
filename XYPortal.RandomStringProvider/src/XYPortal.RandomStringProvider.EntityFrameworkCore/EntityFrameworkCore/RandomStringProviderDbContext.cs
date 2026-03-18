using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace XYPortal.RandomStringProvider.EntityFrameworkCore;

[ConnectionStringName(RandomStringProviderDbProperties.ConnectionStringName)]
public class RandomStringProviderDbContext : AbpDbContext<RandomStringProviderDbContext>, IRandomStringProviderDbContext
{
    /* Add DbSet for each Aggregate Root here. Example:
     * public DbSet<Question> Questions { get; set; }
     */

    public RandomStringProviderDbContext(DbContextOptions<RandomStringProviderDbContext> options)
        : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ConfigureRandomStringProvider();
    }
}
