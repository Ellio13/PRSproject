using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace PRS.Models;

public partial class PRSDBContext : DbContext
{
    public PRSDBContext()
    {
    }

    public PRSDBContext(DbContextOptions<PRSDBContext> options)
        : base(options)
    {
    }

    public virtual DbSet<LineItem> LineItems { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<Request> Requests { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Vendor> Vendors { get; set; }

//    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
//        => optionsBuilder.UseSqlServer("Data Source=DESKTOP-SGG2DJD\\SQLEXPRESS;Initial Catalog=PRSproto;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False");

    //protected override void OnModelCreating(ModelBuilder modelBuilder)
    //{
    //    modelBuilder.Entity<LineItem>(entity =>
    //    {
    //        entity.HasKey(e => e.Id).HasName("PK__LineItem__3214EC27DC4A99A0");

    //        entity.HasOne(d => d.Product).WithMany(p => p.LineItems)
    //            .OnDelete(DeleteBehavior.ClientSetNull)
    //            .HasConstraintName("FK__LineItem__Produc__47DBAE45");

    //        entity.HasOne(d => d.Request).WithMany(p => p.LineItems)
    //            .OnDelete(DeleteBehavior.ClientSetNull)
    //            .HasConstraintName("FK__LineItem__Reques__46E78A0C");
    //    });

    //    modelBuilder.Entity<Product>(entity =>
    //    {
    //        entity.HasKey(e => e.Id).HasName("PK__Product__3214EC271CDBA012");

    //        entity.HasOne(d => d.Vendor).WithMany(p => p.Products)
    //            .OnDelete(DeleteBehavior.ClientSetNull)
    //            .HasConstraintName("FK__Product__VendorI__3E52440B");
    //    });

    //    modelBuilder.Entity<Request>(entity =>
    //    {
    //        entity.HasKey(e => e.Id).HasName("PK__Request__3214EC27E8A76D96");

    //        entity.Property(e => e.Status).HasDefaultValue("NEW");
    //        entity.Property(e => e.Total).HasDefaultValue(0.0m);

    //        entity.HasOne(d => d.User).WithMany(p => p.Requests)
    //            .OnDelete(DeleteBehavior.ClientSetNull)
    //            .HasConstraintName("FK__Request__UserID__412EB0B6");
    //    });

    //    modelBuilder.Entity<User>(entity =>
    //    {
    //        entity.HasKey(e => e.Id).HasName("PK__User__3214EC27FA970E07");
    //    });

    //    modelBuilder.Entity<Vendor>(entity =>
    //    {
    //        entity.HasKey(e => e.Id).HasName("PK__Vendor__3214EC27F4BB6B13");
    //    });

    //    OnModelCreatingPartial(modelBuilder);
    //}

    //partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
