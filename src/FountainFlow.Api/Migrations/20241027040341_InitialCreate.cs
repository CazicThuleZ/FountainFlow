using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FountainFlow.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Archetypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Domain = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Architect = table.Column<string>(type: "text", nullable: true),
                    ExternalLink = table.Column<string>(type: "text", nullable: true),
                    Icon = table.Column<string>(type: "text", nullable: true),
                    CreatedUTC = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedUTC = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Archetypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Themes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Text = table.Column<string>(type: "text", nullable: false),
                    TagList = table.Column<string>(type: "text", nullable: false),
                    CreatedUTC = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedUTC = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Themes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ArchetypeBeats",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ArchetypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Sequence = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Icon = table.Column<string>(type: "text", nullable: true),
                    PercentOfStory = table.Column<int>(type: "integer", nullable: false),
                    CreatedUTC = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedUTC = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArchetypeBeats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArchetypeBeats_Archetypes_ArchetypeId",
                        column: x => x.ArchetypeId,
                        principalTable: "Archetypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ArchetypeGenres",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ArchetypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Icon = table.Column<string>(type: "text", nullable: true),
                    CreatedUTC = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedUTC = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArchetypeGenres", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArchetypeGenres_Archetypes_ArchetypeId",
                        column: x => x.ArchetypeId,
                        principalTable: "Archetypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LogLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Text = table.Column<string>(type: "text", nullable: false),
                    ArchetypeId = table.Column<Guid>(type: "uuid", nullable: true),
                    ThemeId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedUTC = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedUTC = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LogLines_Archetypes_ArchetypeId",
                        column: x => x.ArchetypeId,
                        principalTable: "Archetypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_LogLines_Themes_ThemeId",
                        column: x => x.ThemeId,
                        principalTable: "Themes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ThemeExtensions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ThemeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Notion = table.Column<string>(type: "text", nullable: false),
                    CreatedUTC = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedUTC = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThemeExtensions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ThemeExtensions_Themes_ThemeId",
                        column: x => x.ThemeId,
                        principalTable: "Themes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Storys",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Author = table.Column<string>(type: "text", nullable: false),
                    DevelopmentStage = table.Column<int>(type: "integer", nullable: false),
                    PublishedUTC = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LogLineId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedUTC = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedUTC = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Storys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Storys_LogLines_LogLineId",
                        column: x => x.LogLineId,
                        principalTable: "LogLines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "StoryLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Sequence = table.Column<int>(type: "integer", nullable: false),
                    LineType = table.Column<string>(type: "text", nullable: false),
                    LineText = table.Column<string>(type: "text", nullable: false),
                    StoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    ArchetypeBeatId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedUTC = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedUTC = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoryLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StoryLines_ArchetypeBeats_ArchetypeBeatId",
                        column: x => x.ArchetypeBeatId,
                        principalTable: "ArchetypeBeats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_StoryLines_Storys_StoryId",
                        column: x => x.StoryId,
                        principalTable: "Storys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ArchetypeBeats_ArchetypeId",
                table: "ArchetypeBeats",
                column: "ArchetypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ArchetypeGenres_ArchetypeId",
                table: "ArchetypeGenres",
                column: "ArchetypeId");

            migrationBuilder.CreateIndex(
                name: "IX_LogLines_ArchetypeId",
                table: "LogLines",
                column: "ArchetypeId");

            migrationBuilder.CreateIndex(
                name: "IX_LogLines_ThemeId",
                table: "LogLines",
                column: "ThemeId");

            migrationBuilder.CreateIndex(
                name: "IX_StoryLines_ArchetypeBeatId",
                table: "StoryLines",
                column: "ArchetypeBeatId");

            migrationBuilder.CreateIndex(
                name: "IX_StoryLines_StoryId",
                table: "StoryLines",
                column: "StoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Storys_LogLineId",
                table: "Storys",
                column: "LogLineId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Storys_Title_DevelopmentStage",
                table: "Storys",
                columns: new[] { "Title", "DevelopmentStage" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ThemeExtensions_ThemeId",
                table: "ThemeExtensions",
                column: "ThemeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArchetypeGenres");

            migrationBuilder.DropTable(
                name: "StoryLines");

            migrationBuilder.DropTable(
                name: "ThemeExtensions");

            migrationBuilder.DropTable(
                name: "ArchetypeBeats");

            migrationBuilder.DropTable(
                name: "Storys");

            migrationBuilder.DropTable(
                name: "LogLines");

            migrationBuilder.DropTable(
                name: "Archetypes");

            migrationBuilder.DropTable(
                name: "Themes");
        }
    }
}
