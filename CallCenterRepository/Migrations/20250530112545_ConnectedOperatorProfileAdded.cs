using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CallCenterRepository.Migrations
{
    /// <inheritdoc />
    public partial class ConnectedOperatorProfileAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SelfOperatorProfile",
                table: "Admins",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SelfOperatorProfile",
                table: "Admins");
        }
    }
}
