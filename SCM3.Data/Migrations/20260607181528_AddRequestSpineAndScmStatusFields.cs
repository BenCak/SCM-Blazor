using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SCM3.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRequestSpineAndScmStatusFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Comments",
                table: "RequestSCMStatuses",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReleaseReadinessNotes",
                table: "RequestSCMStatuses",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReviewerNotes",
                table: "RequestSCMStatuses",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NeedDate",
                table: "Requests",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NotesToScm",
                table: "Requests",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ParentVersion",
                table: "Requests",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReadyDate",
                table: "Requests",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReleaseDate",
                table: "Requests",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReleaseDescription",
                table: "Requests",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RequestorEmail",
                table: "Requests",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RequestorName",
                table: "Requests",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RequestorPhone",
                table: "Requests",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Comments",
                table: "RequestSCMStatuses");

            migrationBuilder.DropColumn(
                name: "ReleaseReadinessNotes",
                table: "RequestSCMStatuses");

            migrationBuilder.DropColumn(
                name: "ReviewerNotes",
                table: "RequestSCMStatuses");

            migrationBuilder.DropColumn(
                name: "NeedDate",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "NotesToScm",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "ParentVersion",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "ReadyDate",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "ReleaseDate",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "ReleaseDescription",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "RequestorEmail",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "RequestorName",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "RequestorPhone",
                table: "Requests");
        }
    }
}
