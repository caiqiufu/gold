using System.ComponentModel;
using System.Globalization;
using GF.Api.Balances;

namespace GF.BasicExample.Converters
{
    internal sealed class Balance : TypeConverterBase<IBalance>
    {
        protected override string ConvertToString(IBalance balance, CultureInfo culture, ITypeDescriptorContext context)
        {
            // NetLiquidatingValue contains cash balance + open trade equity value (settle PnL), so we need to substract it
            return $"'{balance.Account}' account balance: {balance.NetLiquidatingValue - balance.SettlePnL:C}";
        }
    }
}
