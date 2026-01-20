using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace XYPortal.Data;

/* This is used if database provider does't define
 * IXYPortalDbSchemaMigrator implementation.
 */
public class NullXYPortalDbSchemaMigrator : IXYPortalDbSchemaMigrator, ITransientDependency
{
    public Task MigrateAsync()
    {
        return Task.CompletedTask;
    }
}
