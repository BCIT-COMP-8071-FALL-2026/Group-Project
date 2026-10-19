using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OLTPApp.Models;

namespace OLTPApp.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Person> Person => Set<Person>();
    public DbSet<Senior> Senior => Set<Senior>();
    public DbSet<Agency> Agency => Set<Agency>();
    public DbSet<Caregiver> Caregiver => Set<Caregiver>();
    public DbSet<FamilyMember> FamilyMember => Set<FamilyMember>();
    public DbSet<AgencyManager> AgencyManager => Set<AgencyManager>();
    public DbSet<Service> Service => Set<Service>();
    public DbSet<Visit> Visit => Set<Visit>();
    public DbSet<VisitNote> VisitNote => Set<VisitNote>();
    public DbSet<SeniorFamilyMember> SeniorFamilyMember => Set<SeniorFamilyMember>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(e =>
        {
            e.HasOne(u => u.Person)
             .WithOne()
             .HasForeignKey<ApplicationUser>(u => u.PersonID)
             .OnDelete(DeleteBehavior.Restrict);
        });

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

        builder.Entity<FamilyMember>(e =>
        {
            e.HasKey(f => f.FamilyMemberID);
            e.HasOne(f => f.Person)
             .WithOne(p => p.FamilyMember)
             .HasForeignKey<FamilyMember>(f => f.FamilyMemberID)
             .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<AgencyManager>(e =>
        {
            e.HasKey(m => m.AgencyManagerID);
            e.HasOne(m => m.Person)
             .WithOne(p => p.AgencyManager)
             .HasForeignKey<AgencyManager>(m => m.AgencyManagerID)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(m => m.Agency)
             .WithMany(a => a.AgencyManagers)
             .HasForeignKey(m => m.AgencyID)
             .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Visit>(e =>
        {
            e.HasOne(v => v.Senior).WithMany(s => s.Visits).HasForeignKey(v => v.SeniorID).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(v => v.Service).WithMany().HasForeignKey(v => v.ServiceID).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(v => v.Caregiver).WithMany(c => c.Visits).HasForeignKey(v => v.CaregiverID).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(v => v.Agency).WithMany().HasForeignKey(v => v.AgencyID).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(v => v.CreatedByUser).WithMany().HasForeignKey(v => v.CreatedByUserID).OnDelete(DeleteBehavior.Restrict);
            e.Property(v => v.Status).HasConversion<string>().HasMaxLength(20);
        });

        builder.Entity<VisitNote>(e =>
        {
            e.HasKey(n => n.NoteID);
            e.HasOne(n => n.Visit).WithMany(v => v.Notes).HasForeignKey(n => n.VisitID).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(n => n.Caregiver).WithMany().HasForeignKey(n => n.CaregiverID).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<SeniorFamilyMember>(e =>
        {
            e.HasKey(sf => new { sf.SeniorID, sf.FamilyMemberID });
            e.HasOne(sf => sf.Senior).WithMany(s => s.SeniorFamilyMembers).HasForeignKey(sf => sf.SeniorID).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(sf => sf.FamilyMember).WithMany(f => f.SeniorFamilyMembers).HasForeignKey(sf => sf.FamilyMemberID).OnDelete(DeleteBehavior.Restrict);
        });
    }
}