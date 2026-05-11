using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using XYPortal.PasswordBook.AggregateRoots;
using XYPortal.PasswordBook.Entities;
using PasswordBookEntity = XYPortal.PasswordBook.AggregateRoots.PasswordBook;

namespace XYPortal.PasswordBook.EntityFrameworkCore;

[ConnectionStringName(PasswordBookDbProperties.ConnectionStringName)]
public interface IPasswordBookDbContext : IEfCoreDbContext
{
    DbSet<PasswordBookEntity> PasswordBooks { get; }
    DbSet<PasswordEntry> PasswordEntries { get; }
    DbSet<PasswordHistory> PasswordHistories { get; }
}
