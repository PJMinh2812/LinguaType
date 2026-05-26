using LinguaType.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

#nullable disable

namespace LinguaType.Api.Migrations;

[DbContext(typeof(LinguaTypeDbContext))]
public partial class LinguaTypeDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
#pragma warning disable 612, 618
        modelBuilder
            .HasAnnotation("ProductVersion", "10.0.0");

        modelBuilder.Entity("LinguaType.Api.Models.TypingSample", b =>
        {
            b.Property<int>("Id")
                .HasColumnType("integer");

            b.Property<string>("Text")
                .IsRequired()
                .HasMaxLength(1000)
                .HasColumnType("character varying(1000)");

            b.HasKey("Id");

            b.ToTable("typing_samples");

            b.HasData(
                new
                {
                    Id = 1,
                    Text = "今天天气很好，适合出去散步。"
                },
                new
                {
                    Id = 2,
                    Text = "学习中文需要坚持每天练习。"
                },
                new
                {
                    Id = 3,
                    Text = "我喜欢吃饺子，尤其是冬天。"
                });
        });
#pragma warning restore 612, 618
    }
}