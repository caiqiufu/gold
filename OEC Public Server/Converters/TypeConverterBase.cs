using System;
using System.ComponentModel;
using System.Globalization;

namespace GF.BasicExample.Converters
{
    public abstract class TypeConverterBase : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return false;
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof(string) || base.CanConvertTo(context, destinationType);
        }

        public static string ToString(object value)
        {
            if (value == null)
                return null;

            if (value is string stringValue)
                return stringValue;

            var type = value.GetType();
            var converter = TypeDescriptor.GetConverter(type);

            return converter.CanConvertTo(typeof(string))
                ? converter.ConvertToString(null, CultureInfo.CurrentCulture, value)
                : value.ToString();
        }
    }

    public abstract class TypeConverterBase<T> : TypeConverterBase
    {
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            return value is T typedValue && destinationType == typeof(string)
                ? ConvertToString(typedValue, culture, context)
                : base.ConvertTo(context, culture, value, destinationType);
        }

        protected abstract string ConvertToString(T value, CultureInfo culture, ITypeDescriptorContext context);
    }
}
