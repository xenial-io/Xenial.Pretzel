using System;
using System.Security;
using System.Xml;

using Fluid;
using Fluid.Values;

namespace Pretzel.Logic.Liquid
{
    public static class XmlEscapeFilter
    {
        public static FluidValue xml_escape(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            _ = arguments ?? throw new ArgumentNullException(nameof(arguments));
            _ = context ?? throw new ArgumentNullException(nameof(context));
            return new StringValue(SecurityElement.Escape(input.ToStringValue()));
        }

        //TODO: date_to_xmlschema filter
        //public static FluidValue date_to_xmlschema(FluidValue input, FilterArguments arguments, TemplateContext context)
        //{
        //    _ = arguments ?? throw new ArgumentNullException(nameof(arguments));
        //    _ = context ?? throw new ArgumentNullException(nameof(context));
        //    return XmlConvert.ToString(input, XmlDateTimeSerializationMode.Local);
        //}
    }
}
