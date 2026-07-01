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

        [Fact]
        public async Task UpdatePerson_UpdatesPersistedPerson()
        {
            var seed = new[] { new Person { Vorname = "Old", Nachname = "Name" } };
            var factory = CreateFactory("UpdateDb", seed);
            var manager = new DbManager(factory);

            // load the seeded entity to get its id
            using var ctx = factory.CreateDbContext();
            var person = await ctx.Personen.FirstAsync();
            person.Vorname = "New";

            await manager.UpdatePersonAsync(person);

            using var verify = factory.CreateDbContext();
            var saved = await verify.Personen.FindAsync(person.Id);
            Assert.NotNull(saved);
            Assert.Equal("New", saved!.Vorname);
        }

        [Fact]
        public async Task LoadPagedPersons_NoFilter_ReturnsPagedAndOrdered()
        {
            // Create seed with different last names to verify ordering
            var seed = new[] {
                new Person { Vorname = "P1", Nachname = "C" },
                new Person { Vorname = "P2", Nachname = "A" },
                new Person { Vorname = "P3", Nachname = "B" }
            };

            var factory = CreateFactory("PagedDb", seed);
            var manager = new DbManager(factory);

            // request 2 items, skip 0 -> should return A, B (ordered by Nachname)
            var result = await manager.LoadPagedPersonsAsync(0, 2, null, null);

            Assert.Equal(2, result.Count);
            Assert.Equal("A", result[0].Nachname);
            Assert.Equal("B", result[1].Nachname);
        }

        [Fact]
        public async Task LoadPagedPersons_SkipTake_Works()
        {
            var seed = new[] {
                new Person { Vorname = "P1", Nachname = "C" },
                new Person { Vorname = "P2", Nachname = "A" },
                new Person { Vorname = "P3", Nachname = "B" }
            };

            var factory = CreateFactory("PagedDb_Skip", seed);
            var manager = new DbManager(factory);

            // skip 1, take 1 -> should return the second item in ordered list (B)
            var result = await manager.LoadPagedPersonsAsync(1, 1, null, null);

            Assert.Single(result);
            Assert.Equal("B", result[0].Nachname);
        }

        [Fact]
        public async Task LoadPagedPersons_WithIds_FiltersAndOrders()
        {
            var seed = new[] {
                new Person { Vorname = "P1", Nachname = "C" },
                new Person { Vorname = "P2", Nachname = "A" },
                new Person { Vorname = "P3", Nachname = "B" }
            };

            var factory = CreateFactory("PagedDb_Ids", seed);
            var manager = new DbManager(factory);

            using var ctx = factory.CreateDbContext();
            var persons = await ctx.Personen.ToListAsync();
            // pick ids by matching last names to avoid relying on insertion id ordering
            var idA = persons.Single(p => p.Nachname == "A").Id;
            var idC = persons.Single(p => p.Nachname == "C").Id;
            var ids = new List<int> { idC, idA };

            var result = await manager.LoadPagedPersonsAsync(0, 10, null, ids);

            // Should return only the two requested, ordered by Nachname (A, C)
            Assert.Equal(2, result.Count);
            Assert.Equal("A", result[0].Nachname);
            Assert.Equal("C", result[1].Nachname);
        }
    }
}
