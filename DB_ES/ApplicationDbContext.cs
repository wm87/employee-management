using Microsoft.EntityFrameworkCore;
using WpfApp.Model;

namespace DB_ES
{
    public class AppDbContext : DbContext
    {
        public DbSet<Person> Personen => Set<Person>();

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            string connectionString = "Server = 127.0.0.1; Port = 3306; Database = personen_db; User = test_user; Password = sicheres_passwort; SslMode = Preferred;";
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
        }
    }
}