using LinguaType.Api.Models;

namespace LinguaType.Api.Services;

public interface ISampleProvider
{
    IReadOnlyList<TypingSample> GetAll();
    TypingSample GetRandom();
}
