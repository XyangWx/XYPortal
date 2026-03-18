using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace XYPortal.PasswordBook.EntityFrameworkCore;

[ConnectionStringName(PasswordBookDbProperties.ConnectionStringName)]
public interface IPasswordBookDbContext : IEfCoreDbContext
{
    /* Add DbSet for each Aggregate Root here. Example:
     * DbSet<Question> Questions { get; }
     */
}
