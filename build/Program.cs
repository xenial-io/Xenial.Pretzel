using System;

using static SimpleExec.Command;
using static Bullseye.Targets;

var sln = "./src/Pretzel.sln";
var tool = "./src/Pretzel/Pretzel.csproj";

Target("restore", () => RunAsync("dotnet", $"restore {sln}"));

Target("build", DependsOn("restore"), () => RunAsync("dotnet", $"build {sln}"));
Target("pack", DependsOn("build"), () => RunAsync("dotnet", $"pack {tool}"));

Target("default", DependsOn("pack"));

await RunTargetsAndExitAsync(args);
