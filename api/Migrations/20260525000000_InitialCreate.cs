using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinguaType.Api.Migrations;

public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "typing_samples",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false),
                Text = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_typing_samples", x => x.Id);
            });

        migrationBuilder.InsertData(
            table: "typing_samples",
            columns: new[] { "Id", "Text" },
            values: new object[,]
            {
                { 1, "今天天气很好，适合出去散步。" },
                { 2, "学习中文需要坚持每天练习。" },
                { 3, "我喜欢吃饺子，尤其是冬天。" },
            });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "typing_samples");
    }
}