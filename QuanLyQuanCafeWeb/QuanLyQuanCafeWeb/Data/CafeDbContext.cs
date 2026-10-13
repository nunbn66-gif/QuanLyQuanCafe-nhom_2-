using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using QuanLyQuanCafeWeb.Models;

namespace QuanLyQuanCafeWeb.Data;

public partial class CafeDbContext : DbContext
{
    public CafeDbContext(DbContextOptions<CafeDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Ban> Bans { get; set; }

    public virtual DbSet<ChiTietOrder> ChiTietOrders { get; set; }

    public virtual DbSet<DanhMucMon> DanhMucMons { get; set; }

    public virtual DbSet<Mon> Mons { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<TaiKhoan> TaiKhoans { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Ban>(entity =>
        {
            entity.HasKey(e => e.MaBan).HasName("PK__Ban__3520ED6CBDFD9223");

            entity.Property(e => e.TrangThai).HasDefaultValue("Trống");
        });

        modelBuilder.Entity<ChiTietOrder>(entity =>
        {
            entity.HasOne(d => d.MaMonNavigation).WithMany(p => p.ChiTietOrders)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChiTietOr__MaMon__6E01572D");

            entity.HasOne(d => d.MaOrderNavigation).WithMany(p => p.ChiTietOrders)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ChiTietOr__MaOrd__6D0D32F4");
        });

        modelBuilder.Entity<DanhMucMon>(entity =>
        {
            entity.HasKey(e => e.MaDm).HasName("PK__DanhMucM__2725866E970E964D");

            entity.Property(e => e.TrangThai).HasDefaultValue(true);
        });

        modelBuilder.Entity<Mon>(entity =>
        {
            entity.HasKey(e => e.MaMon).HasName("PK__Mon__3A5B29A8518E001B");

            entity.Property(e => e.TrangThai).HasDefaultValue("Còn bán");

            entity.HasOne(d => d.MaDmNavigation).WithMany(p => p.Mons)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Mon__MaDM__534D60F1");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.MaOrder).HasName("PK__Order__50559EF70AF0FBCB");

            entity.Property(e => e.GioVao).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.TrangThai).HasDefaultValue("Đang tạo");

            entity.HasOne(d => d.MaBanNavigation).WithMany(p => p.Orders).HasConstraintName("FK__Order__MaBan__656C112C");

            entity.HasOne(d => d.MaKhNavigation).WithMany(p => p.Orders).HasConstraintName("FK__Order__MaKH__66603565");
        });

        modelBuilder.Entity<TaiKhoan>(entity =>
        {
            entity.HasKey(e => e.MaTk).HasName("PK__TaiKhoan__27250070EE35506B");

            entity.Property(e => e.TrangThai).HasDefaultValue(true);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
