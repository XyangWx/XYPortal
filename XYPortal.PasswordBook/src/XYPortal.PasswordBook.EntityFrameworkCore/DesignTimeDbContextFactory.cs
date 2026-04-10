using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace XYPortal.PasswordBook.EntityFrameworkCore;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<PasswordBookDbContext>
{
    public PasswordBookDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<PasswordBookDbContext>();
        
        optionsBuilder.UseNpgsql("Host=localhost;Database=PasswordBook;Username=postgres;Password=postgres");

        return new PasswordBookDbContext(optionsBuilder.Options);
    }
}
