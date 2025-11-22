using Microsoft.EntityFrameworkCore;
using ShopInspector.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopInspector.Infrastructure.Data;
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<Asset> Assets { get; set; }
    public DbSet<AssetType> AssetTypes { get; set; }
    public DbSet<InspectionCheckList> InspectionCheckLists { get; set; }
    public DbSet<AssetCheckList> AssetCheckLists { get; set; }
    public DbSet<InspectionFrequency> InspectionFrequencies { get; set; }
    public DbSet<Company> Companies { get; set; }
    public DbSet<Employee> Employees { get; set; }
    public DbSet<AssetInspection> AssetInspections { get; set; }
    public DbSet<AssetInspectionCheckList> AssetInspectionCheckLists { get; set; }
    public DbSet<InspectionPhoto> InspectionPhotos { get; set; }  // ADD THIS

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Asset
        builder.Entity<Asset>()
            .HasOne(a => a.AssetType)
            .WithMany(t => t.Assets)
            .HasForeignKey(a => a.AssetTypeID)
            .OnDelete(DeleteBehavior.Cascade);

        // AssetCheckList
        builder.Entity<AssetCheckList>()
            .HasOne(a => a.Asset)
            .WithMany(a => a.AssetCheckLists)
            .HasForeignKey(a => a.AssetID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<AssetCheckList>()
            .HasOne(a => a.InspectionCheckList)
            .WithMany(i => i.AssetCheckLists)
            .HasForeignKey(a => a.InspectionCheckListID)
            .OnDelete(DeleteBehavior.Restrict);

        // Employee ↔ Company
        builder.Entity<Employee>()
            .HasOne(e => e.Company)
            .WithMany(c => c.Employees)
            .HasForeignKey(e => e.CompanyID)
            .OnDelete(DeleteBehavior.Restrict);

        // AssetInspection
        builder.Entity<AssetInspection>()
            .HasOne(ai => ai.Asset)
            .WithMany(a => a.AssetInspections)
            .HasForeignKey(ai => ai.AssetID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<AssetInspection>()
            .HasOne(ai => ai.Employee)
            .WithMany(e => e.AssetInspections)
            .HasForeignKey(ai => ai.EmployeeID)
             .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<AssetInspection>()
            .HasOne(ai => ai.InspectionFrequency)
            .WithMany(f => f.AssetInspections)
            .HasForeignKey(ai => ai.InspectionFrequencyID)
            .OnDelete(DeleteBehavior.Restrict);
        // AssetInspectionCheckList
        builder.Entity<AssetInspectionCheckList>()
            .HasOne(aic => aic.AssetInspection)
            .WithMany(ai => ai.AssetInspectionCheckLists)
            .HasForeignKey(aic => aic.AssetInspectionID)
           .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<AssetInspectionCheckList>()
            .HasOne(aic => aic.AssetCheckList)
            .WithMany(ac => ac.AssetInspectionCheckLists)
            .HasForeignKey(aic => aic.AssetCheckListID)
              .OnDelete(DeleteBehavior.Restrict);

        // InspectionPhoto relationship - ADD THIS
        builder.Entity<InspectionPhoto>()
            .HasOne(ip => ip.AssetInspection)
            .WithMany(ai => ai.Photos)
            .HasForeignKey(ip => ip.AssetInspectionID)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

