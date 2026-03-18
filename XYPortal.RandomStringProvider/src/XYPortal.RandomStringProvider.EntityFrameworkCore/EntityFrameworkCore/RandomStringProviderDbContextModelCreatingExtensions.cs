using Microsoft.EntityFrameworkCore;
using Volo.Abp;

namespace XYPortal.RandomStringProvider.EntityFrameworkCore;

public static class RandomStringProviderDbContextModelCreatingExtensions
{
    public static void ConfigureRandomStringProvider(
        this ModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));

        /* Configure all entities here. Example:

        builder.Entity<Question>(b =>
        {
            //Configure table & schema name
            b.ToTable(RandomStringProviderDbProperties.DbTablePrefix + "Questions", RandomStringProviderDbProperties.DbSchema);

            b.ConfigureByConvention();

            //Properties
            b.Property(q => q.Title).IsRequired().HasMaxLength(QuestionConsts.MaxTitleLength);

            //Relations
            b.HasMany(question => question.Tags).WithOne().HasForeignKey(qt => qt.QuestionId);

            //Indexes
            b.HasIndex(q => q.CreationTime);
        });
        */
    }
}
