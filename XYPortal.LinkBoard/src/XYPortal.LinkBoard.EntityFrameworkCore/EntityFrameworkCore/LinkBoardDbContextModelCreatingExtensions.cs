using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.EntityFrameworkCore.Modeling;
using XYPortal.LinkBoard.Entities;

namespace XYPortal.LinkBoard.EntityFrameworkCore;

public static class LinkBoardDbContextModelCreatingExtensions
{
    public static void ConfigureLinkBoard(
        this ModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));

        builder.Entity<LinkCategory>(b =>
        {
            b.ToTable(LinkBoardDbProperties.DbTablePrefix + "Categories", LinkBoardDbProperties.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.Name).IsRequired().HasMaxLength(LinkBoardConsts.CategoryNameMaxLength);
            b.Property(x => x.DisplayName).HasMaxLength(LinkBoardConsts.CategoryDisplayNameMaxLength);
            b.Property(x => x.Description).HasMaxLength(LinkBoardConsts.CategoryDescriptionMaxLength);
            b.Property(x => x.Icon).HasMaxLength(LinkBoardConsts.CategoryIconMaxLength);
            b.Property(x => x.ReviewComment).HasMaxLength(LinkBoardConsts.ReviewCommentMaxLength);

            b.HasIndex(x => x.Name).IsUnique();
            b.HasIndex(x => x.SortOrder);
            b.HasIndex(x => new { x.IsPublic, x.Status });
            b.HasIndex(x => x.DraftOfId);
        });

        builder.Entity<Link>(b =>
        {
            b.ToTable(LinkBoardDbProperties.DbTablePrefix + "Links", LinkBoardDbProperties.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.Title).IsRequired().HasMaxLength(LinkBoardConsts.LinkTitleMaxLength);
            b.Property(x => x.Url).IsRequired().HasMaxLength(LinkBoardConsts.LinkUrlMaxLength);
            b.Property(x => x.Description).HasMaxLength(LinkBoardConsts.LinkDescriptionMaxLength);
            b.Property(x => x.Icon).HasMaxLength(LinkBoardConsts.LinkIconMaxLength);
            b.Property(x => x.ReviewComment).HasMaxLength(LinkBoardConsts.ReviewCommentMaxLength);

            b.HasOne(x => x.Category)
                .WithMany()
                .HasForeignKey(x => x.CategoryId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(x => x.CategoryId);
            b.HasIndex(x => x.SortOrder);
            b.HasIndex(x => x.Url);
            b.HasIndex(x => new { x.IsPublic, x.Status, x.CreatorId });
            b.HasIndex(x => x.DraftOfId);
        });
    }
}
