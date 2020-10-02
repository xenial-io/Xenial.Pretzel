using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;

using Fluid;

using Pretzel.Logic.Templating.Context;
using Pretzel.Logic.Templating.Jekyll.Extensions;

namespace Pretzel.Logic.Templating.Jekyll.Liquid
{
    public class SiteContextDrop
    {
        public SiteContext context { get; }

        public DateTime Time
        {
            get
            {
                return context.Time;
            }
        }

        public string Title
        {
            get { return context.Title; }
        }

        public SiteContextDrop(SiteContext context)
        {
            this.context = context;
        }

        public IDictionary<string, object> ToHash(TemplateContext templateContext)
        {
            var dict = context.Config.ToDictionary();
            templateContext.MemberAccessStrategy.Register(dict.GetType());
            templateContext.MemberAccessStrategy.Register(typeof(SiteContextDrop));
            templateContext.MemberAccessStrategy.Register(typeof(IEnumerable<Tag>));
            templateContext.MemberAccessStrategy.Register(typeof(Tag));
            templateContext.MemberAccessStrategy.Register(typeof(IEnumerable<Category>));
            templateContext.MemberAccessStrategy.Register(typeof(Category));
            templateContext.MemberAccessStrategy.Register(typeof(Data));
            templateContext.MemberAccessStrategy.Register(typeof(IList<Page>));
            templateContext.MemberAccessStrategy.Register(typeof(Page));
            templateContext.Model = dict;

            dict["posts"] = context.Posts.Select(p => p.ToHash(templateContext)).ToList();
            dict["pages"] = context.Pages.Select(p => p.ToHash(templateContext)).ToList();
            dict["html_pages"] = context.Html_Pages.Select(p => p.ToHash(templateContext)).ToList();
            dict["title"] = context.Title;
            dict["tags"] = context.Tags;
            dict["categories"] = context.Categories;
            dict["time"] = Time;
            dict["data"] = context.Data;

            return dict;
        }
    }
}
