using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CallCenterRepository.Migrations
{
    /// <inheritdoc />
    public partial class NullableFkAddedAtProjectStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectStatuses_ProjectStatusGroups_FkStatusGroup",
                table: "ProjectStatuses");

            migrationBuilder.AlterColumn<int>(
                name: "FkStatusGroup",
                table: "ProjectStatuses",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectStatuses_ProjectStatusGroups_FkStatusGroup",
                table: "ProjectStatuses",
                column: "FkStatusGroup",
                principalTable: "ProjectStatusGroups",
                principalColumn: "PkStatusGroup");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectStatuses_ProjectStatusGroups_FkStatusGroup",
                table: "ProjectStatuses");

            migrationBuilder.AlterColumn<int>(
                name: "FkStatusGroup",
                table: "ProjectStatuses",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectStatuses_ProjectStatusGroups_FkStatusGroup",
                table: "ProjectStatuses",
                column: "FkStatusGroup",
                principalTable: "ProjectStatusGroups",
                principalColumn: "PkStatusGroup",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
