using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace XYPortal.PasswordBook.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueNameIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PasswordBooks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OwnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    PasswordFormatJson = table.Column<string>(type: "text", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasswordBooks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PasswordEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PasswordBookId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    HasUsername = table.Column<bool>(type: "boolean", nullable: false),
                    Username = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PasswordType = table.Column<int>(type: "integer", nullable: false),
                    WeakLevel = table.Column<int>(type: "integer", nullable: true),
                    CurrentPassword = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Remark = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasswordEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PasswordEntries_PasswordBooks_PasswordBookId",
                        column: x => x.PasswordBookId,
                        principalTable: "PasswordBooks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PasswordHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PasswordEntryId = table.Column<Guid>(type: "uuid", nullable: false),
                    PasswordValue = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    IsCurrent = table.Column<bool>(type: "boolean", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasswordHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PasswordHistories_PasswordEntries_PasswordEntryId",
                        column: x => x.PasswordEntryId,
                        principalTable: "PasswordEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PasswordBooks_IsDeleted",
                table: "PasswordBooks",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_PasswordBooks_OwnerId",
                table: "PasswordBooks",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_PasswordBooks_OwnerId_Name",
                table: "PasswordBooks",
                columns: new[] { "OwnerId", "Name" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_PasswordEntries_IsDeleted",
                table: "PasswordEntries",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_PasswordEntries_PasswordBookId",
                table: "PasswordEntries",
                column: "PasswordBookId");

            migrationBuilder.CreateIndex(
                name: "IX_PasswordHistories_IsCurrent",
                table: "PasswordHistories",
                column: "IsCurrent");

            migrationBuilder.CreateIndex(
                name: "IX_PasswordHistories_PasswordEntryId",
                table: "PasswordHistories",
                column: "PasswordEntryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PasswordHistories");

            migrationBuilder.DropTable(
                name: "PasswordEntries");

            migrationBuilder.DropTable(
                name: "PasswordBooks");
        }
    }
}
