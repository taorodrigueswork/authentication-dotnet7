using Entities.Entity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace Persistence.Context;

[ExcludeFromCodeCoverage]
public class SqliteDataContext : IdentityDbContext
{
    public SqliteDataContext(DbContextOptions<SqliteDataContext> options) : base(options)
    {
        Database.OpenConnection();
        //Database.EnsureCreated();
    }

    public override void Dispose()
    {
        Database.CloseConnection();
        base.Dispose();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        // connect to sqlite database
        options.UseSqlite("DataSource=:memory:");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // call base method to create identity tables
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SqliteDataContext).Assembly);

        // Configuring indirect many-to-many relationship between Day and Person using explicit class DayPersonEntity
        // check this link for more information: https://docs.microsoft.com/en-us/ef/core/modeling/relationships#many-to-many
        // https://www.thereformedprogrammer.net/updating-many-to-many-relationships-in-ef-core-5-and-above/
        modelBuilder.Entity<DayEntity>()
            .HasMany(n => n.People)
            .WithMany(n => n.Days)
            .UsingEntity<DayPersonEntity>(
                l => l.HasOne<PersonEntity>().WithMany().HasForeignKey(e => e.PersonId).OnDelete(DeleteBehavior.Cascade),
                r => r.HasOne<DayEntity>().WithMany().HasForeignKey(e => e.DayId).OnDelete(DeleteBehavior.Cascade));
    }

    public DbSet<PersonEntity>? Person { get; set; }
    public DbSet<DayEntity>? Day { get; set; }
    public DbSet<ScheduleEntity>? Schedule { get; set; }
    public DbSet<DayPersonEntity>? DayPerson { get; set; }
}