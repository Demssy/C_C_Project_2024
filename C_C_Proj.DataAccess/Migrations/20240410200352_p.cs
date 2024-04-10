using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace C_C_Proj_WebStore.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class p : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 8,
                column: "ShoeModel",
                value: "x Aimé Leon Dore");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 8,
                column: "ShoeModel",
                value: "x Aimé Leon Dore 860v2");
        }
    }
}
