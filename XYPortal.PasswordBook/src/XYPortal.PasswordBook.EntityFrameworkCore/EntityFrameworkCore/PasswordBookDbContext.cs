using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using XYPortal.PasswordBook.AggregateRoots;
using XYPortal.PasswordBook.Entities;
using PasswordBookEntity = XYPortal.PasswordBook.AggregateRoots.PasswordBook;

namespace XYPortal.PasswordBook.EntityFrameworkCore;

[ConnectionStringName(PasswordBookDbProperties.ConnectionStringName)]
public class PasswordBookDbContext : AbpDbContext<PasswordBookDbContext>, IPasswordBookDbContext
{
    public DbSet<PasswordBookEntity> PasswordBooks { get; set; }
    public DbSet<PasswordEntry> PasswordEntries { get; set; }
    public DbSet<PasswordHistory> PasswordHistories { get; set; }

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
