using System;
using Microsoft.VisualStudio.Shell.TableManager;

namespace Microsoft.CodeAnalysis.Editor.EditorConfigSettings
{
    internal partial class EditorConfigSettingsPresenter
    {
        internal class SourceToAddColumns : ITableDataSource
        {
            public string SourceTypeIdentifier => "FindAllReferences";

            public string Identifier => nameof(SourceToAddColumns);

            public string DisplayName => null;

            public IDisposable Subscribe(ITableDataSink sink)
            {
                // SymbolSearchPresenter already added an instance of SymbolSnapshotFactory.
                return new NoopDisposable();
            }

            private class NoopDisposable : IDisposable
            {
                public NoopDisposable()
                {
                }

                public void Dispose()
                {
                }
            }
        }
    }
}
