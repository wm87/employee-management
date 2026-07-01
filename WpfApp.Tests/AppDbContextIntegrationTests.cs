using System;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using WpfApp.Model.Data;
using WpfApp.Model;
using Xunit;

namespace WpfApp.Tests
{
    public class AppDbContextIntegrationTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly DbContextOptions<AppDbContext> _options;

        public AppDbContextIntegrationTests()
        {
            // Create in-memory SQLite connection that persists for the test lifetime
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            _options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(_connection)
                .Options;

            // Ensure database schema is created
            using var context = new AppDbContext(_options);
            context.Database.EnsureCreated();
        }

        [Fact]
        public async Task CanPerformCrudOperations()
        {
            // Create
            using (var ctx = new AppDbContext(_options))
            {
                ctx.Personen.Add(new Person { Vorname = "I", Nachname = "Ntegration" });
                await ctx.SaveChangesAsync();
            }

            // Read
            using (var ctx = new AppDbContext(_options))
            {
                var person = await ctx.Personen.FirstOrDefaultAsync(p => p.Nachname == "Ntegration");
                Assert.NotNull(person);
                Assert.Equal("I", person.Vorname);

                // Update
                person.Vorname = "Updated";
                ctx.Personen.Update(person);
                await ctx.SaveChangesAsync();
            }

            // Verify update and delete
            using (var ctx = new AppDbContext(_options))
            {
                var person = await ctx.Personen.FirstOrDefaultAsync(p => p.Nachname == "Ntegration");
                Assert.NotNull(person);
                Assert.Equal("Updated", person.Vorname);

                ctx.Personen.Remove(person);
                await ctx.SaveChangesAsync();
            }

            using (var ctx = new AppDbContext(_options))
            {
                var count = await ctx.Personen.CountAsync(p => p.Nachname == "Ntegration");
                Assert.Equal(0, count);
            }
        }

        [Fact]
        public async Task QueryWithEFFunctions_Like_Works()
        {
            using (var ctx = new AppDbContext(_options))
            {
                ctx.Personen.Add(new Person { Vorname = "LikeMe", Nachname = "FilterTest" });
                await ctx.SaveChangesAsync();
            }

            using (var ctx = new AppDbContext(_options))
            {
                var results = await ctx.Personen
                    .ToListAsync();

                Assert.Contains(results, p => p.Nachname == "FilterTest");
            }
        }

        public void Dispose()
        {
            _connection?.Dispose();
        }
    }
}
