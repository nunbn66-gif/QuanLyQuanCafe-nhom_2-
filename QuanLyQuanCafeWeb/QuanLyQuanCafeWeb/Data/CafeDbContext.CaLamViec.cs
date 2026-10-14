
using Microsoft.EntityFrameworkCore;
using QuanLyQuanCafeWeb.Models;
using QuanLyQuanCafeWeb.TempModels;
namespace QuanLyQuanCafeWeb.Data
{
    public partial class CafeDbContext
    {
        public virtual DbSet<CaLamViec> CaLamViecs { get; set; } = null!;
        public virtual DbSet<DanhGiaMon> DanhGiaMons { get; set; } = null!;
        partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CaLamViec>(entity =>
            {
                entity.HasKey(e => e.MaCa);
                entity.ToTable("CaLamViec", tb =>
                {
                    tb.HasCheckConstraint("CK_CaLamViec_Gio", "[GioBatDau] < [GioKetThuc]");
                    tb.HasCheckConstraint("CK_CaLamViec_TrangThai", "[TrangThai] IN (N'Đã phân công', N'Đang làm', N'Hoàn thành', N'Đã hủy')");
                });
                entity.Property(e => e.MaCa).ValueGeneratedOnAdd();
                entity.Property(e => e.NgayLam).HasColumnType("date");
                entity.Property(e => e.GioBatDau).HasColumnType("time(0)");
                entity.Property(e => e.GioKetThuc).HasColumnType("time(0)");
                entity.Property(e => e.TrangThai).HasMaxLength(30).HasDefaultValue("Đã phân công");
                entity.Property(e => e.GhiChu).HasMaxLength(300);
                entity.Property(e => e.NgayTao).HasColumnType("datetime2(0)").HasDefaultValueSql("(sysdatetime())");
                entity.HasIndex(e => new { e.MaTk, e.NgayLam }, "IX_CaLamViec_MaTk_NgayLam");
            });
            modelBuilder.Entity<DanhGiaMon>(entity =>
            {
                entity.HasKey(e => e.MaDanhGia);
                entity.ToTable("DanhGiaMon");
                entity.Property(e => e.MaDanhGia).ValueGeneratedOnAdd();
                entity.Property(e => e.SoSao).HasColumnType("tinyint");
                entity.Property(e => e.NoiDung).HasMaxLength(500);
                entity.Property(e => e.NgayDanhGia)
                    .HasColumnType("datetime2(0)")
                    .HasDefaultValueSql("(sysdatetime())");
                entity.HasIndex(e => new { e.MaTk, e.MaMon }, "UQ_DanhGiaMon_TaiKhoan_Mon")
                    .IsUnique();
                entity.HasOne<TaiKhoan>()
                    .WithMany()
                    .HasForeignKey(e => e.MaTk)
                    .OnDelete(DeleteBehavior.NoAction)
                    .HasConstraintName("FK_DanhGiaMon_TaiKhoan");
                entity.HasOne<Mon>()
                    .WithMany()
                    .HasForeignKey(e => e.MaMon)
                    .OnDelete(DeleteBehavior.NoAction)
                    .HasConstraintName("FK_DanhGiaMon_Mon");
            });
        }
    }
}
