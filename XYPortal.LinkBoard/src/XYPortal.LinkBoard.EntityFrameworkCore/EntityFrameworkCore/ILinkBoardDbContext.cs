using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using XYPortal.LinkBoard.Entities;

namespace XYPortal.LinkBoard.EntityFrameworkCore;

[ConnectionStringName(LinkBoardDbProperties.ConnectionStringName)]
public interface ILinkBoardDbContext : IEfCoreDbContext
{
    DbSet<LinkCategory> LinkCategories { get; }
    DbSet<Link> Links { get; }
}
