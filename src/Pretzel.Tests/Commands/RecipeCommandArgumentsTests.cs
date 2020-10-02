using System;
using System.IO.Abstractions;
using Pretzel.Logic.Commands;

namespace Pretzel.Tests.Commands
{
    public class RecipeCommandArgumentsTests : PretzelBaseCommandArgumentsTests<RecipeCommandArguments>
    {
        protected override RecipeCommandArguments CreateArguments(IFileSystem fileSystem)
            => new RecipeCommandArguments(fileSystem);
    }
}
