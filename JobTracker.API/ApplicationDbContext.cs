using JobTracker.API.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace JobTracker.API
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Company> Companies { get; set; } = null!;
        public DbSet<Job> Jobs { get; set; } = null!;
        public DbSet<JobApplication> JobApplications { get; set; } = null!;
        public DbSet<Interview> Interviews { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ---------- Company ----------
            modelBuilder.Entity<Company>(entity =>
            {
                entity.Property(c => c.Name)
                      .IsRequired()
                      .HasMaxLength(200);

                // keep Industry for now
                entity.Property(c => c.Industry)
                      .HasMaxLength(100);

                entity.Property(c => c.Location)
                      .HasMaxLength(200);
            });

            // ---------- Job ----------
            modelBuilder.Entity<Job>(entity =>
            {
                entity.Property(j => j.Title)
                      .IsRequired()
                      .HasMaxLength(200);

                entity.Property(j => j.Location)
                      .HasMaxLength(200);

                entity.Property(j => j.SalaryMin)
                      .HasPrecision(18, 2);

                entity.Property(j => j.SalaryMax)
                      .HasPrecision(18, 2);

                // Company → Jobs (cascade is fine)
                entity.HasOne(j => j.Company)
                      .WithMany(c => c.Jobs)
                      .HasForeignKey(j => j.CompanyId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ---------- JobApplication ----------
            modelBuilder.Entity<JobApplication>(entity =>
            {
                entity.Property(a => a.Title)
                      .IsRequired()
                      .HasMaxLength(200);

                entity.Property(a => a.Location)
                      .HasMaxLength(200);

                entity.Property(a => a.SourceUrl)
                      .HasMaxLength(500);

                entity.Property(a => a.Notes)
                      .HasMaxLength(2000);

                // JobApplication -> User 
                entity.HasOne(a => a.User)
                      .WithMany()
                      .HasForeignKey(a => a.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                // JobApplication -> Company 
                entity.HasOne(a => a.Company)
                      .WithMany()
                      .HasForeignKey(a => a.CompanyId)
                      .OnDelete(DeleteBehavior.NoAction);

                // JobApplication -> Job 
                entity.HasOne(a => a.Job)
                      .WithMany()
                      .HasForeignKey(a => a.JobId)
                      .OnDelete(DeleteBehavior.NoAction);
            });

            // ---------- Interview ----------
            modelBuilder.Entity<Interview>(entity =>
            {
                entity.Property(i => i.Type)
                      .HasMaxLength(100);

                entity.Property(i => i.Notes)
                      .HasMaxLength(2000);

                // Interview -> JobApplication
                entity.HasOne(i => i.JobApplication)
                      .WithMany(a => a.Interviews)
                      .HasForeignKey(i => i.JobApplicationId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
