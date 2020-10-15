using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Shell.TableManager;

namespace Microsoft.CodeAnalysis.Editor.EditorConfigSettings
{
    internal partial class EditorConfigSettingsPresenter
    {
        private class RemoveSinkWhenDisposed : IDisposable
        {
            private readonly List<ITableDataSink> _tableSinks;
            private readonly ITableDataSink _sink;

            public RemoveSinkWhenDisposed(List<ITableDataSink> tableSinks, ITableDataSink sink)
            {
                _tableSinks = tableSinks;
                _sink = sink;
            }

            public void Dispose()
            {
                // whoever subscribed is no longer interested in my data.
                // Remove them from the list of sinks
                _tableSinks.Remove(_sink);
            }
        }
    }
}
