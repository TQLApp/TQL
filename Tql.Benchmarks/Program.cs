using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.CsProj;
using BenchmarkDotNet.Toolchains.DotNetCli;
using Tql.Benchmarks;

var config = DefaultConfig
    .Instance
    .AddJob(
        Job.Default
            .WithToolchain(
                CsProjCoreToolchain.From(
                    new NetCoreAppSettings(
                        targetFrameworkMoniker: "net8.0-windows", // the key to make it work
                        runtimeFrameworkVersion: null!,
                        name: "8.0"
                    )
                )
            )
            .AsDefault()
    );

BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, config);
