using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace HotelReservationSystemApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RolePermissionSeeding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RolePermissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Permission = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "Id", "Permission", "RoleId" },
                values: new object[,]
                {
                    { new Guid("31b1a566-3a08-41df-bf87-f4db967d6cdb"), "read:users", new Guid("a1ea0823-516d-4441-9a40-16e8b7649171") },
                    { new Guid("5ac03c4f-759d-406b-aefc-3cf7fa22954a"), "read:profile", new Guid("844eb56d-ea3a-4f71-abd5-3f648ed9d61b") },
                    { new Guid("6b1fa29e-495a-4814-a5c4-4d9e37e4ef44"), "read:profile", new Guid("a1ea0823-516d-4441-9a40-16e8b7649171") },
                    { new Guid("94be7cb5-cddb-4174-b010-c0e0bf99fcc7"), "write:booking", new Guid("a1ea0823-516d-4441-9a40-16e8b7649171") },
                    { new Guid("c29b95b1-5b0c-416e-8086-74c37c304411"), "*", new Guid("34f40151-388b-434f-8b34-910ef9c6098b") }
                });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("34f40151-388b-434f-8b34-910ef9c6098b"),
                columns: new[] { "ConcurrencyStamp", "CreatedAt" },
                values: new object[] { "seed-superadmin", new DateTime(2025, 11, 12, 9, 38, 45, 412, DateTimeKind.Utc).AddTicks(3562) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("844eb56d-ea3a-4f71-abd5-3f648ed9d61b"),
                columns: new[] { "ConcurrencyStamp", "CreatedAt" },
                values: new object[] { "seed-guest", new DateTime(2025, 11, 12, 9, 38, 45, 412, DateTimeKind.Utc).AddTicks(3562) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("a1ea0823-516d-4441-9a40-16e8b7649171"),
                columns: new[] { "ConcurrencyStamp", "CreatedAt" },
                values: new object[] { "seed-admin", new DateTime(2025, 11, 12, 9, 38, 45, 412, DateTimeKind.Utc).AddTicks(3562) });

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_RoleId",
                table: "RolePermissions",
                column: "RoleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RolePermissions");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("34f40151-388b-434f-8b34-910ef9c6098b"),
                columns: new[] { "ConcurrencyStamp", "CreatedAt" },
                values: new object[] { "fixed-concurrency-stamp-superadmin", new DateTime(2025, 10, 12, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("844eb56d-ea3a-4f71-abd5-3f648ed9d61b"),
                columns: new[] { "ConcurrencyStamp", "CreatedAt" },
                values: new object[] { "fixed-concurrency-stamp-guest", new DateTime(2025, 10, 12, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("a1ea0823-516d-4441-9a40-16e8b7649171"),
                columns: new[] { "ConcurrencyStamp", "CreatedAt" },
                values: new object[] { "fixed-concurrency-stamp-hoteladmin", new DateTime(2025, 10, 12, 0, 0, 0, 0, DateTimeKind.Unspecified) });
        }
    }
}
