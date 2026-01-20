using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using XYPortal.Data;
using Volo.Abp.DependencyInjection;

namespace XYPortal.EntityFrameworkCore;

public class EntityFrameworkCoreXYPortalDbSchemaMigrator
    : IXYPortalDbSchemaMigrator, ITransientDependency
{
    private readonly IServiceProvider _serviceProvider;

    public EntityFrameworkCoreXYPortalDbSchemaMigrator(
        IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task MigrateAsync()
    {
        /* We intentionally resolve the XYPortalDbContext
         * from IServiceProvider (instead of directly injecting it)
         * to properly get the connection string of the current tenant in the
         * current scope.
         */

        await _serviceProvider
            .GetRequiredService<XYPortalDbContext>()
            .Database
            .MigrateAsync();
    }
}
