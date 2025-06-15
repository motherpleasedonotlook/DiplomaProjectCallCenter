using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CallCenterRepository.Migrations
{
    /// <inheritdoc />
    public partial class MandatoryDeleted : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Mandatory",
                table: "ProjectStatusGroups");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastUpdate",
                table: "Projects",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastUpdate",
                table: "Projects");

            migrationBuilder.AddColumn<bool>(
                name: "Mandatory",
                table: "ProjectStatusGroups",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
