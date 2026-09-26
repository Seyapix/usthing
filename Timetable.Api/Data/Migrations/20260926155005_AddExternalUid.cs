using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Timetable.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddExternalUid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "external_uid",
                table: "timetable_events",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_timetable_events_user_id_external_uid",
                table: "timetable_events",
                columns: new[] { "user_id", "external_uid" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_timetable_events_user_id_external_uid",
                table: "timetable_events");

            migrationBuilder.DropColumn(
                name: "external_uid",
                table: "timetable_events");
        }
    }
}
