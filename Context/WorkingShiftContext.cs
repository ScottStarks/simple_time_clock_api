using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using WorkingShiftActivity.Context.Models;

namespace WorkingShiftActivity.Context;

public partial class WorkingShiftContext : DbContext
{
    public WorkingShiftContext()
    {

    }
    public WorkingShiftContext(DbContextOptions<WorkingShiftContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Activity> Activities { get; set; }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Activity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_WorkShiftActivity");
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.RoleNavigation).WithMany(p => p.Employees)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Employee_Role");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.Property(e => e.Role1).ValueGeneratedNever();
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
