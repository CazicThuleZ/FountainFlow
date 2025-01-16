using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FountainFlow.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddPromptAndHierarchicalSequence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ArchetypeBeats_ArchetypeId",
                table: "ArchetypeBeats");

            migrationBuilder.RenameColumn(
                name: "Sequence",
                table: "ArchetypeBeats",
                newName: "ParentSequence");

            migrationBuilder.AddColumn<int>(
                name: "ChildSequence",
                table: "ArchetypeBeats",
                type: "integer",
                nullable: true,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GrandchildSequence",
                table: "ArchetypeBeats",
                type: "integer",
                nullable: true,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Prompt",
                table: "ArchetypeBeats",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ArchetypeBeats_ArchetypeId_ParentSequence_ChildSequence_Gra~",
                table: "ArchetypeBeats",
                columns: new[] { "ArchetypeId", "ParentSequence", "ChildSequence", "GrandchildSequence" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ArchetypeBeats_ArchetypeId_ParentSequence_ChildSequence_Gra~",
                table: "ArchetypeBeats");

            migrationBuilder.DropColumn(
                name: "ChildSequence",
                table: "ArchetypeBeats");

            migrationBuilder.DropColumn(
                name: "GrandchildSequence",
                table: "ArchetypeBeats");

            migrationBuilder.DropColumn(
                name: "Prompt",
                table: "ArchetypeBeats");

            migrationBuilder.RenameColumn(
                name: "ParentSequence",
                table: "ArchetypeBeats",
                newName: "Sequence");

            migrationBuilder.CreateIndex(
                name: "IX_ArchetypeBeats_ArchetypeId",
                table: "ArchetypeBeats",
                column: "ArchetypeId");
        }
    }
}
