using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using GF.Api;

namespace GF.BasicExample.Processors
{
    public abstract class DataProcessorBase<T> : IClientDataProcessor
    {
        private readonly SynchronizationContext _syncContext;
        private readonly BindingList<T> _bindingList = new BindingList<T>();

        protected readonly IGFClient Client;

        public IBindingList DataSource => _bindingList;

        public event StatusUpdateHandler StatusUpdate;

        protected DataProcessorBase(IGFClient client)
        {
            Client = client;
            Client.Connection.Aggregate.Disconnected += OnDisconnected;
            Client.Connection.Aggregate.LoginCompleted += OnLoginComplete;
            _syncContext = SynchronizationContext.Current ?? new WindowsFormsSynchronizationContext();
        }

        protected virtual void ClearDataSource()
        {
            SyncInvoke(() => DataSource.Clear());
        }

        protected void LoadItems(IEnumerable<T> items, bool replace = true)
        {
            LoadItems(items.Cast<object>(), replace);
        }

        protected void LoadItems(IEnumerable<object> items, bool replace = true)
        {
            SyncInvoke(() =>
            {
                var index = 0;

                foreach (var item in items)
                {
                    if (!replace || index >= DataSource.Count)
                        DataSource.Add(item);
                    else
                        DataSource[index] = item;

                    index++;
                }

                if (replace)
                {
                    for (var i = DataSource.Count - 1; i >= 0 && i > index; i++)
                        DataSource.RemoveAt(i);
                }
            });
        }

        protected void ResetItem(int index)
        {
            SyncInvoke(() => _bindingList.ResetItem(index));
        }

        protected virtual void OnLoginComplete(IGFClient client, Api.Connection.LoginCompleteEventArgs e)
        {
        }

        protected virtual void OnDisconnected(IGFClient client, Api.Connection.DisconnectedEventArgs e)
        {
            ClearDataSource();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Client.Connection.Aggregate.Disconnected -= OnDisconnected;
                Client.Connection.Aggregate.LoginCompleted -= OnLoginComplete;
            }
        }

        protected virtual void OnStatusUpdate(string status)
        {
            SyncInvoke(() => StatusUpdate?.Invoke(this, status));
        }

        protected void SyncInvoke(Action action)
        {
            _syncContext.Send(s => action(), null);
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}
