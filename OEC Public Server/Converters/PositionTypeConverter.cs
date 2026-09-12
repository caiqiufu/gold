using System.ComponentModel;
using System.Globalization;
using GF.Api.Positions;

namespace GF.BasicExample.Converters
{
    public sealed class Position : TypeConverterBase<IPosition>
    {
        protected override string ConvertToString(IPosition position, CultureInfo culture, ITypeDescriptorContext context)
        {
            return $"{position.Contract}: " +
                   $"({position.Short.Volume:+#;-#;0}/{-1 * position.Long.Volume:+#;-#;0}) " +
                   $"{position.Net.Volume} @ {position.Contract.PriceToString(position.Net.Price)} - " +
                   $"P/L {position.OTE:C}";
        }
    }
}
