using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Integration.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueIndexOnInvoiceNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_XeroInvoices_InvoiceNumber",
                table: "XeroInvoices",
                column: "InvoiceNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_XeroInvoices_InvoiceNumber",
                table: "XeroInvoices");
        }
    }
}
