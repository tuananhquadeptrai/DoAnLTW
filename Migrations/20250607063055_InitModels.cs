using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VAYTIEN.Migrations
{
    /// <inheritdoc />
    public partial class InitModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Age = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CCCD = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VaiTro = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChiNhanhNganHang",
                columns: table => new
                {
                    MaChiNhanh = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenChiNhanh = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DiaChi = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SDT = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChiNhanhNganHang", x => x.MaChiNhanh);
                });

            migrationBuilder.CreateTable(
                name: "DoiTuongVays",
                columns: table => new
                {
                    MaDoiTuongVay = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenDoiTuong = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LaiSuat = table.Column<decimal>(type: "decimal(5,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DoiTuongVays", x => x.MaDoiTuongVay);
                });

            migrationBuilder.CreateTable(
                name: "LoaiTienTe",
                columns: table => new
                {
                    MaLoaiTienTe = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenLoaiTienTe = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoaiTienTe", x => x.MaLoaiTienTe);
                });

            migrationBuilder.CreateTable(
                name: "LoaiVay",
                columns: table => new
                {
                    MaLoaiVay = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenLoaiVay = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LaiSuat = table.Column<double>(type: "float", nullable: true),
                    KyHan = table.Column<int>(type: "int", nullable: true),
                    GhiChu = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoaiVay", x => x.MaLoaiVay);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NhanVien",
                columns: table => new
                {
                    MaNv = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenNv = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Sdt = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MaChiNhanh = table.Column<int>(type: "int", nullable: true),
                    MatKhau = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VaiTro = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MaChiNhanhNavigationMaChiNhanh = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NhanVien", x => x.MaNv);
                    table.ForeignKey(
                        name: "FK_NhanVien_ChiNhanhNganHang_MaChiNhanhNavigationMaChiNhanh",
                        column: x => x.MaChiNhanhNavigationMaChiNhanh,
                        principalTable: "ChiNhanhNganHang",
                        principalColumn: "MaChiNhanh");
                });

            migrationBuilder.CreateTable(
                name: "KhachHang",
                columns: table => new
                {
                    MaKh = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HoTen = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CmndCccd = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgaySinh = table.Column<DateOnly>(type: "date", nullable: true),
                    DiaChi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Sdt = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgheNghiep = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TinhTrangHonNhan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DoiTuongVayMaDoiTuongVay = table.Column<int>(type: "int", nullable: true),
                    AnhDinhKem = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KhachHang", x => x.MaKh);
                    table.ForeignKey(
                        name: "FK_KhachHang_DoiTuongVays_DoiTuongVayMaDoiTuongVay",
                        column: x => x.DoiTuongVayMaDoiTuongVay,
                        principalTable: "DoiTuongVays",
                        principalColumn: "MaDoiTuongVay");
                });

            migrationBuilder.CreateTable(
                name: "HopDongVay",
                columns: table => new
                {
                    MaHopDong = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaKh = table.Column<int>(type: "int", nullable: true),
                    MaLoaiVay = table.Column<int>(type: "int", nullable: true),
                    MaLoaiTienTe = table.Column<int>(type: "int", nullable: true),
                    SoTienVay = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    NgayVay = table.Column<DateOnly>(type: "date", nullable: true),
                    NgayHetHan = table.Column<DateOnly>(type: "date", nullable: true),
                    LaiSuat = table.Column<double>(type: "float", nullable: true),
                    HinhThucTra = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MaNv = table.Column<int>(type: "int", nullable: true),
                    TinhTrang = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MaLoaiTienTeNavigationMaLoaiTienTe = table.Column<int>(type: "int", nullable: true),
                    MaLoaiVayNavigationMaLoaiVay = table.Column<int>(type: "int", nullable: true),
                    MaNvNavigationMaNv = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HopDongVay", x => x.MaHopDong);
                    table.ForeignKey(
                        name: "FK_HopDongVay_KhachHang",
                        column: x => x.MaKh,
                        principalTable: "KhachHang",
                        principalColumn: "MaKh",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HopDongVay_LoaiTienTe_MaLoaiTienTeNavigationMaLoaiTienTe",
                        column: x => x.MaLoaiTienTeNavigationMaLoaiTienTe,
                        principalTable: "LoaiTienTe",
                        principalColumn: "MaLoaiTienTe");
                    table.ForeignKey(
                        name: "FK_HopDongVay_LoaiVay_MaLoaiVayNavigationMaLoaiVay",
                        column: x => x.MaLoaiVayNavigationMaLoaiVay,
                        principalTable: "LoaiVay",
                        principalColumn: "MaLoaiVay");
                    table.ForeignKey(
                        name: "FK_HopDongVay_NhanVien_MaNvNavigationMaNv",
                        column: x => x.MaNvNavigationMaNv,
                        principalTable: "NhanVien",
                        principalColumn: "MaNv");
                });

            migrationBuilder.CreateTable(
                name: "TaiKhoanNganHang",
                columns: table => new
                {
                    MaTaiKhoan = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaKh = table.Column<int>(type: "int", nullable: true),
                    SoTaiKhoan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LoaiTaiKhoan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SoDu = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MaKhNavigationMaKh = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaiKhoanNganHang", x => x.MaTaiKhoan);
                    table.ForeignKey(
                        name: "FK_TaiKhoanNganHang_KhachHang_MaKhNavigationMaKh",
                        column: x => x.MaKhNavigationMaKh,
                        principalTable: "KhachHang",
                        principalColumn: "MaKh");
                });

            migrationBuilder.CreateTable(
                name: "LichTraNo",
                columns: table => new
                {
                    MaLich = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaHopDong = table.Column<int>(type: "int", nullable: true),
                    KyHanThu = table.Column<int>(type: "int", nullable: true),
                    NgayTra = table.Column<DateOnly>(type: "date", nullable: true),
                    SoTienPhaiTra = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MaHopDongNavigationMaHopDong = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LichTraNo", x => x.MaLich);
                    table.ForeignKey(
                        name: "FK_LichTraNo_HopDongVay_MaHopDongNavigationMaHopDong",
                        column: x => x.MaHopDongNavigationMaHopDong,
                        principalTable: "HopDongVay",
                        principalColumn: "MaHopDong");
                });

            migrationBuilder.CreateTable(
                name: "TaiSanTheChap",
                columns: table => new
                {
                    MaTs = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaHopDong = table.Column<int>(type: "int", nullable: true),
                    TenTaiSan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GiaTri = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MoTa = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TinhTrang = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MaHopDongNavigationMaHopDong = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaiSanTheChap", x => x.MaTs);
                    table.ForeignKey(
                        name: "FK_TaiSanTheChap_HopDongVay_MaHopDongNavigationMaHopDong",
                        column: x => x.MaHopDongNavigationMaHopDong,
                        principalTable: "HopDongVay",
                        principalColumn: "MaHopDong");
                });

            migrationBuilder.CreateTable(
                name: "GiaoDich",
                columns: table => new
                {
                    MaGiaoDich = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaTaiKhoan = table.Column<int>(type: "int", nullable: true),
                    NgayGd = table.Column<DateOnly>(type: "date", nullable: true),
                    SoTienGd = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    LoaiGd = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NoiDungGd = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MaTaiKhoanNavigationMaTaiKhoan = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GiaoDich", x => x.MaGiaoDich);
                    table.ForeignKey(
                        name: "FK_GiaoDich_TaiKhoanNganHang_MaTaiKhoanNavigationMaTaiKhoan",
                        column: x => x.MaTaiKhoanNavigationMaTaiKhoan,
                        principalTable: "TaiKhoanNganHang",
                        principalColumn: "MaTaiKhoan");
                });

            migrationBuilder.CreateTable(
                name: "ThanhToanLichTra",
                columns: table => new
                {
                    MaThanhToan = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaLich = table.Column<int>(type: "int", nullable: true),
                    NgayThanhToan = table.Column<DateOnly>(type: "date", nullable: true),
                    SoTienThanhToan = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    HinhThucThanhToan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SoTaiKhoanGd = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MaLichNavigationMaLich = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThanhToanLichTra", x => x.MaThanhToan);
                    table.ForeignKey(
                        name: "FK_ThanhToanLichTra_LichTraNo_MaLichNavigationMaLich",
                        column: x => x.MaLichNavigationMaLich,
                        principalTable: "LichTraNo",
                        principalColumn: "MaLich");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_GiaoDich_MaTaiKhoanNavigationMaTaiKhoan",
                table: "GiaoDich",
                column: "MaTaiKhoanNavigationMaTaiKhoan");

            migrationBuilder.CreateIndex(
                name: "IX_HopDongVay_MaKh",
                table: "HopDongVay",
                column: "MaKh");

            migrationBuilder.CreateIndex(
                name: "IX_HopDongVay_MaLoaiTienTeNavigationMaLoaiTienTe",
                table: "HopDongVay",
                column: "MaLoaiTienTeNavigationMaLoaiTienTe");

            migrationBuilder.CreateIndex(
                name: "IX_HopDongVay_MaLoaiVayNavigationMaLoaiVay",
                table: "HopDongVay",
                column: "MaLoaiVayNavigationMaLoaiVay");

            migrationBuilder.CreateIndex(
                name: "IX_HopDongVay_MaNvNavigationMaNv",
                table: "HopDongVay",
                column: "MaNvNavigationMaNv");

            migrationBuilder.CreateIndex(
                name: "IX_KhachHang_DoiTuongVayMaDoiTuongVay",
                table: "KhachHang",
                column: "DoiTuongVayMaDoiTuongVay");

            migrationBuilder.CreateIndex(
                name: "IX_LichTraNo_MaHopDongNavigationMaHopDong",
                table: "LichTraNo",
                column: "MaHopDongNavigationMaHopDong");

            migrationBuilder.CreateIndex(
                name: "IX_NhanVien_MaChiNhanhNavigationMaChiNhanh",
                table: "NhanVien",
                column: "MaChiNhanhNavigationMaChiNhanh");

            migrationBuilder.CreateIndex(
                name: "IX_TaiKhoanNganHang_MaKhNavigationMaKh",
                table: "TaiKhoanNganHang",
                column: "MaKhNavigationMaKh");

            migrationBuilder.CreateIndex(
                name: "IX_TaiSanTheChap_MaHopDongNavigationMaHopDong",
                table: "TaiSanTheChap",
                column: "MaHopDongNavigationMaHopDong");

            migrationBuilder.CreateIndex(
                name: "IX_ThanhToanLichTra_MaLichNavigationMaLich",
                table: "ThanhToanLichTra",
                column: "MaLichNavigationMaLich");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "GiaoDich");

            migrationBuilder.DropTable(
                name: "TaiSanTheChap");

            migrationBuilder.DropTable(
                name: "ThanhToanLichTra");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "TaiKhoanNganHang");

            migrationBuilder.DropTable(
                name: "LichTraNo");

            migrationBuilder.DropTable(
                name: "HopDongVay");

            migrationBuilder.DropTable(
                name: "KhachHang");

            migrationBuilder.DropTable(
                name: "LoaiTienTe");

            migrationBuilder.DropTable(
                name: "LoaiVay");

            migrationBuilder.DropTable(
                name: "NhanVien");

            migrationBuilder.DropTable(
                name: "DoiTuongVays");

            migrationBuilder.DropTable(
                name: "ChiNhanhNganHang");
        }
    }
}
