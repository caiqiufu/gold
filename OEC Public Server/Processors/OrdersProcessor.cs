using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using GF.Api;
using GF.Api.Connection;
using GF.Api.Contracts;
using GF.Api.Orders;
using GF.Api.Orders.Drafts;
using GF.Api.Orders.Drafts.Validation;
using GF.Api.Values.Accounts;
using GF.Api.Values.Orders;
using GF.BasicExample.Converters;
using GF.BasicExample.Managers;

namespace GF.BasicExample.Processors
{
    public sealed class OrdersProcessor : DataProcessorBase<IOrder>, IDescriptionProvider
    {
        static OrdersProcessor()
        {
            TypeDescriptor.AddAttributes(typeof(IOrder), new TypeConverterAttribute(typeof(Order)));
        }

        public OrdersProcessor(IGFClient client)
            : base(client)
        {
        }

        public string GetItemDescription(object item)
        {
            return DescriptionManager.ToDescription<IOrder>(item, (order, sb) =>
            {
                foreach (var state in order.States)
                    sb.AppendLine($"{state.Timestamp:G}: {state}");

                if (!order.IsFinalState)
                {
                    sb.AppendLine();
                    sb.AppendLine(" * Double click to cancel order");
                }
            });
        }

        public IOrder SendOrder(OrderSide side, int quantity, IContract contract, OrderType orderType, double? price = null)
        {
            return SendOrder(new OrderDraftBuilder()
                .WithSide(side)
                .WithOrderType(orderType)
                .WithQuantity(quantity)
                .WithContractID(contract?.ID)
                .WithPrice(orderType == OrderType.Market ? new double?() : price.GetValueOrDefault(0))
                .WithAccountID(Client.Accounts.Get().FirstOrDefault(a => a.Type == AccountType.Customer)?.ID)
                .Build());
        }

        public IOrder SendOrder(OrderDraft draft)
        {
            var result = Client.Orders.Drafts.Validate(draft);

            if (result.Count > 0)
                throw new Exception($"Invalid values:{Environment.NewLine}{GetInvalidParts(result)}");

            return Client.Orders.SendOrder(draft);
        }

        public void CancelOrder(IOrder order)
        {
            Client.Orders.CancelOrder(order.ID, SubmissionType.Manual);
        }

        protected override void OnLoginComplete(IGFClient client, LoginCompleteEventArgs e)
        {
            base.OnLoginComplete(client, e);

            RefreshOrders();
            Client.Orders.OrderFilled += RefreshOrders;
            Client.Orders.CommandUpdated += RefreshOrders;
            Client.Orders.OrderConfirmed += RefreshOrders;
            Client.Orders.OrderStateChanged += RefreshOrders;
        }

        private void RefreshOrders(IGFClient client, EventArgs e)
        {
            RefreshOrders();
        }

        /// <summary>
        ///     Loads simple orders descriptions to the data source
        /// </summary>
        private void RefreshOrders()
        {
            LoadItems(Client.Orders.Get());
        }

        private static string GetInvalidParts(IReadOnlyList<OrderDraftValidationError> errors)
        {
            return string.Join(Environment.NewLine, errors.Select(e => $" * {e.Message}"));
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                Client.Orders.OrderFilled -= RefreshOrders;
                Client.Orders.CommandUpdated -= RefreshOrders;
                Client.Orders.OrderConfirmed -= RefreshOrders;
                Client.Orders.OrderStateChanged -= RefreshOrders;
            }
        }
    }
}
