using LinguaType.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace LinguaType.Api.Data;

public sealed class LinguaTypeDbContext(DbContextOptions<LinguaTypeDbContext> options) : DbContext(options)
{
    public DbSet<TypingSample> TypingSamples => Set<TypingSample>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TypingSample>(entity =>
        {
            entity.ToTable("typing_samples");
            entity.HasKey(sample => sample.Id);
            entity.Property(sample => sample.Id).ValueGeneratedNever();
            entity.Property(sample => sample.Text).IsRequired().HasMaxLength(1000);
            entity.HasData(
                new TypingSample { Id = 1, Text = "今天天气很好，适合出去散步。" },
                new TypingSample { Id = 2, Text = "学习中文需要坚持每天练习。" },
                new TypingSample { Id = 3, Text = "我喜欢吃饺子，尤其是冬天。" }
            );
        });
    }
}