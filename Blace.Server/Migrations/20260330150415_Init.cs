using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blace.Server.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "deletes",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    DateTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_deletes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "places",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CreatedTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastChangeTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Canvas = table.Column<byte[]>(type: "bytea", nullable: true),
                    Height = table.Column<int>(type: "integer", nullable: false),
                    Width = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_places", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tiles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    CreatedTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PlaceId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    X = table.Column<int>(type: "integer", nullable: false),
                    Y = table.Column<int>(type: "integer", nullable: false),
                    Color = table.Column<byte>(type: "smallint", nullable: false),
                    PreviousColor = table.Column<byte>(type: "smallint", nullable: false),
                    DeleteId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tiles", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tiles_DeleteId",
                table: "tiles",
                column: "DeleteId");

            migrationBuilder.CreateIndex(
                name: "IX_tiles_PlaceId",
                table: "tiles",
                column: "PlaceId");

            migrationBuilder.CreateIndex(
                name: "IX_tiles_PlaceId_UserId",
                table: "tiles",
                columns: new[] { "PlaceId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_tiles_PlaceId_X_Y",
                table: "tiles",
                columns: new[] { "PlaceId", "X", "Y" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "deletes");

            migrationBuilder.DropTable(
                name: "places");

            migrationBuilder.DropTable(
                name: "tiles");
        }
    }
}
