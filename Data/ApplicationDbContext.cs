using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;
using vulnerablewebpage.Models;

namespace vulnerablewebpage.Data
{
    public sealed class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<XssEntry> XssEntries => Set<XssEntry>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<XssEntry>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Title)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.Property(x => x.Content)
                    .IsRequired()
                    .HasMaxLength(4000);

                entity.Property(x => x.CreatedAtUtc)
                    .IsRequired();
            });
        }
    }
}