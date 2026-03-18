using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace XYPortal.PasswordBook.EntityFrameworkCore;

[ConnectionStringName(PasswordBookDbProperties.ConnectionStringName)]
public class PasswordBookDbContext : AbpDbContext<PasswordBookDbContext>, IPasswordBookDbContext
{
    /* Add DbSet for each Aggregate Root here. Example:
     * public DbSet<Question> Questions { get; set; }
     */

    public PasswordBookDbContext(DbContextOptions<PasswordBookDbContext> options)
        : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ConfigurePasswordBook();
    }
}
