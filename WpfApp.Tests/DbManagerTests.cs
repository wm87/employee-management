using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using WpfApp.Model.Data;
using WpfApp.Model;
using Xunit;

namespace WpfApp.Tests
{
    public class DbManagerTests
    {
        private static IDbContextFactory<AppDbContext> CreateFactory(string dbName, IEnumerable<Person>? seed = null)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;

            seed ??= Enumerable.Empty<Person>();

            // Ensure seed data exists in the in-memory database
            using (var seeder = new AppDbContext(options))
            {
                if (seed.Any())
                {
                    seeder.Personen.AddRange(seed);
                    seeder.SaveChanges();
                }
            }

            var mock = new Mock<IDbContextFactory<AppDbContext>>();
            mock.Setup(f => f.CreateDbContext()).Returns(() => new AppDbContext(options));
            return mock.Object;
        }

        [Fact]
        public async Task InsertPerson_AddsPerson()
        {
            var factory = CreateFactory("InsertPersonDb");
            var manager = new DbManager(factory);

            var person = new Person { Vorname = "Max", Nachname = "Mustermann" };
            await manager.InsertPersonAsync(person);

            // verify persisted
            using var ctx = factory.CreateDbContext();
            var count = await ctx.Personen.CountAsync();
            Assert.Equal(1, count);
            var saved = await ctx.Personen.FirstAsync();
            Assert.Equal("Max", saved.Vorname);
        }

        [Fact]
        public async Task LoadPersonsByIds_ReturnsExpected()
        {
            var seed = new[] {
                new Person { Vorname = "A", Nachname = "One" },
                new Person { Vorname = "B", Nachname = "Two" }
            };
            var factory = CreateFactory("LoadByIdsDb", seed);
            var manager = new DbManager(factory);

            using var ctx = factory.CreateDbContext();
            var ids = (await ctx.Personen.Select(p => p.Id).ToListAsync()).ToList();

            var result = await manager.LoadPersonsByIdsAsync(ids);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task DeletePerson_RemovesPerson()
        {
            var seed = new[] { new Person { Vorname = "Del", Nachname = "Me" } };
            var factory = CreateFactory("DeletePersonDb", seed);
            var manager = new DbManager(factory);

            using var ctx = factory.CreateDbContext();
            var id = (await ctx.Personen.Select(p => p.Id).FirstAsync());

            await manager.DeletePersonAsync(id);

            using var verifyCtx = factory.CreateDbContext();
            var exists = await verifyCtx.Personen.FindAsync(id);
            Assert.Null(exists);
        }

        [Fact]
        public async Task CountPersons_ReturnsCorrectCount()
        {
            var seed = new[] {
                new Person { Vorname = "X", Nachname = "Alpha" },
                new Person { Vorname = "Y", Nachname = "Beta" }
            };
            var factory = CreateFactory("CountDb", seed);
            var manager = new DbManager(factory);

            var count = await manager.CountPersonsAsync("", null);
            Assert.Equal(2, count);
        }
    }
}
