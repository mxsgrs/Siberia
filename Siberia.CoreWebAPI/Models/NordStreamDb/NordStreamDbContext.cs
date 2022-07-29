using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Siberia.CoreWebAPI.Models.NordStreamDb
{
    public partial class NordStreamDbContext : DbContext
    {
        public NordStreamDbContext()
        {
        }

        public NordStreamDbContext(DbContextOptions<NordStreamDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Pipeline> Pipelines { get; set; } = null!;
        public virtual DbSet<Society> Societies { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Pipeline>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();
            });

            modelBuilder.Entity<Society>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
