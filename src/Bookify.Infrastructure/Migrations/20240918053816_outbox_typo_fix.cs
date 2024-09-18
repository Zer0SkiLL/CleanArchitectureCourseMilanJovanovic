using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bookify.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class outbox_typo_fix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "occured_on_utc",
                table: "outbox_messages",
                newName: "occurred_on_utc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "occurred_on_utc",
                table: "outbox_messages",
                newName: "occured_on_utc");
        }
    }
}
