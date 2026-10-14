using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using QuanLyQuanCafeWeb.TempModels;

namespace QuanLyQuanCafeWeb.TempData;

public partial class TempDanhGiaDbContext : DbContext
{
    public TempDanhGiaDbContext(DbContextOptions<TempDanhGiaDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<DanhGiaMon> DanhGiaMons { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DanhGiaMon>(entity =>
        {
            entity.HasKey(e => e.MaDanhGia).HasName("PK__DanhGiaM__AA9515BFF3E16686");

            entity.ToTable("DanhGiaMon");

            entity.HasIndex(e => new { e.MaTk, e.MaMon }, "UQ_DanhGiaMon_TaiKhoan_Mon").IsUnique();

            entity.Property(e => e.MaTk).HasColumnName("MaTK");
            entity.Property(e => e.NgayDanhGia)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())", "DF_DanhGiaMon_NgayDanhGia");
            entity.Property(e => e.NoiDung).HasMaxLength(500);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
