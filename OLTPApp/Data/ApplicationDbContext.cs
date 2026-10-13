using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OLTPApp.Models;

namespace OLTPApp.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Person> Person => Set<Person>();
    public DbSet<Senior> Senior => Set<Senior>();
    public DbSet<Agency> Agency => Set<Agency>();
    public DbSet<Caregiver> Caregiver => Set<Caregiver>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Senior>(e =>
        {
            e.HasKey(s => s.SeniorID);
            e.HasOne(s => s.Person)
             .WithOne(p => p.Senior)
             .HasForeignKey<Senior>(s => s.SeniorID)
             .OnDelete(DeleteBehavior.Restrict);
            e.Property(s => s.Status).HasConversion<string>().HasMaxLength(30);
        });

        builder.Entity<Caregiver>(e =>
        {
            e.HasKey(c => c.CaregiverID);
            e.HasOne(c => c.Person)
             .WithOne(p => p.Caregiver)
             .HasForeignKey<Caregiver>(c => c.CaregiverID)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(c => c.Agency)
             .WithMany(a => a.Caregivers)
             .HasForeignKey(c => c.AgencyID)
             .OnDelete(DeleteBehavior.Restrict);
        });
    }
}