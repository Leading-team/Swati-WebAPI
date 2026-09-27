using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SWWebAPI.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreateNew : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "UserAccounts",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "AQAAAAIAAYagAAAAEBM3DqUukzapjiv/DrUmn0+/NMFp26/M4clFj5pw9YVWugvvj687w9ww+X7r4JOCVQ==");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "UserAccounts",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "AQAAAAIAAYagAAAAEKdShEDLG8fbG3DVkHGqXMk9lYcZ6HLM8VHTB+wa57mpwWI0W1LhoMx5sXpUotu1ZA==");
        }
    }
}
