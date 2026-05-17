using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AuthService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RecreateAuth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "api_clients",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_api_clients", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "api_scopes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_api_scopes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "api_keys",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClientId = table.Column<Guid>(type: "uuid", nullable: false),
                    KeyPrefix = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    KeyHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RevokedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RevokedReason = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    LastUsedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_api_keys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_api_keys_api_clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "api_clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "api_key_scopes",
                columns: table => new
                {
                    ApiKeyId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScopeId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_api_key_scopes", x => new { x.ApiKeyId, x.ScopeId });
                    table.ForeignKey(
                        name: "FK_api_key_scopes_api_keys_ApiKeyId",
                        column: x => x.ApiKeyId,
                        principalTable: "api_keys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_api_key_scopes_api_scopes_ScopeId",
                        column: x => x.ScopeId,
                        principalTable: "api_scopes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "api_scopes",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[,]
                {
                    { new Guid("a4df7a08-1f9b-4470-a76c-f90ccfdb2f1b"), "Read transfer data", "transfer.read" },
                    { new Guid("ca12d43d-c1b2-4dda-a4f9-f3349a95b12d"), "Write transfer data", "transfer.write" },
                    { new Guid("d11dc76f-c426-4ef8-bf0d-4d9fb4e14501"), "Read customer data", "customer.read" },
                    { new Guid("da1842c9-13a4-414d-ae47-882ee3964e18"), "Write customer data", "customer.write" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_api_clients_Name",
                table: "api_clients",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_api_key_scopes_ScopeId",
                table: "api_key_scopes",
                column: "ScopeId");

            migrationBuilder.CreateIndex(
                name: "IX_api_keys_ClientId",
                table: "api_keys",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_api_keys_KeyHash",
                table: "api_keys",
                column: "KeyHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_api_scopes_Name",
                table: "api_scopes",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "api_key_scopes");

            migrationBuilder.DropTable(
                name: "api_keys");

            migrationBuilder.DropTable(
                name: "api_scopes");

            migrationBuilder.DropTable(
                name: "api_clients");
        }
    }
}
