using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FountainFlow.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddRankPropertyToArchetype : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Rank",
                table: "Archetypes",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Rank",
                table: "Archetypes");
        }
    }
}
