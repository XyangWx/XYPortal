using System;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.EntityFrameworkCore.Modeling;
using XYPortal.PasswordBook.AggregateRoots;
using XYPortal.PasswordBook.Entities;
using PasswordBookEntity = XYPortal.PasswordBook.AggregateRoots.PasswordBook;

namespace XYPortal.PasswordBook.EntityFrameworkCore;

public static class PasswordBookDbContextModelCreatingExtensions
{
    public static void ConfigurePasswordBook(
        this ModelBuilder builder,
        Action<PasswordBookModelBuilderConfigurationOptions>? optionsAction = null)
    {
        var options = new PasswordBookModelBuilderConfigurationOptions();
        optionsAction?.Invoke(options);

        builder.Entity<PasswordBookEntity>(b =>
        {
            b.ToTable(options.TablePrefix + "PasswordBooks", options.Schema);
            b.ConfigureByConvention();
            b.HasKey(x => x.Id);

            b.Property(x => x.OwnerId).IsRequired();
            b.Property(x => x.Name).IsRequired().HasMaxLength(200);
            b.Property(x => x.Description).HasMaxLength(1000);
            b.Property(x => x.PasswordFormatJson).IsRequired().HasColumnType("text");

            b.HasIndex(x => x.OwnerId);
            b.HasIndex(x => x.IsDeleted);
            b.HasIndex(x => new { x.OwnerId, x.Name }).IsUnique().HasFilter("[IsDeleted] = 0");
        });

        builder.Entity<PasswordEntry>(b =>
        {
            b.ToTable(options.TablePrefix + "PasswordEntries", options.Schema);
            b.ConfigureByConvention();
            b.HasKey(x => x.Id);

            b.Property(x => x.Title).IsRequired().HasMaxLength(200);
            b.Property(x => x.Username).HasMaxLength(200);
            b.Property(x => x.CurrentPassword).IsRequired().HasMaxLength(500);
            b.Property(x => x.Remark).HasMaxLength(2000);
            b.Property(x => x.PasswordType).HasConversion<int>();
            b.Property(x => x.WeakLevel).HasConversion<int?>();

            b.HasIndex(x => x.PasswordBookId);
            b.HasIndex(x => x.IsDeleted);
        });

        builder.Entity<PasswordHistory>(b =>
        {
            b.ToTable(options.TablePrefix + "PasswordHistories", options.Schema);
            b.ConfigureByConvention();
            b.HasKey(x => x.Id);

            b.Property(x => x.PasswordValue).IsRequired().HasMaxLength(500);

            b.HasIndex(x => x.PasswordEntryId);
            b.HasIndex(x => x.IsCurrent);
        });
    }
}

public class PasswordBookModelBuilderConfigurationOptions
{
    public string TablePrefix { get; set; } = "";
    public string? Schema { get; set; }
}
