using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace fitback.Migrations
{
    /// <inheritdoc />
    public partial class AddTrainerToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RefreshToken",
                table: "Trainers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RefreshTokenExpiry",
                table: "Trainers",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RefreshToken",
                table: "Trainers");

            migrationBuilder.DropColumn(
                name: "RefreshTokenExpiry",
                table: "Trainers");
        }
    }
}
