using System;
using System.Windows.Forms;
using GF.Api;
using GF.Api.Threading;

namespace GF.BasicExample.Runner
{
    public abstract class ClientRunner : IClientRunner
    {
        public IGFClient Client { get; private set; }

        public static ClientRunner OnGuiThread()
        {
            return new GuiThread();
        }

        public static ClientRunner OnWorkerThread()
        {
            return new WorkerThread();
        }

        public virtual IClientRunner WithClient(IGFClient client)
        {
            Client = client;
            return this;
        }

        public virtual void Start()
        {
            if (Client == null)
                throw new InvalidOperationException("Runner isn't initialized");
        }

        public abstract void Stop();

        public virtual void Dispose()
        {
            Stop();
            Client = null;
        }

        private sealed class WorkerThread : ClientRunner
        {
            private GFClientRunner _runner;

            public override IClientRunner WithClient(IGFClient client)
            {
                _runner = new GFClientRunner(client);
                return base.WithClient(client);
            }

            public override void Start()
            {
                base.Start();
                _runner.Start();
            }

            public override void Stop()
            {
                _runner?.Stop();
            }

            public override void Dispose()
            {
                _runner?.Dispose();
            }
        }

        private sealed class GuiThread : ClientRunner
        {
            private readonly Timer _timer;

            public GuiThread()
            {
                _timer = new Timer { Interval = 250 };
                _timer.Tick += (s, a) => Client.Threading.Advance();
            }

            public override void Start()
            {
                base.Start();
                _timer.Start();
            }

            public override void Stop()
            {
                _timer.Stop();
            }

            public override void Dispose()
            {
                base.Dispose();
                _timer.Dispose();
            }
        }
    }
}
