using LinguaType.Api.Models;

namespace LinguaType.Api.Services;

public sealed class InMemorySampleProvider : ISampleProvider
{
    private static readonly IReadOnlyList<TypingSample> Samples =
    [
        new TypingSample { Id = 1, Text = "今天天气很好，适合出去散步。" },
        new TypingSample { Id = 2, Text = "学习中文需要坚持每天练习。" },
        new TypingSample { Id = 3, Text = "我喜欢吃饺子，尤其是冬天。" },
    ];

    private static readonly Random Random = new();

    public IReadOnlyList<TypingSample> GetAll() => Samples;

    public TypingSample GetRandom() => Samples[Random.Next(Samples.Count)];
}
