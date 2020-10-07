using System.Collections.Generic;
using System.Linq;

using Fluid;

using Pretzel.Logic.Templating.Context;

namespace Pretzel.Logic.Templating.Jekyll.Extensions
{
    public static class PageExtensions
    {
        public static IDictionary<string, object> ToHash(this Page page, TemplateContext context)
        {
            var dict = new Dictionary<string, object>(page.Bag);
            context.MemberAccessStrategy.Register(typeof(Dictionary<string, object>));
            context.MemberAccessStrategy.Register(typeof(IDictionary<string, object>));

            dict.Remove("Date");
            dict.Remove("Title");
            dict.Remove("Url");
            dict.Add("Title", page.Title);
            dict.Add("Url", page.Url);
            dict.Add("Date", page.Date);
            dict.Add("Content", page.Content);

            return dict;
        }
    }
}
