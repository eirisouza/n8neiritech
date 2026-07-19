using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Pomelo.EntityFrameworkCore.MySql;

namespace n8neiritech.Infrastructure.Data;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseMySql("Server=localhost;Port=3306;Database=n8neiritech;User=n8nuser;Password=n8npass123;", new MySqlServerVersion(new Version(8, 0, 26)));
        return new AppDbContext(optionsBuilder.Options);
    }
}
