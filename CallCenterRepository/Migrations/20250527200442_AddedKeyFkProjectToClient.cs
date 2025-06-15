using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CallCenterRepository.Migrations
{
    /// <inheritdoc />
    public partial class AddedKeyFkProjectToClient : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Clients_Projects_ProjectPkProject",
                table: "Clients");

            migrationBuilder.DropIndex(
                name: "IX_Clients_ProjectPkProject",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "ProjectPkProject",
                table: "Clients");

            migrationBuilder.AddColumn<int>(
                name: "FkProject",
                table: "Clients",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Clients_FkProject",
                table: "Clients",
                column: "FkProject");

            migrationBuilder.AddForeignKey(
                name: "FK_Clients_Projects_FkProject",
                table: "Clients",
                column: "FkProject",
                principalTable: "Projects",
                principalColumn: "PkProject",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Clients_Projects_FkProject",
                table: "Clients");

            migrationBuilder.DropIndex(
                name: "IX_Clients_FkProject",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "FkProject",
                table: "Clients");

            migrationBuilder.AddColumn<int>(
                name: "ProjectPkProject",
                table: "Clients",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clients_ProjectPkProject",
                table: "Clients",
                column: "ProjectPkProject");

            migrationBuilder.AddForeignKey(
                name: "FK_Clients_Projects_ProjectPkProject",
                table: "Clients",
                column: "ProjectPkProject",
                principalTable: "Projects",
                principalColumn: "PkProject");
        }
    }
}
