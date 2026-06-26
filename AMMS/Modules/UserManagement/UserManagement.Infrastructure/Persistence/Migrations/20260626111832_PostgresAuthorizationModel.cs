using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserManagement.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PostgresAuthorizationModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PasswordHash",
                schema: "UserManagement",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Role",
                schema: "UserManagement",
                table: "Users");

            migrationBuilder.AddColumn<string>(
                name: "KeycloakUserId",
                schema: "UserManagement",
                table: "Users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RoleGroups",
                schema: "UserManagement",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                schema: "UserManagement",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserRoleGroups",
                schema: "UserManagement",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleGroupId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoleGroups", x => new { x.UserId, x.RoleGroupId });
                    table.ForeignKey(
                        name: "FK_UserRoleGroups_RoleGroups_RoleGroupId",
                        column: x => x.RoleGroupId,
                        principalSchema: "UserManagement",
                        principalTable: "RoleGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRoleGroups_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "UserManagement",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RoleGroupRoles",
                schema: "UserManagement",
                columns: table => new
                {
                    RoleGroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleGroupRoles", x => new { x.RoleGroupId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_RoleGroupRoles_RoleGroups_RoleGroupId",
                        column: x => x.RoleGroupId,
                        principalSchema: "UserManagement",
                        principalTable: "RoleGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoleGroupRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "UserManagement",
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                schema: "UserManagement",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_UserRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "UserManagement",
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRoles_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "UserManagement",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_KeycloakUserId",
                schema: "UserManagement",
                table: "Users",
                column: "KeycloakUserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RoleGroupRoles_RoleId",
                schema: "UserManagement",
                table: "RoleGroupRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleGroups_Code",
                schema: "UserManagement",
                table: "RoleGroups",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Roles_Code",
                schema: "UserManagement",
                table: "Roles",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserRoleGroups_RoleGroupId",
                schema: "UserManagement",
                table: "UserRoleGroups",
                column: "RoleGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_RoleId",
                schema: "UserManagement",
                table: "UserRoles",
                column: "RoleId");

            var seedTime = new DateTime(2026, 6, 25, 0, 0, 0, DateTimeKind.Utc);
            var adminUserId = new Guid("11111111-1111-1111-1111-111111111111");
            var roleAdminId = new Guid("22222222-2222-2222-2222-222222222201");
            var roleOperatorId = new Guid("22222222-2222-2222-2222-222222222202");
            var roleViewerId = new Guid("22222222-2222-2222-2222-222222222203");
            var roleTest1Id = new Guid("22222222-2222-2222-2222-222222222204");
            var roleTest2Id = new Guid("22222222-2222-2222-2222-222222222205");
            var roleGroupUserManagementId = new Guid("33333333-3333-3333-3333-333333333301");

            migrationBuilder.InsertData(
                schema: "UserManagement",
                table: "Roles",
                columns: new[]
                {
                    "Id", "Code", "Name", "Description", "IsActive",
                    "CreatedAt", "CreatedBy", "UpdatedAt", "UpdatedBy", "IsDeleted"
                },
                values: new object[,]
                {
                    { roleAdminId, "admin", "Administrator", "System administrator", true, seedTime, null, null, null, false },
                    { roleOperatorId, "operator", "Operator", "Operations user", true, seedTime, null, null, null, false },
                    { roleViewerId, "viewer", "Viewer", "Read-only user", true, seedTime, null, null, null, false },
                    { roleTest1Id, "test_rol1", "Test Role 1", null, true, seedTime, null, null, null, false },
                    { roleTest2Id, "test_rol2", "Test Role 2", null, true, seedTime, null, null, null, false }
                });

            migrationBuilder.InsertData(
                schema: "UserManagement",
                table: "RoleGroups",
                columns: new[]
                {
                    "Id", "Code", "Name", "Description", "IsActive",
                    "CreatedAt", "CreatedBy", "UpdatedAt", "UpdatedBy", "IsDeleted"
                },
                values: new object[]
                {
                    roleGroupUserManagementId, "UserManagement", "User Management", "Can manage users, roles and role groups",
                    true, seedTime, null, null, null, false
                });

            migrationBuilder.InsertData(
                schema: "UserManagement",
                table: "RoleGroupRoles",
                columns: new[] { "RoleGroupId", "RoleId" },
                values: new object[,]
                {
                    { roleGroupUserManagementId, roleAdminId },
                    { roleGroupUserManagementId, roleTest1Id },
                    { roleGroupUserManagementId, roleTest2Id }
                });

            migrationBuilder.InsertData(
                schema: "UserManagement",
                table: "UserRoles",
                columns: new[] { "UserId", "RoleId" },
                values: new object[] { adminUserId, roleAdminId });

            migrationBuilder.InsertData(
                schema: "UserManagement",
                table: "UserRoleGroups",
                columns: new[] { "UserId", "RoleGroupId" },
                values: new object[] { adminUserId, roleGroupUserManagementId });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RoleGroupRoles",
                schema: "UserManagement");

            migrationBuilder.DropTable(
                name: "UserRoleGroups",
                schema: "UserManagement");

            migrationBuilder.DropTable(
                name: "UserRoles",
                schema: "UserManagement");

            migrationBuilder.DropTable(
                name: "RoleGroups",
                schema: "UserManagement");

            migrationBuilder.DropTable(
                name: "Roles",
                schema: "UserManagement");

            migrationBuilder.DropIndex(
                name: "IX_Users_KeycloakUserId",
                schema: "UserManagement",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "KeycloakUserId",
                schema: "UserManagement",
                table: "Users");

            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                schema: "UserManagement",
                table: "Users",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Role",
                schema: "UserManagement",
                table: "Users",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");
        }
    }
}
