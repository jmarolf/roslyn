// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Editor.Shared.Utilities;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.VisualStudio.Shell.TableControl;
using Microsoft.VisualStudio.Shell.TableManager;

namespace Microsoft.CodeAnalysis.Editor.EditorConfigSettings
{
    [Export(typeof(IEditorConfigSettingsPresenterProvider))]
    internal class EditorConfigSettingsPresenterProvider : IEditorConfigSettingsPresenterProvider
    {
        private readonly IThreadingContext _threadingContext;
        private readonly IEditorConfigSettingsWindowProvider _editorConfigSettingsWindowProvider;

        [ImportingConstructor]
        [Obsolete(MefConstruction.ImportingConstructorMessage, error: true)]
        public EditorConfigSettingsPresenterProvider(IThreadingContext threadingContext,
                                                     IEditorConfigSettingsWindowProvider editorConfigSettingsWindowProvider)
        {
            _threadingContext = threadingContext;
            _editorConfigSettingsWindowProvider = editorConfigSettingsWindowProvider;
        }

        public Task ShowAsync(IEditorConfigSettingsDataRepository dataRepository, CancellationToken token)
        {
            var presenter = new EditorConfigSettingsPresenter(_threadingContext, _editorConfigSettingsWindowProvider, dataRepository, new CancellationTokenSource());
            return presenter.ShowAsync();
        }
    }

    internal partial class EditorConfigSettingsPresenter : IEditorConfigSettingsPresenter, ITableDataSource
    {
        public string SourceTypeIdentifier => "EditorConfigSettings";
        public string Identifier => ""; // TODO(jmarolf):: add resource
        public string DisplayName => ""; // TODO(jmarolf): add resource

        public event EventHandler<EventArgs>? SearchDismissed;
        private List<ITableDataSink> TableSinks { get; } = new List<ITableDataSink>();
        public CancellationTokenSource CancellationSource { get; }

        private IEditorConfigSettingsWindow _editorConfigSettingsWindow;
        private EditorConfigSettingsSnapshotFactory _editorConfigSettingsSnapshotFactory;
        private bool _initialized;
        private readonly IThreadingContext _threadingContext;
        private readonly IEditorConfigSettingsWindowProvider _editorConfigSettingsWindowProvider;
        private readonly IEditorConfigSettingsDataRepository _dataRepository;

        public EditorConfigSettingsPresenter(IThreadingContext threadingContext,
                                             IEditorConfigSettingsWindowProvider editorConfigSettingsWindowProvider,
                                             IEditorConfigSettingsDataRepository dataRepository,
                                             CancellationTokenSource cancellationSource)
        {
            _threadingContext = threadingContext;
            _editorConfigSettingsWindowProvider = editorConfigSettingsWindowProvider;
            _dataRepository = dataRepository;
            CancellationSource = cancellationSource;
        }

        public async Task NotifyOfUpdateAsync(ImmutableArray<EditorConfigSetting> results, IEnumerable<string>? additionalColumns)
        {
            // Notify our intermediate class that there is more data.
            _editorConfigSettingsSnapshotFactory.NotifyOfUpdate();

            // Don't notify table sinks if the search has ended
            if (CancellationSource.IsCancellationRequested)
            {
                return;
            }

            // If additional columns are defined, try to show them
            if (additionalColumns != null)
            {
                ShowColumns(additionalColumns);
            }

            // Open the Window to show progress
            await ShowAsync().ConfigureAwait(false);

            // Notify the sinks. Generally, VS Table Control will request data 500ms after the last notification.
            foreach (var tableSink in TableSinks)
            {
                // Notify that an update is available
                tableSink.FactorySnapshotChanged(null);
            }
        }

        public async Task CompleteSettingsPopulationAsync()
        {
            await _threadingContext.JoinableTaskFactory.SwitchToMainThreadAsync();
            foreach (var sink in TableSinks)
            {
                sink.IsStable = true;
            }
        }

        private string[] Columns { get; } = new[] {
            StandardTableColumnDefinitions.DocumentName,
            StandardTableColumnDefinitions.Line,
            StandardTableColumnDefinitions.Column,
            StandardTableColumnDefinitions.ProjectName,
            StandardTableColumnDefinitions2.Definition,
            "symbolkind",
            "SymbolSource",
        };

        public async Task ShowAsync()
        {
            if (_initialized)
            {
                return;
            }

            _initialized = true;
            await _threadingContext.JoinableTaskFactory.SwitchToMainThreadAsync();
            // TODO(jmarolf): may want to re-use the FAR table to show _something_ here
            _editorConfigSettingsWindow = _editorConfigSettingsWindowProvider.ShowWindow();
            _editorConfigSettingsWindow.Closed += OnWindowClosed;
            _editorConfigSettingsWindow.Manager.AddSource(this, Columns);
        }

        private void OnWindowClosed(object sender, EventArgs e)
        {
            EndSettingsPopulation();
        }

        private void EndSettingsPopulation()
        {
            CancellationSource.Cancel();
            SearchDismissed?.Invoke(this, EventArgs.Empty);

            if (_editorConfigSettingsWindow is not null)
            {
                _editorConfigSettingsWindow.Closed -= OnWindowClosed;
            }
        }

        private List<string> addedColumns = null;

        private void ShowColumns(IEnumerable<string> additionalColumns)
        {
            if (additionalColumns == null)
            {
                throw new ArgumentNullException(nameof(additionalColumns));
            }

            if (addedColumns != null)
            {
                // Ensure that we don't add a column twice
                var newlyAddedColumns = additionalColumns.Where(n => !addedColumns.Contains(n));
                addedColumns.AddRange(newlyAddedColumns);
                _editorConfigSettingsWindow.Manager.AddSource(new SourceToAddColumns(), newlyAddedColumns.ToArray());
            }
            else
            {
                addedColumns = additionalColumns.ToList();
                _editorConfigSettingsWindow.Manager.AddSource(new SourceToAddColumns(), additionalColumns.ToArray());
            }
        }

        /// <summary>
        /// This method is generally called only once (when the window is first created)
        /// but it is possible that an extender writes code which also subscribes to this information
        /// </summary>
        public IDisposable Subscribe(ITableDataSink sink)
        {
            sink.AddFactory(_editorConfigSettingsSnapshotFactory);
            TableSinks.Add(sink);
            return new RemoveSinkWhenDisposed(TableSinks, sink);
        }
    }
}
