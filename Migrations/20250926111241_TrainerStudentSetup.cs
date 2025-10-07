using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace fitback.Migrations
{
    /// <inheritdoc />
    public partial class TrainerStudentSetup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Students",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsApprovedByTrainer",
                table: "Students",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "TrainerId",
                table: "Students",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TrainerCodes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DurationInMonths = table.Column<int>(type: "int", nullable: false),
                    Quota = table.Column<int>(type: "int", nullable: false),
                    IsUsed = table.Column<bool>(type: "bit", nullable: false),
                    TrainerId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainerCodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrainerCodes_Trainers_TrainerId",
                        column: x => x.TrainerId,
                        principalTable: "Trainers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Students_TrainerId",
                table: "Students",
                column: "TrainerId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainerCodes_Code",
                table: "TrainerCodes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrainerCodes_TrainerId",
                table: "TrainerCodes",
                column: "TrainerId",
                unique: true,
                filter: "[TrainerId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Students_Trainers_TrainerId",
                table: "Students",
                column: "TrainerId",
                principalTable: "Trainers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Students_Trainers_TrainerId",
                table: "Students");

            migrationBuilder.DropTable(
                name: "TrainerCodes");

            migrationBuilder.DropIndex(
                name: "IX_Students_TrainerId",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "IsApprovedByTrainer",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "TrainerId",
                table: "Students");
        }
    }
}
