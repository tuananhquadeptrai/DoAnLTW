// Đã chỉnh để hỗ trợ Identity mở rộng với ApplicationUser
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using VAYTIEN.Models;

namespace VAYTIEN.Models;

public partial class QlvayTienContext : IdentityDbContext<ApplicationUser>
{
    public QlvayTienContext()
    {
    }

    public QlvayTienContext(DbContextOptions<QlvayTienContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ChiNhanhNganHang> ChiNhanhNganHangs { get; set; }
    public virtual DbSet<GiaoDich> GiaoDiches { get; set; }
    public virtual DbSet<HopDongVay> HopDongVays { get; set; }
    public virtual DbSet<KhachHang> KhachHangs { get; set; }
    public virtual DbSet<LichTraNo> LichTraNos { get; set; }
    public virtual DbSet<LoaiTienTe> LoaiTienTes { get; set; }
    public virtual DbSet<LoaiVay> LoaiVays { get; set; }
    public DbSet<DoiTuongVay> DoiTuongVays { get; set; }

    public virtual DbSet<NhanVien> NhanViens { get; set; }
    public virtual DbSet<TaiKhoanNganHang> TaiKhoanNganHangs { get; set; }
    public virtual DbSet<TaiSanTheChap> TaiSanTheChaps { get; set; }
    public virtual DbSet<ThanhToanLichTra> ThanhToanLichTras { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ChiNhanhNganHang>(entity =>
        {
            entity.ToTable("ChiNhanhNganHang");
            entity.HasKey(e => e.MaChiNhanh);
            entity.Property(e => e.TenChiNhanh).HasMaxLength(100);
            entity.Property(e => e.DiaChi).HasMaxLength(200);
            entity.Property(e => e.Sdt).HasMaxLength(20).IsUnicode(false).HasColumnName("SDT");
        });

        modelBuilder.Entity<GiaoDich>(entity =>
        {
            entity.ToTable("GiaoDich");
            entity.HasKey(e => e.MaGiaoDich);
        });

        modelBuilder.Entity<HopDongVay>(entity =>
        {
            entity.ToTable("HopDongVay");
            entity.HasKey(e => e.MaHopDong);

            entity.HasOne(h => h.MaKhNavigation)
                .WithMany(kh => kh.HopDongVays)
                .HasForeignKey(h => h.MaKh)
                .HasConstraintName("FK_HopDongVay_KhachHang")
                .OnDelete(DeleteBehavior.Restrict);
        });


        modelBuilder.Entity<KhachHang>(entity =>
        {
            entity.ToTable("KhachHang");
            entity.HasKey(e => e.MaKh);
        });

        modelBuilder.Entity<LichTraNo>(entity =>
        {
            entity.ToTable("LichTraNo");
            entity.HasKey(e => e.MaLich);
        });

        modelBuilder.Entity<LoaiTienTe>(entity =>
        {
            entity.ToTable("LoaiTienTe");
            entity.HasKey(e => e.MaLoaiTienTe);
        });

        modelBuilder.Entity<LoaiVay>(entity =>
        {
            entity.ToTable("LoaiVay");
            entity.HasKey(e => e.MaLoaiVay);
        });

        modelBuilder.Entity<NhanVien>(entity =>
        {
            entity.ToTable("NhanVien");
            entity.HasKey(e => e.MaNv);
        });

        modelBuilder.Entity<TaiKhoanNganHang>(entity =>
        {
            entity.ToTable("TaiKhoanNganHang");
            entity.HasKey(e => e.MaTaiKhoan);
        });

        modelBuilder.Entity<TaiSanTheChap>(entity =>
        {
            entity.ToTable("TaiSanTheChap");
            entity.HasKey(e => e.MaTs);
        });

        modelBuilder.Entity<ThanhToanLichTra>(entity =>
        {
            entity.ToTable("ThanhToanLichTra");
            entity.HasKey(e => e.MaThanhToan);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
