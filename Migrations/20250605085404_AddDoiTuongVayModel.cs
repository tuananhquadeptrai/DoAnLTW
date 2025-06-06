using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VAYTIEN.Migrations
{
    /// <inheritdoc />
    public partial class AddDoiTuongVayModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DoiTuongVayMaDoiTuongVay",
                table: "KhachHang",
                type: "int",
                nullable: true);

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

            migrationBuilder.CreateIndex(
                name: "IX_KhachHang_DoiTuongVayMaDoiTuongVay",
                table: "KhachHang",
                column: "DoiTuongVayMaDoiTuongVay");

            migrationBuilder.AddForeignKey(
                name: "FK_KhachHang_DoiTuongVays_DoiTuongVayMaDoiTuongVay",
                table: "KhachHang",
                column: "DoiTuongVayMaDoiTuongVay",
                principalTable: "DoiTuongVays",
                principalColumn: "MaDoiTuongVay");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_KhachHang_DoiTuongVays_DoiTuongVayMaDoiTuongVay",
                table: "KhachHang");

            migrationBuilder.DropTable(
                name: "DoiTuongVays");

            migrationBuilder.DropIndex(
                name: "IX_KhachHang_DoiTuongVayMaDoiTuongVay",
                table: "KhachHang");

            migrationBuilder.DropColumn(
                name: "DoiTuongVayMaDoiTuongVay",
                table: "KhachHang");
        }
    }
}
