using Microsoft.EntityFrameworkCore;

namespace WpfApp.Model.Data
{
    public class DbManager
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public DbManager(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task InsertPersonAsync(Person person)
        {
            await using var context = _contextFactory.CreateDbContext();
            context.Personen.Add(person);
            await context.SaveChangesAsync();
        }

        public async Task UpdatePersonAsync(Person person)
        {
            await using var context = _contextFactory.CreateDbContext();
            context.Personen.Update(person);
            await context.SaveChangesAsync();
        }

        public async Task DeletePersonAsync(int id)
        {
            await using var context = _contextFactory.CreateDbContext();
            var person = await context.Personen.FindAsync(id);
            if (person != null)
            {
                context.Personen.Remove(person);
                await context.SaveChangesAsync();
            }
        }

        public async Task<List<Person>> LoadPagedPersonsAsync(
            int skip,
            int take,
            string? filter = null,
            List<int>? ids = null)
        {
            await using var context = _contextFactory.CreateDbContext();

            if ((ids is null or { Count: 0 }) && string.IsNullOrWhiteSpace(filter))
            {
                context.ChangeTracker.AutoDetectChangesEnabled = false;

                var personen = await context.Personen
                    .OrderBy(p => p.Nachname).ThenBy(p => p.Id)
                    .Skip(skip)
                    .Take(take)
                    .ToListAsync();

                context.ChangeTracker.AutoDetectChangesEnabled = true;

                return personen;
            }

            var query = context.Personen.AsQueryable();

            if (ids is { Count: > 0 })
                query = query.Where(p => ids.Contains(p.Id));
            else if (!string.IsNullOrWhiteSpace(filter))
                query = query.Where(p =>
                    EF.Functions.Like(p.Vorname, $"%{filter}%") ||
                    EF.Functions.Like(p.Nachname, $"%{filter}%"));

            return await query
                .OrderBy(p => p.Nachname)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        public async Task<List<Person>> LoadPersonsByIdsAsync(List<int> ids)
        {
            if (ids is not { Count: > 0 })
                return [];

            using var context = _contextFactory.CreateDbContext();
            var idSet = new HashSet<int>(ids);
            return await context.Personen
                .Where(p => idSet.Contains(p.Id))
                .ToListAsync();
        }

        public async Task<int> CountPersonsAsync(string filter, List<int>? ids = null)
        {
            await using var context = _contextFactory.CreateDbContext();
            var query = context.Personen.AsNoTracking().AsQueryable();

            if (ids is { Count: > 0 })
                query = query.Where(p => ids.Contains(p.Id));
            else if (!string.IsNullOrWhiteSpace(filter))
                query = query.Where(p =>
                    EF.Functions.Like(p.Vorname, $"%{filter}%") ||
                    EF.Functions.Like(p.Nachname, $"%{filter}%"));

            return await query.CountAsync();
        }
    }
}
