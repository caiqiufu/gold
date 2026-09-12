using System.ComponentModel;
using GF.Api;
using GF.Api.Connection;
using GF.Api.Contracts;
using GF.Api.Subscriptions.Price;

namespace GF.BasicExample.Processors
{
    public sealed class PriceProcessor : DataProcessorBase<PriceProcessor.PriceInfo>
    {
        private IPriceSubscription _subscription;
        private readonly PriceInfo _priceInfo = new PriceInfo();

        public IContract SubscribedContract { get; private set; }

        public PriceProcessor(IGFClient client)
            : base(client)
        {
            LoadItems(new [] { _priceInfo });
        }

        public void Subscribe(IContract contract)
        {
            _subscription?.Unsubscribe();
            SubscribedContract = contract;

            if (contract != null)
                _subscription = Client.Subscriptions.Price.Subscribe(contract.ID);

            _priceInfo.SetContract(contract);
            UpdatePrice();
        }

        protected override void OnLoginComplete(IGFClient client, LoginCompleteEventArgs e)
        {
            base.OnLoginComplete(client, e);
            Client.Subscriptions.Price.PriceChanged += OnPriceChanged;
        }

        protected override void OnDisconnected(IGFClient client, DisconnectedEventArgs e)
        {
            SubscribedContract = null;
            base.OnDisconnected(client, e);
        }

        protected override void ClearDataSource()
        {
            // suppress DataSource cleanup - just unsubscribe
            Subscribe(null);
        }

        private void OnPriceChanged(IGFClient client, PriceChangedEventArgs e)
        {
            if (e.Contract.ID == SubscribedContract.ID)
                UpdatePrice();
        }

        /// <summary>
        /// Updates current price information
        /// </summary>
        private void UpdatePrice()
        {
            ResetItem(0);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
                Client.Subscriptions.Price.PriceChanged -= OnPriceChanged;
        }

        [TypeConverter(typeof(Converters.PriceInfo))]
        public sealed class PriceInfo
        {
            public IContract Contract { get; private set; }

            public void SetContract(IContract contract)
            {
                Contract = contract;
            }
        }
    }
}
