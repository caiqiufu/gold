using System;
using GF.Api;

namespace GF.BasicExample.Runner
{
    public interface IClientRunner : IDisposable
    {
        IGFClient Client { get; }

        void Start();
        void Stop();
    }
}