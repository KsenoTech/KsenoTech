using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace minesweeperAPI.Migrations
{
    /// <inheritdoc />
    public partial class afsf : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "VisibleField",
                table: "Games",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VisibleField",
                table: "Games");
        }
    }
}
