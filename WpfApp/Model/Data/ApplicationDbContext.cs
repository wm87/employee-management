using Microsoft.EntityFrameworkCore;

namespace WpfApp.Model.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Person> Personen { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Person>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Id).ValueGeneratedOnAdd();
                entity.Property(p => p.Vorname).IsRequired();
                entity.Property(p => p.Nachname).IsRequired();
                entity.Property(p => p.Geburtsdatum);
                entity.Property(p => p.Geschlecht).HasConversion<int>();
                entity.Property(p => p.Abteilung).HasConversion<int>();

                entity.HasIndex(p => new { p.Nachname, p.Vorname });
                entity.HasIndex(p => new { p.Nachname, p.Id });
            });
        }
    }
}
