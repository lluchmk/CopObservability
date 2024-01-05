using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace OrdersAPI.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Orders",
                columns: new[] { "Id", "Amount", "CustomerId", "ProductId" },
                values: new object[,]
                {
                    { new Guid("0eb4b3a1-080f-430b-a42b-527e2c5607a6"), 13, new Guid("87c77822-d53d-4db1-8e66-468e28102456"), new Guid("e7e45871-5885-462f-b6e7-85dec42e037e") },
                    { new Guid("52dd8301-69d7-4040-b0bf-3695625a15e2"), 15, new Guid("87c77822-d53d-4db1-8e66-468e28102456"), new Guid("5ba58a44-ead2-4efa-96dd-b789101953e6") },
                    { new Guid("5497896b-485d-4951-a720-e52826460705"), 2, new Guid("c654b145-1a4a-43b4-a741-87b186554edc"), new Guid("9323c4f1-8a0b-4dda-9272-a96b4c59313f") },
                    { new Guid("60fd41e5-8b52-4923-b72e-ad9b98f38aed"), 19, new Guid("87c77822-d53d-4db1-8e66-468e28102456"), new Guid("e7e45871-5885-462f-b6e7-85dec42e037e") },
                    { new Guid("86356385-d6cd-48a7-b185-9187e10e9a2b"), 8, new Guid("3f617303-3844-4403-9017-4fb0bd0ac827"), new Guid("e7e45871-5885-462f-b6e7-85dec42e037e") },
                    { new Guid("a0140d4d-ca10-4225-8182-7a2defb890a1"), 13, new Guid("3f617303-3844-4403-9017-4fb0bd0ac827"), new Guid("e7e45871-5885-462f-b6e7-85dec42e037e") },
                    { new Guid("d543a9db-ced4-4d9c-b5ba-06f27f447087"), 64, new Guid("87c77822-d53d-4db1-8e66-468e28102456"), new Guid("e7e45871-5885-462f-b6e7-85dec42e037e") },
                    { new Guid("fde052ba-6d8c-4d6b-ba9a-121d5e9dde8f"), 98, new Guid("c654b145-1a4a-43b4-a741-87b186554edc"), new Guid("e7e45871-5885-462f-b6e7-85dec42e037e") }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Orders");
        }
    }
}
