using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace C_C_Proj_WebStore.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class editProductTable2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Model",
                table: "Products",
                newName: "ShoeModel");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ShoeModel",
                table: "Products",
                newName: "Model");
        }
    }
}
