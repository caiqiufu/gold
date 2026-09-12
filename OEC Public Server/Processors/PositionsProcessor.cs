using System;
using System.ComponentModel;
using System.Linq;
using GF.Api;
using GF.Api.Balances;
using GF.Api.Connection;
using GF.Api.Orders;
using GF.Api.Positions;
using GF.Api.Values.Orders;
using GF.BasicExample.Converters;
using GF.BasicExample.Managers;

namespace GF.BasicExample.Processors
{
    public sealed class PositionsProcessor : DataProcessorBase<object>, IDescriptionProvider
    {
        static PositionsProcessor()
        {
            TypeDescriptor.AddAttributes(typeof(IBalance), new TypeConverterAttribute(typeof(Balance)));
            TypeDescriptor.AddAttributes(typeof(IPosition), new TypeConverterAttribute(typeof(Position)));
        }

        public PositionsProcessor(IGFClient client)
            : base(client)
        {
        }

        public string GetItemDescription(object item)
        {
            return DescriptionManager.ToDescription<IPosition>(item, (position, sb) =>
            {
                if (CanExitPosition(position))
                {
                    sb.AppendLine();
                    sb.AppendLine(" * Double click to exit position");
                }
            });
        }

        public bool CanExitPosition(IPosition position)
        {
            return position != null && GetPositionVolume(position) != 0;
        }

        public IOrder ExitPosition(IPosition position, OrdersProcessor orderProcessor)
        {
            var volume = GetPositionVolume(position);

            return orderProcessor.SendOrder(
                volume > 0 ? OrderSide.Buy : OrderSide.Sell,
                Math.Abs(volume),
                position.Contract,
                OrderType.Market);
        }

        protected override void OnLoginComplete(IGFClient client, LoginCompleteEventArgs e)
        {
            base.OnLoginComplete(client, e);

            RefreshPositions();
            client.Accounts.BalanceChanged += RefreshPositions;
            client.Accounts.AvgPositionChanged += RefreshPositions;
            client.Accounts.AccountSummaryChanged += RefreshPositions;
        }

        private void RefreshPositions(IGFClient client, EventArgs e)
        {
            RefreshPositions();
        }

        private void RefreshPositions()
        {
            // Most traders have only one account
            var account = Client.Accounts.Get().FirstOrDefault();

            // Position OTE property contains theoretical profit/loss value in USD.
            // TotalBalance property is a sum of all balances for each currency converted to USD.
            LoadItems(Enumerable.Concat(account?.AvgPositions.Values.Cast<object>(), new[] { account.TotalBalance })
                     ?? new[] { "No accounts found" });
        }

        private int GetPositionVolume(IPosition position)
        {
//            return position.Net.Volume;
            return position.Short.Volume - position.Long.Volume;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                Client.Accounts.BalanceChanged -= RefreshPositions;
                Client.Accounts.AvgPositionChanged -= RefreshPositions;
                Client.Accounts.AccountSummaryChanged -= RefreshPositions;
            }
        }
    }
}
