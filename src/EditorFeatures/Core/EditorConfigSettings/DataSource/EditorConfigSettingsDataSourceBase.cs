// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeStyle;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Options;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Shared.Extensions;

namespace Microsoft.CodeAnalysis.Editor
{
    internal abstract class EditorConfigSettingsDataSourceBase : IEditorConfigSettingsDataSource
    {
        protected readonly CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();

        // Disallow concurrent modification of results
        private readonly object gate = new object();
        private readonly List<EditorConfigSetting> _snapshot = new List<EditorConfigSetting>();
        internal bool Completed = false;
        private IEditorConfigSettingsPresenter? _presenter;

        public void AddRange(ImmutableArray<EditorConfigSetting> results, IEnumerable<string>? additionalColumns = null)
        {
            if (Completed)
            {
                throw new InvalidOperationException("The search has already ended");
            }

            lock (gate)
            {
                _snapshot.AddRange(results);
            }

            NotifyPresenter(results, additionalColumns);
        }

        public ImmutableArray<EditorConfigSetting> GetCurrentDataSnapshot()
        {
            if (Completed)
            {
                throw new InvalidOperationException("The search has already ended"); // TODO(jmarolf): update string
            }

            return _snapshot.ToImmutableArray();
        }

        public void RegisterPresenter(IEditorConfigSettingsPresenter presenter)
        {
            _presenter = presenter ?? throw new ArgumentNullException(nameof(presenter));
            _presenter.SearchDismissed += OnSearchDismissed;
        }

        private void NotifyPresenter(ImmutableArray<EditorConfigSetting> results, IEnumerable<string>? additionalColumns = null)
        {
            _presenter?.NotifyOfUpdateAsync(results, additionalColumns);
        }

        private void OnSearchDismissed(object sender, EventArgs e)
        {
            CancellationTokenSource.Cancel();
            FreeResources();
        }

        private void FreeResources()
        {
            if (Completed)
                return;

            if (_presenter is not null)
            {
                _presenter.SearchDismissed -= OnSearchDismissed;

            }
            Completed = true;
        }
    }
}
