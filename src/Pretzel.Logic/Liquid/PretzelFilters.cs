using System;
using System.Security;
using System.Xml;

using Fluid;
using Fluid.Values;

namespace Pretzel.Logic.Liquid
{
    public static class PretzelFilters

    {
        public static FluidValue xml_escape(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            _ = arguments ?? throw new ArgumentNullException(nameof(arguments));
            _ = context ?? throw new ArgumentNullException(nameof(context));
            return new StringValue(SecurityElement.Escape(input.ToStringValue()));
        }

        public static FluidValue date_to_xmlschema(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            _ = arguments ?? throw new ArgumentNullException(nameof(arguments));
            _ = context ?? throw new ArgumentNullException(nameof(context));
            if(input is DateTimeValue dateTimeValue)
            {
                var dateTimeOffset = (DateTimeOffset)dateTimeValue.ToObjectValue();
                var value = XmlConvert.ToString(dateTimeOffset.DateTime, XmlDateTimeSerializationMode.Local);
                return new StringValue(value);
            }
            return input;
        }

        public static FluidValue markdown(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            _ = arguments ?? throw new ArgumentNullException(nameof(arguments));
            _ = context ?? throw new ArgumentNullException(nameof(context));
            return new StringValue(CommonMark.CommonMarkConverter.Convert(input.ToStringValue()));
        }
    }
}
