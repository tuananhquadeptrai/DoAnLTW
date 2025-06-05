using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VAYTIEN.Migrations
{
    /// <inheritdoc />
    public partial class AddAnhDinhKemToKhachHang : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AnhDinhKem",
                table: "KhachHang",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AnhDinhKem",
                table: "KhachHang");
        }
    }
}
