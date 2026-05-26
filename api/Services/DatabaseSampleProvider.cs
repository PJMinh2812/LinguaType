using LinguaType.Api.Data;
using LinguaType.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace LinguaType.Api.Services;

public sealed class DatabaseSampleProvider(LinguaTypeDbContext db) : ISampleProvider
{
    public IReadOnlyList<TypingSample> GetAll() =>
        db.TypingSamples.AsNoTracking().OrderBy(sample => sample.Id).ToList();

    public TypingSample GetRandom()
    {
        var samples = db.TypingSamples.AsNoTracking().OrderBy(sample => sample.Id).ToList();
        if (samples.Count == 0)
        {
            throw new InvalidOperationException("No samples available.");
        }

        return samples[Random.Shared.Next(samples.Count)];
    }
}