using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _3_Payment.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCompraCatalogoId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IdCompraCatalogo",
                table: "Compras",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Compras_IdCompraCatalogo",
                table: "Compras",
                column: "IdCompraCatalogo",
                unique: true,
                filter: "[IdCompraCatalogo] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Compras_IdCompraCatalogo",
                table: "Compras");

            migrationBuilder.DropColumn(
                name: "IdCompraCatalogo",
                table: "Compras");
        }
    }
}
