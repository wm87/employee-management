using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using WpfApp.Model;

namespace DB_ES
{
    public class AppDbContext : DbContext
    {
        public DbSet<Person> Personen => Set<Person>();

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            string connectionString = "Host=127.0.0.1;Port=5432;Database=personen_db;Username=test_user;Password=sicheres_passwort;";
            options.UseNpgsql(connectionString);
        }
    }
}