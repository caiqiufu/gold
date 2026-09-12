using System;
using System.ComponentModel;

namespace GF.BasicExample.Processors
{
    public delegate void StatusUpdateHandler(object sender, string status);

    public interface IClientDataProcessor : IDisposable
    {
        IBindingList DataSource { get; }

        event StatusUpdateHandler StatusUpdate;
    }
}
