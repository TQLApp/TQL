using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.CsProj;
using BenchmarkDotNet.Toolchains.DotNetCli;

// From https://github.com/dotnet/BenchmarkDotNet/issues/1535.

var config = DefaultConfig
    .Instance
    .AddJob(
        Job.Default
            .WithToolchain(
                CsProjCoreToolchain.From(
                    new NetCoreAppSettings(
                        targetFrameworkMoniker: "net8.0-windows",
                        runtimeFrameworkVersion: null!,
                        name: "8.0"
                    )
                )
            )
            .AsDefault()
    );

BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, config);
