using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SWWebAPI.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "employees",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    first_name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    last_name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    username = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    pswd = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    mobilenumber = table.Column<int>(type: "int", nullable: true),
                    email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    created_by = table.Column<int>(type: "int", nullable: false),
                    created_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    modified_by = table.Column<int>(type: "int", nullable: true),
                    modified_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_employees", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "menu_list",
                columns: table => new
                {
                    menu_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    menu_name = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    language = table.Column<string>(type: "char(2)", maxLength: 2, nullable: false),
                    created_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_menu_list", x => x.menu_id);
                });

            migrationBuilder.CreateTable(
                name: "subscribers",
                columns: table => new
                {
                    subscriber_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    subscriber_name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    plan_id = table.Column<int>(type: "int", nullable: false),
                    issuedate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    expirydate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    address1 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    address2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    address3 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    state = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    pincode = table.Column<int>(type: "int", nullable: false),
                    mobilenumber = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_subscribers", x => x.subscriber_id);
                });

            migrationBuilder.CreateTable(
                name: "subscription_plan",
                columns: table => new
                {
                    plan_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    plan_name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    created_by = table.Column<int>(type: "int", nullable: false),
                    created_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    modified_by = table.Column<int>(type: "int", nullable: true),
                    modified_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_subscription_plan", x => x.plan_id);
                });

            migrationBuilder.CreateTable(
                name: "UserAccounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAccounts", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "UserAccounts",
                columns: new[] { "Id", "FullName", "Password", "UserName" },
                values: new object[] { 1, "Administrator", "AQAAAAIAAYagAAAAEKdShEDLG8fbG3DVkHGqXMk9lYcZ6HLM8VHTB+wa57mpwWI0W1LhoMx5sXpUotu1ZA==", "admin" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "employees");

            migrationBuilder.DropTable(
                name: "menu_list");

            migrationBuilder.DropTable(
                name: "subscribers");

            migrationBuilder.DropTable(
                name: "subscription_plan");

            migrationBuilder.DropTable(
                name: "UserAccounts");
        }
    }
}
