using System.ComponentModel;
using System.Globalization;
using GF.Api.Orders;

namespace GF.BasicExample.Converters
{
    internal sealed class Order : TypeConverterBase<IOrder>
    {
        protected override string ConvertToString(IOrder order, CultureInfo culture, ITypeDescriptorContext context)
        {
            return $"#{order.ID}: {order} - {order.CurrentState}, {order.Fills.TotalQuantity} fill(s)";
        }
    }
}
