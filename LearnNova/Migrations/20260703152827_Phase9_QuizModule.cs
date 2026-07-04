using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearnNova.Migrations
{
    /// <inheritdoc />
    public partial class Phase9_QuizModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DueDate",
                table: "Quizzes");

            migrationBuilder.RenameColumn(
                name: "DurationMin",
                table: "Quizzes",
                newName: "TimeLimitMinutes");

            migrationBuilder.RenameColumn(
                name: "OptionsJson",
                table: "QuizQuestions",
                newName: "OptionB");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Quizzes",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Quizzes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "MaxAttempts",
                table: "Quizzes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PassingScore",
                table: "Quizzes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "ShowAnswers",
                table: "Quizzes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "OptionA",
                table: "QuizQuestions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OptionC",
                table: "QuizQuestions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OptionD",
                table: "QuizQuestions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OrderIndex",
                table: "QuizQuestions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AttemptNumber",
                table: "QuizAttempts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DurationSeconds",
                table: "QuizAttempts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "Passed",
                table: "QuizAttempts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<double>(
                name: "Percentage",
                table: "QuizAttempts",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartedAt",
                table: "QuizAttempts",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Quizzes");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Quizzes");

            migrationBuilder.DropColumn(
                name: "MaxAttempts",
                table: "Quizzes");

            migrationBuilder.DropColumn(
                name: "PassingScore",
                table: "Quizzes");

            migrationBuilder.DropColumn(
                name: "ShowAnswers",
                table: "Quizzes");

            migrationBuilder.DropColumn(
                name: "OptionA",
                table: "QuizQuestions");

            migrationBuilder.DropColumn(
                name: "OptionC",
                table: "QuizQuestions");

            migrationBuilder.DropColumn(
                name: "OptionD",
                table: "QuizQuestions");

            migrationBuilder.DropColumn(
                name: "OrderIndex",
                table: "QuizQuestions");

            migrationBuilder.DropColumn(
                name: "AttemptNumber",
                table: "QuizAttempts");

            migrationBuilder.DropColumn(
                name: "DurationSeconds",
                table: "QuizAttempts");

            migrationBuilder.DropColumn(
                name: "Passed",
                table: "QuizAttempts");

            migrationBuilder.DropColumn(
                name: "Percentage",
                table: "QuizAttempts");

            migrationBuilder.DropColumn(
                name: "StartedAt",
                table: "QuizAttempts");

            migrationBuilder.RenameColumn(
                name: "TimeLimitMinutes",
                table: "Quizzes",
                newName: "DurationMin");

            migrationBuilder.RenameColumn(
                name: "OptionB",
                table: "QuizQuestions",
                newName: "OptionsJson");

            migrationBuilder.AddColumn<DateTime>(
                name: "DueDate",
                table: "Quizzes",
                type: "datetime2",
                nullable: true);
        }
    }
}
