using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CallCenterRepository.Migrations
{
    /// <inheritdoc />
    public partial class FixedClientNoteRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Admins",
                columns: table => new
                {
                    PkAdmin = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Username = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Password = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Admins", x => x.PkAdmin);
                });

            migrationBuilder.CreateTable(
                name: "Operators",
                columns: table => new
                {
                    PkOperator = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Username = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Password = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    FkAdmin = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Operators", x => x.PkOperator);
                    table.ForeignKey(
                        name: "FK_Operators_Admins_FkAdmin",
                        column: x => x.FkAdmin,
                        principalTable: "Admins",
                        principalColumn: "PkAdmin",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    PkProject = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProjectName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProjectStatus = table.Column<bool>(type: "boolean", nullable: false),
                    ScriptText = table.Column<string>(type: "text", nullable: true),
                    CallInterval = table.Column<int>(type: "integer", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    TimeZoneOffset = table.Column<int>(type: "integer", nullable: false),
                    Closed = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FkAdmin = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.PkProject);
                    table.ForeignKey(
                        name: "FK_Projects_Admins_FkAdmin",
                        column: x => x.FkAdmin,
                        principalTable: "Admins",
                        principalColumn: "PkAdmin",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Clients",
                columns: table => new
                {
                    PkClient = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PhoneNumber = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ProjectPkProject = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clients", x => x.PkClient);
                    table.ForeignKey(
                        name: "FK_Clients_Projects_ProjectPkProject",
                        column: x => x.ProjectPkProject,
                        principalTable: "Projects",
                        principalColumn: "PkProject");
                });

            migrationBuilder.CreateTable(
                name: "OperatorProjects",
                columns: table => new
                {
                    FkOperator = table.Column<int>(type: "integer", nullable: false),
                    FkProject = table.Column<int>(type: "integer", nullable: false),
                    PkOperatorProject = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AssignedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperatorProjects", x => new { x.FkOperator, x.FkProject });
                    table.ForeignKey(
                        name: "FK_OperatorProjects_Operators_FkOperator",
                        column: x => x.FkOperator,
                        principalTable: "Operators",
                        principalColumn: "PkOperator",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OperatorProjects_Projects_FkProject",
                        column: x => x.FkProject,
                        principalTable: "Projects",
                        principalColumn: "PkProject",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectStatusGroups",
                columns: table => new
                {
                    PkStatusGroup = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StatusGroupName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Mandatory = table.Column<bool>(type: "boolean", nullable: false),
                    FkProject = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectStatusGroups", x => x.PkStatusGroup);
                    table.ForeignKey(
                        name: "FK_ProjectStatusGroups_Projects_FkProject",
                        column: x => x.FkProject,
                        principalTable: "Projects",
                        principalColumn: "PkProject",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Conversations",
                columns: table => new
                {
                    PkTalk = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TimeStarted = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TimeEnded = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PathToAudio = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    FkOperator = table.Column<int>(type: "integer", nullable: false),
                    FkClient = table.Column<int>(type: "integer", nullable: false),
                    FkProject = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Conversations", x => x.PkTalk);
                    table.ForeignKey(
                        name: "FK_Conversations_Clients_FkClient",
                        column: x => x.FkClient,
                        principalTable: "Clients",
                        principalColumn: "PkClient",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Conversations_Operators_FkOperator",
                        column: x => x.FkOperator,
                        principalTable: "Operators",
                        principalColumn: "PkOperator",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Conversations_Projects_FkProject",
                        column: x => x.FkProject,
                        principalTable: "Projects",
                        principalColumn: "PkProject",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectStatuses",
                columns: table => new
                {
                    PkProjectStatus = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StatusName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FkStatusGroup = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectStatuses", x => x.PkProjectStatus);
                    table.ForeignKey(
                        name: "FK_ProjectStatuses_ProjectStatusGroups_FkStatusGroup",
                        column: x => x.FkStatusGroup,
                        principalTable: "ProjectStatusGroups",
                        principalColumn: "PkStatusGroup",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClientNotes",
                columns: table => new
                {
                    PkNote = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DateWritten = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Text = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    FkConversation = table.Column<int>(type: "integer", nullable: false),
                    ConversationPkTalk = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientNotes", x => x.PkNote);
                    table.ForeignKey(
                        name: "FK_ClientNotes_Conversations_ConversationPkTalk",
                        column: x => x.ConversationPkTalk,
                        principalTable: "Conversations",
                        principalColumn: "PkTalk");
                    table.ForeignKey(
                        name: "FK_ClientNotes_Conversations_FkConversation",
                        column: x => x.FkConversation,
                        principalTable: "Conversations",
                        principalColumn: "PkTalk",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConversationGrades",
                columns: table => new
                {
                    PkGrade = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GradeType = table.Column<int>(type: "integer", nullable: false),
                    Score = table.Column<int>(type: "integer", nullable: false),
                    FkConversation = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConversationGrades", x => x.PkGrade);
                    table.ForeignKey(
                        name: "FK_ConversationGrades_Conversations_FkConversation",
                        column: x => x.FkConversation,
                        principalTable: "Conversations",
                        principalColumn: "PkTalk",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConversationStatuses",
                columns: table => new
                {
                    FkTalk = table.Column<int>(type: "integer", nullable: false),
                    FkProjectStatus = table.Column<int>(type: "integer", nullable: false),
                    PkConversationStatus = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SelectedTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConversationStatuses", x => new { x.FkTalk, x.FkProjectStatus });
                    table.ForeignKey(
                        name: "FK_ConversationStatuses_Conversations_FkTalk",
                        column: x => x.FkTalk,
                        principalTable: "Conversations",
                        principalColumn: "PkTalk",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ConversationStatuses_ProjectStatuses_FkProjectStatus",
                        column: x => x.FkProjectStatus,
                        principalTable: "ProjectStatuses",
                        principalColumn: "PkProjectStatus",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Admins_Username",
                table: "Admins",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClientNotes_ConversationPkTalk",
                table: "ClientNotes",
                column: "ConversationPkTalk");

            migrationBuilder.CreateIndex(
                name: "IX_ClientNotes_FkConversation",
                table: "ClientNotes",
                column: "FkConversation");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_ProjectPkProject",
                table: "Clients",
                column: "ProjectPkProject");

            migrationBuilder.CreateIndex(
                name: "IX_ConversationGrades_FkConversation",
                table: "ConversationGrades",
                column: "FkConversation");

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_FkClient",
                table: "Conversations",
                column: "FkClient");

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_FkOperator",
                table: "Conversations",
                column: "FkOperator");

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_FkProject",
                table: "Conversations",
                column: "FkProject");

            migrationBuilder.CreateIndex(
                name: "IX_ConversationStatuses_FkProjectStatus",
                table: "ConversationStatuses",
                column: "FkProjectStatus");

            migrationBuilder.CreateIndex(
                name: "IX_OperatorProjects_FkProject",
                table: "OperatorProjects",
                column: "FkProject");

            migrationBuilder.CreateIndex(
                name: "IX_Operators_FkAdmin",
                table: "Operators",
                column: "FkAdmin");

            migrationBuilder.CreateIndex(
                name: "IX_Operators_Username",
                table: "Operators",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Projects_FkAdmin",
                table: "Projects",
                column: "FkAdmin");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectStatuses_FkStatusGroup",
                table: "ProjectStatuses",
                column: "FkStatusGroup");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectStatusGroups_FkProject",
                table: "ProjectStatusGroups",
                column: "FkProject");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClientNotes");

            migrationBuilder.DropTable(
                name: "ConversationGrades");

            migrationBuilder.DropTable(
                name: "ConversationStatuses");

            migrationBuilder.DropTable(
                name: "OperatorProjects");

            migrationBuilder.DropTable(
                name: "Conversations");

            migrationBuilder.DropTable(
                name: "ProjectStatuses");

            migrationBuilder.DropTable(
                name: "Clients");

            migrationBuilder.DropTable(
                name: "Operators");

            migrationBuilder.DropTable(
                name: "ProjectStatusGroups");

            migrationBuilder.DropTable(
                name: "Projects");

            migrationBuilder.DropTable(
                name: "Admins");
        }
    }
}
