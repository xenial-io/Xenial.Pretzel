using System;

using static SimpleExec.Command;
using static Bullseye.Targets;

var sln = "./src/Pretzel.sln";
var tool = "./src/Pretzel/Pretzel.csproj";
var configuration = "--configuration Release";

Target("restore", () => RunAsync("dotnet", $"restore {sln}"));

Target("build", DependsOn("restore"), () => RunAsync("dotnet", $"build {sln} {configuration}"));
Target("pack", DependsOn("build"), () => RunAsync("dotnet", $"pack {tool} {configuration}"));

Target("deploy.nuget", async () =>
{
    var files = Directory.EnumerateFiles("artifacts/nuget", "*.nupkg");

    foreach (var file in files)
    {
        await RunAsync("dotnet", $"nuget push {file} --skip-duplicate -s https://api.nuget.org/v3/index.json -k {Environment.GetEnvironmentVariable("NUGET_AUTH_TOKEN")}",
            noEcho: true
        );
    }
});

Target("default", DependsOn("pack"));

await RunTargetsAndExitAsync(args);
