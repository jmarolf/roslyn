// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.CodeAnalysis.Editor
{
    internal class EditorConfigSettingsDataRepository : IEditorConfigSettingsDataRepository
    {
        private readonly EditorConfigSettingsBroker _editorConfigSettingsBroker;
        private IEditorConfigSettingsPresenter _presenter;
        private bool searchEnded = false;

        // Disallow concurrent modification of results
        private readonly object gate = new object();

        private readonly SortedDictionary<string, Dictionary<int, EditorConfigSetting>> _resultsPerOrigin;

        public EditorConfigSettingsDataRepository(EditorConfigSettingsBroker editorConfigSettingsBroker)
        {
            _editorConfigSettingsBroker = editorConfigSettingsBroker ?? throw new ArgumentNullException(nameof(editorConfigSettingsBroker));
            _resultsPerOrigin = new SortedDictionary<string, Dictionary<int, EditorConfigSetting>>();
        }

        public void AddRange(ImmutableArray<EditorConfigSetting> results, IEnumerable<string>? additionalColumns = null)
        {
            if (searchEnded)
            {
                throw new InvalidOperationException("The search has already ended");
            }

            lock (gate)
            {
                // add items to _resultsPerOrigin dictionary
            }

            NotifyPresenter(results, additionalColumns);
        }

        private void NotifyPresenter(ImmutableArray<EditorConfigSetting> results, IEnumerable<string>? additionalColumns = null)
        {
            _ = Task.Run(async delegate
            {
                await _presenter.NotifyOfUpdateAsync(results, additionalColumns).ConfigureAwait(false);
            });
        }

        public ImmutableArray<EditorConfigSetting> GetCurrentDataSnapshot()
        {
            if (searchEnded)
            {
                throw new InvalidOperationException("The search has already ended");
            }

            lock (gate)
            {
                return _resultsPerOrigin.SelectMany(n => n.Value).Select(n => n.Value).ToImmutableArray();
            }
        }

        public void RegisterPresenter(IEditorConfigSettingsPresenter presenter)
        {
            _presenter = presenter ?? throw new ArgumentNullException(nameof(presenter));
            _presenter.SearchDismissed += OnSearchDismissed;
        }

        private void OnSearchDismissed(object sender, EventArgs e)
        {
            FreeResources();
        }

        private void FreeResources()
        {
            if (searchEnded)
                return;

            _presenter.SearchDismissed -= OnSearchDismissed;
            searchEnded = true;
        }
    }
}
