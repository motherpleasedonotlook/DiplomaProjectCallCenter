using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CallCenterRepository.Migrations
{
    /// <inheritdoc />
    public partial class thirdMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClientNotes_Conversations_ConversationPkTalk",
                table: "ClientNotes");

            migrationBuilder.DropIndex(
                name: "IX_ClientNotes_ConversationPkTalk",
                table: "ClientNotes");

            migrationBuilder.DropColumn(
                name: "ConversationPkTalk",
                table: "ClientNotes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ConversationPkTalk",
                table: "ClientNotes",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClientNotes_ConversationPkTalk",
                table: "ClientNotes",
                column: "ConversationPkTalk");

            migrationBuilder.AddForeignKey(
                name: "FK_ClientNotes_Conversations_ConversationPkTalk",
                table: "ClientNotes",
                column: "ConversationPkTalk",
                principalTable: "Conversations",
                principalColumn: "PkTalk");
        }
    }
}
