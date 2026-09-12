using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using GF.Api;
using GF.Api.Connection;
using GF.Api.Contracts;
using GF.Api.Contracts.Criteria;
using GF.Api.Contracts.Load.Expression;
using GF.Api.Contracts.Load.Request;
using GF.Api.Contracts.Lookup.Request;
using GF.Api.Values.Contracts;
using GF.BasicExample.Converters;
using GF.BasicExample.Managers;

namespace GF.BasicExample.Processors
{
    public sealed class ContractsProcessor : DataProcessorBase<IContract>, IDescriptionProvider
    {
        private readonly string _baseContractName;

        public HashSet<IContract> Contracts { get; } = new HashSet<IContract>();

        static ContractsProcessor()
        {
            TypeDescriptor.AddAttributes(typeof(IContract), new TypeConverterAttribute(typeof(Contract)));
        }

        public ContractsProcessor(IGFClient client, string baseContractName)
            : base(client)
        {
            _baseContractName = baseContractName;
            Client.Contracts.Load.ContractLoadReceived += OnContractLoadReceived;
            Client.Contracts.Lookup.SymbolLookupReceived += OnSymbolLookupReceived;
        }

        public string GetItemDescription(object item)
        {
            return $"{item} - {((IContract)item).Description}";
        }

        protected override void OnLoginComplete(IGFClient client, LoginCompleteEventArgs e)
        {
            ClearDataSource();
            LoadContracts();
            base.OnLoginComplete(client, e);
        }

        protected override void ClearDataSource()
        {
            Contracts.Clear();
            base.ClearDataSource();
        }

        private void LoadContracts()
        {
            OnStatusUpdate("Loading contracts...");
            /*
            var cr = new SymbolLookupRequestBuilder()
                .WithResultCount(10)
                .WithSymbol(TextSearch.CreateStartsWith("OEW3K19 C2950"))
                .WithDescription(TextSearch.CreateStartsWith("OEW3K19 C2950"))
                .WithExpression(new ContractKindsCriterion(Enum.GetValues(typeof(ContractKind)).Cast<ContractKind>().Skip(1).ToArray()))
                .Build();

            // lookup 5 contracts, which symbol started with _baseContractName
            Cache<SymbolLookupRequestID>.Put(Client.Contracts.Lookup.ByCriteria(cr));
            */

            // lookup 5 contracts, which symbol started with _baseContractName
            Cache<SymbolLookupRequestID>.Put(Client.Contracts.Lookup.ByCriteria(
                new SymbolLookupRequestBuilder()
                    .WithResultCount(5)
                    .WithBaseSymbol(TextSearch.CreateStartsWith(_baseContractName))
                    .Build()));

            // load contracts from Indices group (2 per base contract)
            var indicesGroupID = Client.Contracts.Groups.Get("Indices").ID;

            var requests = Client.Contracts.Base.Get()
                .Where(bc => bc.IsFuture &&bc.Symbol.Contains("GC"))// .Where(bc => bc.IsFuture && bc.ContractGroup.ID == indicesGroupID && bc.Type == ContractType.Electronic)
                .Select(bc => new ContractLoadRequestBuilder()
                    .WithResultCount(5)
                    .WithExpression(new ContractLoadExpressionBuilder()
                        .Push(new BaseContractIDCriterion(bc.ID))
                        .Push(new ContractKindsCriterion(new [] {  ContractKind.Continuous }))//ContractKind.Future,
                        .Push(ContractLoadExpressionOperator.And)
                        .Build())
                    .Build());

            foreach (var request in requests)
                Cache<ContractLoadRequestID>.Put(Client.Contracts.Load.Request(request));
        }

        private void OnContractLoadReceived(IGFClient client, Api.Contracts.Load.ContractLoadResponseEventArgs e)
        {
            Cache<ContractLoadRequestID>.Remove(e.RequestID);
            UpdateContracts(e.Contracts);
        }

        private void OnSymbolLookupReceived(IGFClient client, Api.Contracts.Lookup.SymbolLookupEventArgs e)
        {
            Cache<SymbolLookupRequestID>.Remove(e.RequestID);
            UpdateContracts(e.Contracts);
        }

        private void UpdateContracts(IEnumerable<IContract> contracts)
        {
            LoadItems(contracts.Where(c => Contracts.Add(c)), false);

            if (Cache<ContractLoadRequestID>.Count == 0 && Cache<SymbolLookupRequestID>.Count == 0)
                OnStatusUpdate($"Contracts loading done (total: {Contracts.Count}).");
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                Client.Contracts.Load.ContractLoadReceived -= OnContractLoadReceived;
                Client.Contracts.Lookup.SymbolLookupReceived -= OnSymbolLookupReceived;
            }
        }

        private static class Cache<T>
        {
            private static readonly HashSet<T> Items = new HashSet<T>();

            public static int Count
            {
                get
                {
                    lock (Items)
                        return Items.Count;
                }
            }

            public static void Put(T id)
            {
                lock (Items)
                    Items.Add(id);
            }

            public static void Remove(T id)
            {
                lock (Items)
                    Items.Remove(id);
            }
        }
    }
}
