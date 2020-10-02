using Fluid;

using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

using Pretzel.Logic.Extensions;
using Pretzel.Logic.Liquid;
using Pretzel.Logic.Templating.Context;
using Pretzel.Logic.Templating.Jekyll.Liquid;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Composition;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Pretzel.Logic.Templating.Jekyll
{
    [Shared]
    [SiteEngineInfo(Engine = "liquid")]
    public class LiquidEngine : JekyllEngineBase
    {
        private SiteContextDrop contextDrop;
        private static readonly Regex emHtmlRegex = new Regex(@"(?<=\{[\{\%].*?)(</?em>)(?=.*?[\%\}]\})", RegexOptions.Compiled);

        static LiquidEngine()
        {
            //DotLiquid.Liquid.UseRubyDateFormat = true;
        }

        protected override void PreProcess()
        {
            contextDrop = new SiteContextDrop(Context);
            
            //Template.FileSystem = new Includes(Context.SourceFolder, FileSystem);

            //if (Filters != null)
            //{
            //    foreach (var filter in Filters)
            //    {
            //        Template.RegisterFilter(filter.GetType());
            //    }
            //}
            //if (Tags != null)
            //{
            //    var registerTagMethod = typeof(Template).GetMethod("RegisterTag", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

            //    foreach (var tag in Tags)
            //    {
            //            var registerTagGenericMethod = registerTagMethod.MakeGenericMethod(new[] { tag.GetType() });
            //            registerTagGenericMethod.Invoke(null, new[] { tag.Name.ToUnderscoreCase() });
            //    }
            //}
            //if(TagFactories!=null)
            //{
            //    foreach (var tagFactory in TagFactories)
            //    {
            //        tagFactory.Initialize(Context);
            //        Template.RegisterTagFactory(tagFactory);
            //    }
            //}
        }

        private TemplateContext CreatePageData(PageContext pageContext)
        {
            var context = new TemplateContext();
            context.MemberAccessStrategy.Register(typeof(SiteContext));
            context.MemberAccessStrategy.Register(typeof(Page));
            context.MemberAccessStrategy.Register(typeof(PageData));
            context.MemberAccessStrategy.Register(typeof(Paginator));
            context.MemberAccessStrategy.Register(typeof(Dictionary<string, object>));
            context.MemberAccessStrategy.Register(typeof(IDictionary<string, object>));

            var y = new Dictionary<string, object>(pageContext.Bag);

            if (y.ContainsKey("title"))
            {
                if (string.IsNullOrWhiteSpace(y["title"].ToString()))
                {
                    y["title"] = Context.Title;
                }
            }
            else
            {
                y.Add("title", Context.Title);
            }

            y.Add("previous", pageContext.Previous);
            y.Add("next", pageContext.Next);
            y.Add("data", contextDrop.Data);

            var x = new PageData
            {
                site = contextDrop.ToHash(context),
                page = y,
                content = pageContext.FullContent,
                paginator = pageContext.Paginator,
            };

            context.Model = x;

            return context;
        }

        protected override string RenderTemplate(string content, PageContext pageData)
        {
            // Replace all em HTML tags in liquid tags ({{ or {%) by underscores
            content = emHtmlRegex.Replace(content, "_");

            var data = CreatePageData(pageData);
            var includes = Path.Combine(contextDrop.context.SourceFolder, "_includes");
            includes = Path.GetFullPath(includes);

            data.FileProvider = new PretzelPhysicalFileProvider(new FileSystemPhysicalFileProvider(FileSystem, includes));

            var template = FluidTemplate.Parse(content);
            var output = template.Render(data);

            return output;
        }

        public override void Initialize()
        {
            TemplateContext.GlobalFilters.AddFilter(nameof(PretzelFilters.xml_escape), PretzelFilters.xml_escape);
            TemplateContext.GlobalFilters.AddFilter(nameof(PretzelFilters.date_to_xmlschema), PretzelFilters.date_to_xmlschema);
        }
    }

    public class PageData
    {
        public IDictionary<string, object> site { get; set; }
        public IDictionary<string, object> page { get; set; }
        public string content { get; set; }
        public Paginator paginator { get; set; }
    }
    public class FileSystemPhysicalFileProvider : IFileProvider
    {
        public FileSystemPhysicalFileProvider(System.IO.Abstractions.IFileSystem fileSystem, string root)
        {
            FileSystem = fileSystem;
            Root = root;
        }

        public System.IO.Abstractions.IFileSystem FileSystem { get; }
        public string Root { get; }

        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            var dir = FileSystem.DirectoryInfo.FromDirectoryName(Path.Combine(Root, subpath));
            return new DirectoryContents(dir);
        }

        class DirectoryContents : IDirectoryContents
        {
            public DirectoryContents(System.IO.Abstractions.IDirectoryInfo dir)
            {
                Dir = dir;
            }

            public System.IO.Abstractions.IDirectoryInfo Dir { get; }
            public bool Exists => Dir.Exists;

            public IEnumerator<IFileInfo> GetEnumerator()
            {
                return Dir.EnumerateFiles().Select(f => new FileInfo(f)).GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                throw new NotImplementedException();
            }
        }

        class FileInfo : IFileInfo
        {
            public System.IO.Abstractions.IFileInfo File { get; }
            public FileInfo(System.IO.Abstractions.IFileInfo file)
            {
                File = file;
            }

            public bool Exists => File.Exists;
            public long Length => File.Length;
            public string PhysicalPath => File.ToString();
            public string Name => File.Name;
            public DateTimeOffset LastModified => DateTime.SpecifyKind(File.LastWriteTime, DateTimeKind.Utc);
            public bool IsDirectory => false;
            
            public Stream CreateReadStream()
            {
                return File.OpenRead();
            }
        }

        public IFileInfo GetFileInfo(string subpath)
        {
            return new FileInfo(this.FileSystem.FileInfo.FromFileName(Path.Combine(Root, subpath)));
        }

        public IChangeToken Watch(string filter)
        {
            throw new NotImplementedException();
        }
    }

    public class PretzelPhysicalFileProvider : IFileProvider, IDisposable
    {
        private bool disposedValue;
        readonly IFileProvider provider;

        public PretzelPhysicalFileProvider(IFileProvider provider)
            => this.provider = provider ?? throw new ArgumentNullException(nameof(provider));

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing && provider is IDisposable disposable)
                {
                    disposable.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public IFileInfo GetFileInfo(string subpath)
        {
            return provider.GetFileInfo(Path.GetFileNameWithoutExtension(subpath));
        }

        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            return provider.GetDirectoryContents(Path.GetFileNameWithoutExtension(subpath));
        }

        public IChangeToken Watch(string filter)
        {
            return provider.Watch(filter);
        }
    }
}
