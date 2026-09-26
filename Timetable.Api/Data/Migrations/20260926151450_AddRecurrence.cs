using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Timetable.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRecurrence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "recurrence_rule",
                table: "timetable_events",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "time_zone_id",
                table: "timetable_events",
                type: "TEXT",
                nullable: false,
                defaultValue: "Asia/Hong_Kong");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "recurrence_rule",
                table: "timetable_events");

            migrationBuilder.DropColumn(
                name: "time_zone_id",
                table: "timetable_events");
        }
    }
}
