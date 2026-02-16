namespace WebApplication.Models;

public sealed class MutabilityDemo
{
    public MutabilityDemoResult BuildExamples()
    {
        var mutableCar = new MutableCar("Roadster", 2024);
        var mutableBefore = mutableCar.ToString();
        mutableCar.ModelName = "Roadster GT";
        var mutableAfter = mutableCar.ToString();

        var immutableCar = new ImmutableCar("Coupe", 2024);
        var immutableBefore = immutableCar.ToString();
        var immutableAfter = immutableCar with { ModelName = "Coupe Touring" };

        return new MutabilityDemoResult(
            mutableBefore,
            mutableAfter,
            immutableBefore,
            immutableAfter.ToString());
    }
}

public sealed class MutableCar(string modelName, int year)
{
    public string ModelName { get; set; } = modelName;

    public int Year { get; set; } = year;

    public override string ToString() => $"{Year} {ModelName}";
}

public sealed record ImmutableCar(string ModelName, int Year)
{
    public override string ToString() => $"{Year} {ModelName}";
}

public sealed record MutabilityDemoResult(
    string MutableBefore,
    string MutableAfter,
    string ImmutableBefore,
    string ImmutableAfter);
