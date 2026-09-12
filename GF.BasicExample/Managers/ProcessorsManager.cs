using System;
using System.Collections.Generic;
using System.Windows.Forms;
using GF.BasicExample.Processors;

namespace GF.BasicExample.Managers
{
    public sealed class ProcessorsManager : IDisposable
    {
        private readonly Action<string> _updateStatus;
        private readonly HashSet<IClientDataProcessor> _processors = new HashSet<IClientDataProcessor>();

        public ProcessorsManager(Action<string> updateStatus)
        {
            _updateStatus = updateStatus;
        }

        public T RegisterProcessor<T>(T processor, Control viewControl)
            where T : IClientDataProcessor
        {
            viewControl.BindTo(processor.DataSource);
            return RegisterProcessor(processor);
        }

        public T RegisterProcessor<T>(T processor, ListControl viewControl)
            where T : IClientDataProcessor
        {
            viewControl.BindTo(processor.DataSource);
            return RegisterProcessor(processor);
        }

        private T RegisterProcessor<T>(T processor)
            where T : IClientDataProcessor
        {
            _processors.Add(processor);
            processor.StatusUpdate += OnProcessorStatusUpdate;
            return processor;
        }

        private void OnProcessorStatusUpdate(object sender, string status)
        {
            _updateStatus?.Invoke(status);
        }

        public void Dispose()
        {
            foreach (var processor in _processors)
            {
                processor.StatusUpdate -= OnProcessorStatusUpdate;
                processor.Dispose();
            }
        }
    }
}
