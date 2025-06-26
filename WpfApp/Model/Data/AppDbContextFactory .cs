using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace WpfApp.Model.Data
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(@"C:\Users\mawei\Documents\repos\WpfApp\WpfApp")
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            var connStr = config.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string not found.");

            optionsBuilder.UseMySql(connStr, ServerVersion.AutoDetect(connStr));

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
