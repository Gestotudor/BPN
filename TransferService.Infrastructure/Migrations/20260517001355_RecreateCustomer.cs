using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TransferService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RecreateCustomer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "idempotency_records",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IdempotencyKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RequestHash = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ResponseJson = table.Column<string>(type: "text", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_idempotency_records", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "transfers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SenderCustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReceiverCustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    TryAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ExchangeRate = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                    Fee = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TransactionCode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    FraudRiskLevel = table.Column<int>(type: "integer", nullable: false),
                    ApprovalAvailableAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transfers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "transfer_fee_refunds",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TransferId = table.Column<Guid>(type: "uuid", nullable: false),
                    RefundedFeeAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    RefundedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transfer_fee_refunds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_transfer_fee_refunds_transfers_TransferId",
                        column: x => x.TransferId,
                        principalTable: "transfers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "transfer_status_histories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TransferId = table.Column<Guid>(type: "uuid", nullable: false),
                    OldStatus = table.Column<int>(type: "integer", nullable: true),
                    NewStatus = table.Column<int>(type: "integer", nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ChangedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transfer_status_histories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_transfer_status_histories_transfers_TransferId",
                        column: x => x.TransferId,
                        principalTable: "transfers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_idempotency_records_IdempotencyKey",
                table: "idempotency_records",
                column: "IdempotencyKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_transfer_fee_refunds_TransferId",
                table: "transfer_fee_refunds",
                column: "TransferId");

            migrationBuilder.CreateIndex(
                name: "IX_transfer_status_histories_TransferId",
                table: "transfer_status_histories",
                column: "TransferId");

            migrationBuilder.CreateIndex(
                name: "IX_transfers_ReceiverCustomerId",
                table: "transfers",
                column: "ReceiverCustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_transfers_SenderCustomerId",
                table: "transfers",
                column: "SenderCustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_transfers_Status",
                table: "transfers",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_transfers_TransactionCode",
                table: "transfers",
                column: "TransactionCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "idempotency_records");

            migrationBuilder.DropTable(
                name: "transfer_fee_refunds");

            migrationBuilder.DropTable(
                name: "transfer_status_histories");

            migrationBuilder.DropTable(
                name: "transfers");
        }
    }
}
