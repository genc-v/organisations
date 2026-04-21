using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace cmsOrg.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Organisations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Name = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organisations", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "OrganisationApiKeys",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    OrganisationId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Key = table.Column<string>(type: "varchar(512)", maxLength: 512, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganisationApiKeys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrganisationApiKeys_Organisations_OrganisationId",
                        column: x => x.OrganisationId,
                        principalTable: "Organisations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "OrganisationAssets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    OrganisationId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Name = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Type = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganisationAssets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrganisationAssets_Organisations_OrganisationId",
                        column: x => x.OrganisationId,
                        principalTable: "Organisations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "OrganisationRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    OrganisationId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Name = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganisationRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrganisationRoles_Organisations_OrganisationId",
                        column: x => x.OrganisationId,
                        principalTable: "Organisations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UserOrganisationRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    UserId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    OrganisationId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    RoleId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserOrganisationRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserOrganisationRoles_OrganisationRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "OrganisationRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserOrganisationRoles_Organisations_OrganisationId",
                        column: x => x.OrganisationId,
                        principalTable: "Organisations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationApiKeys_Key",
                table: "OrganisationApiKeys",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationApiKeys_OrganisationId",
                table: "OrganisationApiKeys",
                column: "OrganisationId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationAssets_OrganisationId",
                table: "OrganisationAssets",
                column: "OrganisationId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationRoles_OrganisationId",
                table: "OrganisationRoles",
                column: "OrganisationId");

            migrationBuilder.CreateIndex(
                name: "IX_UserOrganisationRoles_OrganisationId",
                table: "UserOrganisationRoles",
                column: "OrganisationId");

            migrationBuilder.CreateIndex(
                name: "IX_UserOrganisationRoles_RoleId",
                table: "UserOrganisationRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserOrganisationRoles_UserId_OrganisationId_RoleId",
                table: "UserOrganisationRoles",
                columns: new[] { "UserId", "OrganisationId", "RoleId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrganisationApiKeys");

            migrationBuilder.DropTable(
                name: "OrganisationAssets");

            migrationBuilder.DropTable(
                name: "UserOrganisationRoles");

            migrationBuilder.DropTable(
                name: "OrganisationRoles");

            migrationBuilder.DropTable(
                name: "Organisations");
        }
    }
}
