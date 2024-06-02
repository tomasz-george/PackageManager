using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.Logging.Abstractions;
using PackageManager.Application.Interfaces;
using PackageManager.Application.Services;

[MemoryDiagnoser]
[ShortRunJob]
public class PackageInstallerServiceBenchmark
{
    private IPackageValidationService _packageManagerService;
    private string[]? _smallInput;
    private string[]? _largeInput;

    [GlobalSetup]
    public void Setup()
    {
        var nullLogger = NullLogger<PackageValidationService>.Instance;
        _packageManagerService = new PackageValidationService(nullLogger);

        _smallInput =
        [
            "3",
            "A,1",
            "B,1",
            "C,1",
            "2",
            "A,1,B,1",
            "B,1,C,1"
        ];

        _largeInput = GenerateLargeTestInput(1000);
    }

    [Benchmark]
    public bool BenchmarkSmallInput()
    {
        return _packageManagerService.IsInstallationValid(_smallInput);
    }

    [Benchmark]
    public bool BenchmarkLargeInput()
    {
        return _packageManagerService.IsInstallationValid(_largeInput);
    }

    private string[] GenerateLargeTestInput(int numberOfPackages)
    {
        var input = new List<string>
        {
            numberOfPackages.ToString()
        };

        for (int i = 1; i <= numberOfPackages; i++)
        {
            input.Add($"Package{i},1");
        }

        input.Add((numberOfPackages - 1).ToString());

        for (int i = 1; i < numberOfPackages; i++)
        {
            input.Add($"Package{i},1,Package{i + 1},1");
        }

        return input.ToArray();
    }
}
