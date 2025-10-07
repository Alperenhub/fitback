using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace fitback.Migrations
{
    /// <inheritdoc />
    public partial class AddTrainerCodeToStudent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TrainerCodeId",
                table: "Students",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Students_TrainerCodeId",
                table: "Students",
                column: "TrainerCodeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Students_TrainerCodes_TrainerCodeId",
                table: "Students",
                column: "TrainerCodeId",
                principalTable: "TrainerCodes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Students_TrainerCodes_TrainerCodeId",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_Students_TrainerCodeId",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "TrainerCodeId",
                table: "Students");
        }
    }
}
