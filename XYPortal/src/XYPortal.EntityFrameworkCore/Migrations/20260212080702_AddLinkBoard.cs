using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace XYPortal.Migrations
{
    /// <inheritdoc />
    public partial class AddLinkBoard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LinkBoardCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    Description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    Icon = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsPublic = table.Column<bool>(type: "boolean", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ReviewComment = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    DraftOfId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LinkBoardCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LinkBoardLinks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    Icon = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsPublic = table.Column<bool>(type: "boolean", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ReviewComment = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    DraftOfId = table.Column<Guid>(type: "uuid", nullable: true),
                    ExtraProperties = table.Column<string>(type: "text", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LinkBoardLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LinkBoardLinks_LinkBoardCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "LinkBoardCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LinkBoardCategories_DraftOfId",
                table: "LinkBoardCategories",
                column: "DraftOfId");

            migrationBuilder.CreateIndex(
                name: "IX_LinkBoardCategories_IsPublic_Status",
                table: "LinkBoardCategories",
                columns: new[] { "IsPublic", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_LinkBoardCategories_Name",
                table: "LinkBoardCategories",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LinkBoardCategories_SortOrder",
                table: "LinkBoardCategories",
                column: "SortOrder");

            migrationBuilder.CreateIndex(
                name: "IX_LinkBoardLinks_CategoryId",
                table: "LinkBoardLinks",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_LinkBoardLinks_DraftOfId",
                table: "LinkBoardLinks",
                column: "DraftOfId");

            migrationBuilder.CreateIndex(
                name: "IX_LinkBoardLinks_IsPublic_Status_CreatorId",
                table: "LinkBoardLinks",
                columns: new[] { "IsPublic", "Status", "CreatorId" });

            migrationBuilder.CreateIndex(
                name: "IX_LinkBoardLinks_SortOrder",
                table: "LinkBoardLinks",
                column: "SortOrder");

            migrationBuilder.CreateIndex(
                name: "IX_LinkBoardLinks_Url",
                table: "LinkBoardLinks",
                column: "Url");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LinkBoardLinks");

            migrationBuilder.DropTable(
                name: "LinkBoardCategories");
        }
    }
}
