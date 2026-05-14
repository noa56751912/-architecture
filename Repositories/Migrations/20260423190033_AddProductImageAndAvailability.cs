using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddProductImageAndAvailability : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Category_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Category_Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Categori__6DB38D6E84024264", x => x.Category_Id);
                });

            migrationBuilder.CreateTable(
                name: "Rating",
                columns: table => new
                {
                    Rating_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Host = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Method = table.Column<string>(type: "nchar(10)", fixedLength: true, maxLength: 10, nullable: true),
                    Path = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Referer = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    User_Agent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Record_Date = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RATING", x => x.Rating_Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    User_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    First_Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Last_Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Password = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Is_Admin = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Users__206D91703D18E153", x => x.User_Id);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Product_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Product_Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Price = table.Column<decimal>(type: "money", nullable: false),
                    Category_Id = table.Column<int>(type: "int", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Image_URL = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Is_Available = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Products__9834FBBAB524BA17", x => x.Product_Id);
                    table.ForeignKey(
                        name: "FK__Products__Catego__3C69FB99",
                        column: x => x.Category_Id,
                        principalTable: "Categories",
                        principalColumn: "Category_Id");
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Order_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Order_Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Order_Sum = table.Column<int>(type: "int", nullable: false),
                    User_Id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Orders__F1E4607B7D8F7859", x => x.Order_Id);
                    table.ForeignKey(
                        name: "FK__Orders__User_Id__3F466844",
                        column: x => x.User_Id,
                        principalTable: "Users",
                        principalColumn: "User_Id");
                });

            migrationBuilder.CreateTable(
                name: "Order_Item",
                columns: table => new
                {
                    Order_Item_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Product_Id = table.Column<int>(type: "int", nullable: false),
                    Order_Id = table.Column<int>(type: "int", nullable: false),
                    quantity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Order_It__4845682171D62BB1", x => x.Order_Item_Id);
                    table.ForeignKey(
                        name: "FK__Order_Ite__Order__4316F928",
                        column: x => x.Order_Id,
                        principalTable: "Orders",
                        principalColumn: "Order_Id");
                    table.ForeignKey(
                        name: "FK__Order_Ite__Produ__4222D4EF",
                        column: x => x.Product_Id,
                        principalTable: "Products",
                        principalColumn: "Product_Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Order_Item_Order_Id",
                table: "Order_Item",
                column: "Order_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Order_Item_Product_Id",
                table: "Order_Item",
                column: "Product_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_User_Id",
                table: "Orders",
                column: "User_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Category_Id",
                table: "Products",
                column: "Category_Id");

            migrationBuilder.CreateIndex(
                name: "UQ__Products__723D869496C0104C",
                table: "Products",
                column: "Product_Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Order_Item");

            migrationBuilder.DropTable(
                name: "Rating");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Categories");
        }
    }
}
