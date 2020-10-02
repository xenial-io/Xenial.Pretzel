using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;

using Pretzel.Logic.Extensibility;
using Pretzel.Logic.Extensions;

namespace Pretzel.Logic.Recipes
{
    public class Recipe
    {
        public Recipe(IFileSystem fileSystem, string engine, string directory, IEnumerable<IAdditionalIngredient> additionalIngredients, bool withDrafts = false)
        {
            this.fileSystem = fileSystem;
            this.engine = engine;
            this.directory = directory;
            this.additionalIngredients = additionalIngredients;
            this.withDrafts = withDrafts;
        }

        private readonly IFileSystem fileSystem;
        private readonly string engine;
        private readonly string directory;
        private readonly IEnumerable<IAdditionalIngredient> additionalIngredients;
        private readonly bool withDrafts;

        public void Create()
        {
            try
            {
                if (!fileSystem.Directory.Exists(directory))
                    fileSystem.Directory.CreateDirectory(directory);

                if (string.Equals("liquid", engine, StringComparison.InvariantCultureIgnoreCase))
                {
                    CreateDirectories();

                    fileSystem.File.WriteAllText(Path.Combine(directory, @"rss.xml"), Properties.Liquid.Rss);
                    fileSystem.File.WriteAllText(Path.Combine(directory, @"atom.xml"), Properties.Liquid.Atom);
                    fileSystem.File.WriteAllText(Path.Combine(directory, @"sitemap.xml"), Properties.Liquid.Sitemap);

                    fileSystem.File.WriteAllText(Path.Combine(directory, @"_layouts", "layout.html"), Properties.Liquid.Layout);
                    fileSystem.File.WriteAllText(Path.Combine(directory, @"_layouts", "post.html"), Properties.Liquid.Post);
                    fileSystem.File.WriteAllText(Path.Combine(directory, @"index.html"), Properties.Liquid.Index);
                    fileSystem.File.WriteAllText(Path.Combine(directory, @"about.md"), Properties.Liquid.About);
                    fileSystem.File.WriteAllText(Path.Combine(directory, @"_posts", string.Format("{0}-myfirstpost.md", DateTime.Today.ToString("yyyy-MM-dd"))), Properties.Liquid.FirstPost);
                    fileSystem.File.WriteAllText(Path.Combine(directory, @"css", "style.css"), Properties.Resources.Style);
                    fileSystem.File.WriteAllText(Path.Combine(directory, @"_config.yml"), Properties.Liquid.Config);
                    fileSystem.File.WriteAllText(Path.Combine(directory, @"_includes", "head.html"), Properties.Liquid.Head);

                    CreateImages();

                    Tracing.Info("Pretzel site template has been created");
                }
                else
                {
                    Tracing.Info("Templating Engine not found");
                    return;
                }

                foreach (var additionalIngredient in additionalIngredients)
                {
                    additionalIngredient.MixIn(directory);
                }
            }
            catch (Exception ex)
            {
                Tracing.Error("Error trying to create template: {0}", ex);
            }
        }

        private void CreateImages()
        {
            CreateImage(@"Resources\25.png", directory, @"img", "25.png");
            CreateImage(@"Resources\favicon.png", directory, @"img", "favicon.png");
            CreateImage(@"Resources\logo.png", directory, @"img", "logo.png");

            CreateFavicon();
        }

        private void CreateFavicon()
        {
            CreateImage(@"Resources\favicon.ico", directory, @"img", "favicon.ico");
        }

        private void CreateImage(string resourceName, params string[] pathSegments)
        {
            using (var ms = new MemoryStream())
            using (var resourceStream = GetResourceStream(resourceName))
            {
                resourceStream.CopyTo(ms);
                fileSystem.File.WriteAllBytes(Path.Combine(pathSegments), ms.ToArray());
            }
        }

        //https://github.com/dotnet/corefx/issues/12565
        private Stream GetResourceStream(string path)
        {
            var assembly = GetType().Assembly;
            var name = GetType().Assembly.GetName().Name;

            path = path.Replace("/", ".").Replace("\\", ".");

            var fullPath = $"{name}.{path}";
            var stream = assembly.GetManifestResourceStream(fullPath);

            return stream;
        }

        private void CreateDirectories()
        {
            fileSystem.Directory.CreateDirectory(Path.Combine(directory, @"_posts"));
            fileSystem.Directory.CreateDirectory(Path.Combine(directory, @"_layouts"));
            fileSystem.Directory.CreateDirectory(Path.Combine(directory, @"_includes"));
            fileSystem.Directory.CreateDirectory(Path.Combine(directory, @"css"));
            fileSystem.Directory.CreateDirectory(Path.Combine(directory, @"img"));
            if (withDrafts)
            {
                fileSystem.Directory.CreateDirectory(Path.Combine(directory, @"_drafts"));
            }
        }
    }
}
