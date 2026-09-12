using System.ComponentModel;
using System.Globalization;
using GF.Api.Contracts;

namespace GF.BasicExample.Converters
{
    internal sealed class Contract : TypeConverterBase<IContract>
    {
        protected override string ConvertToString(IContract contract, CultureInfo culture, ITypeDescriptorContext context)
        {
            return $"{contract} - {contract.Description}";
        }
    }
}
