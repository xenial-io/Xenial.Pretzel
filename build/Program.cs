using System;

using static SimpleExec.Command;
using static Bullseye.Targets;

Target("build", () =>
{
    Console.WriteLine("Hello World!");
});

Target("default", DependsOn("build"));

await RunTargetsAndExitAsync(args);
