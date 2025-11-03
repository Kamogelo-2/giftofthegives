using GiftOfTheGivers2.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GiftOfTheGivers2.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<User> Users { get; set; }
        public DbSet<DisasterIncident> DisasterIncidents { get; set; }
        public DbSet<ResourceCategory> ResourceCategories { get; set; }
        public DbSet<Donation> Donations { get; set; }
        public DbSet<VolunteerTask> VolunteerTasks { get; set; }
        public DbSet<VolunteerAssignment> VolunteerAssignments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships and constraints
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<DisasterIncident>()
                .HasOne(d => d.User)
                .WithMany()
                .HasForeignKey(d => d.UserId);

            modelBuilder.Entity<Donation>()
                .HasOne(d => d.User)
                .WithMany()
                .HasForeignKey(d => d.UserId);

            modelBuilder.Entity<Donation>()
                .HasOne(d => d.Category)
                .WithMany()
                .HasForeignKey(d => d.CategoryId);

            modelBuilder.Entity<VolunteerTask>()
                .HasOne(v => v.Incident)
                .WithMany()
                .HasForeignKey(v => v.IncidentId);

            modelBuilder.Entity<VolunteerAssignment>()
                .HasOne(va => va.Task)
                .WithMany()
                .HasForeignKey(va => va.TaskId);

            modelBuilder.Entity<VolunteerAssignment>()
                .HasOne(va => va.User)
                .WithMany()
                .HasForeignKey(va => va.UserId);
        }
    }
}

