using Fluid;

using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

using Pretzel.Logic.Extensions;
using Pretzel.Logic.Liquid;
using Pretzel.Logic.Templating.Context;
using Pretzel.Logic.Templating.Jekyll.Liquid;
using System;
using System.Collections.Generic;
using System.Composition;
using System.IO;
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

            data.FileProvider = new PretzelPhysicalFileProvider(new PhysicalFileProvider(includes));

            var template = FluidTemplate.Parse(content);
            var output = template.Render(data);

            return output;
        }

        public override void Initialize()
        {
            TemplateContext.GlobalFilters.AddFilter(nameof(XmlEscapeFilter.xml_escape), XmlEscapeFilter.xml_escape);
            //TODO: custom filters
            //TemplateContext.GlobalFilters.AddFilter(nameof(XmlEscapeFilter.date_to_xmlschema), XmlEscapeFilter.date_to_xmlschema);
            //Template.RegisterFilter(typeof(DateToStringFilter));
            //Template.RegisterFilter(typeof(DateToLongStringFilter));
            //Template.RegisterFilter(typeof(DateToRfc822FormatFilter));
            //Template.RegisterFilter(typeof(CgiEscapeFilter));
            //Template.RegisterFilter(typeof(UriEscapeFilter));
            //Template.RegisterFilter(typeof(NumberOfWordsFilter));
            //Template.RegisterTag<HighlightBlock>("highlight");
        }
    }

    public class PageData
    {
        public IDictionary<string, object> site { get; set; }
        public IDictionary<string, object> page { get; set; }
        public string content { get; set; }
        public Paginator paginator { get; set; }
    }

    public class PretzelPhysicalFileProvider : IFileProvider, IDisposable
    {
        private bool disposedValue;
        readonly PhysicalFileProvider provider;

        public PretzelPhysicalFileProvider(PhysicalFileProvider provider)
            => this.provider = provider ?? throw new ArgumentNullException(nameof(provider));

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    this.provider.Dispose();
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
            return ((IFileProvider)provider).GetFileInfo(Path.GetFileNameWithoutExtension(subpath));
        }

        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            return ((IFileProvider)provider).GetDirectoryContents(Path.GetFileNameWithoutExtension(subpath));
        }

        public IChangeToken Watch(string filter)
        {
            return ((IFileProvider)provider).Watch(filter);
        }
    }
}
