using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace fitback.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TrainerCodes_TrainerId",
                table: "TrainerCodes");

            migrationBuilder.CreateIndex(
                name: "IX_TrainerCodes_TrainerId",
                table: "TrainerCodes",
                column: "TrainerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TrainerCodes_TrainerId",
                table: "TrainerCodes");

            migrationBuilder.CreateIndex(
                name: "IX_TrainerCodes_TrainerId",
                table: "TrainerCodes",
                column: "TrainerId",
                unique: true,
                filter: "[TrainerId] IS NOT NULL");
        }
    }
}
