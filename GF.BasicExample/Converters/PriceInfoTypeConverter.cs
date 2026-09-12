using System;
using System.ComponentModel;
using System.Globalization;
using GF.Api.Contracts;
using GF.BasicExample.Processors;

namespace GF.BasicExample.Converters
{
    internal sealed class PriceInfo : TypeConverterBase<PriceProcessor.PriceInfo>
    {
        protected override string ConvertToString(PriceProcessor.PriceInfo priceInfo, CultureInfo culture, ITypeDescriptorContext context)
        {
            var contract = priceInfo.Contract;

            if (contract == null)
                return "Market price: <unknown contract>";

            return $"{contract}: " +
                   $"Last: {GetPrice(contract, p => p.LastPrice)}, " +
                   $"Bid: {GetPrice(contract, p => p.BidPrice)}, " +
                   $"Ask: {GetPrice(contract, p => p.AskPrice)}";
        }

        private static string GetPrice(IContract contract, Func<IPrice, double> value)
        {
            // Contract.PriceToString method formats price according to contract specifications
            var currentPrice = contract?.CurrentPrice;
            return currentPrice != null ? contract.PriceToString(value(currentPrice)) : "<no price>";
        }
    }
}
