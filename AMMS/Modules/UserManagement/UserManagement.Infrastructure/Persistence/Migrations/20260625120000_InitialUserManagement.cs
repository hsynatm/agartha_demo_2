using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserManagement.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialUserManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "UserManagement");

            migrationBuilder.CreateTable(
                name: "Users",
                schema: "UserManagement",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Username = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    OrganizationId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    OrganizationName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Role = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                schema: "UserManagement",
                table: "Users",
                column: "Username",
                unique: true);

            migrationBuilder.InsertData(
                schema: "UserManagement",
                table: "Users",
                columns: new[]
                {
                    "Id", "Username", "PasswordHash", "Email", "FirstName", "LastName",
                    "OrganizationId", "OrganizationName", "Role", "IsActive",
                    "CreatedAt", "CreatedBy", "UpdatedAt", "UpdatedBy", "IsDeleted"
                },
                values: new object[]
                {
                    new Guid("11111111-1111-1111-1111-111111111111"),
                    "admin",
                    "$2a$11$8y6m6bwOs9Ah4FviNfs07OI7LCrQ2fLsVDqwzcAXcBWNwK8sSgLUe",
                    "admin@amms.local",
                    "System",
                    "Admin",
                    "1",
                    "Test Organization",
                    "admin",
                    true,
                    new DateTime(2026, 6, 25, 0, 0, 0, DateTimeKind.Utc),
                    null,
                    null,
                    null,
                    false
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Users",
                schema: "UserManagement");
        }
    }
}
