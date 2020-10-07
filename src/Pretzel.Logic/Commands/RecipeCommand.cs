using System;
using System.Collections.Generic;
using System.Composition;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using Pretzel.Logic.Extensibility;
using Pretzel.Logic.Extensions;
using Pretzel.Logic.Recipes;

namespace Pretzel.Logic.Commands
{
    [Shared]
    [Export]
    [CommandArguments]
    public class RecipeCommandArguments : PretzelBaseCommandArguments
    {
        [ImportingConstructor]
        public RecipeCommandArguments(IFileSystem fileSystem) : base(fileSystem) { }
    }

    [Shared]
    [CommandInfo(
        Name = "create",
        Description = "configure a new site",
        ArgumentsType = typeof(RecipeCommandArguments),
        CommandType = typeof(RecipeCommand)
        )]
    public sealed class RecipeCommand : Command<RecipeCommandArguments>
    {
        private static readonly List<string> TemplateEngines = new List<string>(new[] { "Liquid" });

        [Import]
        public IFileSystem FileSystem { get; set; }

        [ImportMany]
        public IEnumerable<IAdditionalIngredient> AdditionalIngredients { get; set; }

        protected override Task<int> Execute(RecipeCommandArguments arguments)
        {
            Tracing.Info("create - configure a new site");

            var engine = string.IsNullOrWhiteSpace(arguments.Template)
                             ? TemplateEngines.First()
                             : arguments.Template;

            if (!TemplateEngines.Any(e => string.Equals(e, engine, StringComparison.InvariantCultureIgnoreCase)))
            {
                Tracing.Info("Requested templating engine not found: {0}", engine);

                return Task.FromResult(1);
            }

            Tracing.Info("Using {0} Engine", engine);

            var recipe = new Recipe(FileSystem, engine, arguments.Source, AdditionalIngredients, arguments.Drafts);
            recipe.Create();

            return Task.FromResult(0);
        }
    }
}
