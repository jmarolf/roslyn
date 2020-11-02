// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Editor.UnitTests.EditorConfigSettings.Mocks
{
    internal class MockEditorConfigSettingsDataSource : IEditorConfigSettingsDataSource
    {
        List<EditorConfigSetting> _settings = new List<EditorConfigSetting>();
        private IEditorConfigSettingsPresenter _presenter;

        public void AddRange(ImmutableArray<EditorConfigSetting> results, IEnumerable<string>? additionalColumns = null)
        {
            _settings.AddRange(results);
            _presenter.NotifyOfUpdateAsync(results, additionalColumns);
        }

        public ImmutableArray<EditorConfigSetting> GetCurrentDataSnapshot() => _settings.ToImmutableArray();

        public void RegisterPresenter(IEditorConfigSettingsPresenter presenter)
        {
            _presenter = presenter;
        }
    }
}
