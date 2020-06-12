using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using DotNetCore.CAP.Persistence;
using DotNetCore.CAP.Transport;

namespace Zxw.Framework.NetCore.Transaction
{
    public class DiagnosticProcessorObserver : IObserver<DiagnosticListener>
    {
        public const string DiagnosticListenerName = "SqlClientDiagnosticListener";
        private readonly IDispatcher _dispatcher;

        public DiagnosticProcessorObserver(IDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
            BufferList = new ConcurrentDictionary<Guid, List<MediumMessage>>();
        }

        public ConcurrentDictionary<Guid, List<MediumMessage>> BufferList { get; }

        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
        }

        public void OnNext(DiagnosticListener listener)
        {
            if (listener.Name == DiagnosticListenerName)
                listener.Subscribe(new DiagnosticObserver(_dispatcher, BufferList));
        }
    }
}
