using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using QuanLyQuanCafeWeb.TempModels;

namespace QuanLyQuanCafeWeb.TempData;

public partial class TempCafeDbContext : DbContext
{
    public TempCafeDbContext(DbContextOptions<TempCafeDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<CaLamViec> CaLamViecs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CaLamViec>(entity =>
        {
            entity.HasKey(e => e.MaCa).HasName("PK__CaLamVie__27258E7B1D4ADEAD");

            entity.ToTable("CaLamViec");

            entity.HasIndex(e => new { e.MaTk, e.NgayLam }, "IX_CaLamViec_MaTk_NgayLam");

            entity.Property(e => e.GhiChu).HasMaxLength(300);
            entity.Property(e => e.GioBatDau).HasPrecision(0);
            entity.Property(e => e.GioKetThuc).HasPrecision(0);
            entity.Property(e => e.NgayTao)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())", "DF_CaLamViec_NgayTao");
            entity.Property(e => e.TrangThai)
                .HasMaxLength(30)
                .HasDefaultValue("Đã phân công", "DF_CaLamViec_TrangThai");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
