using System.Threading.Tasks;

namespace XYPortal.Data;

public interface IXYPortalDbSchemaMigrator
{
    Task MigrateAsync();
}
