// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.CodeAnalysis.Editor.UnitTests.EditorConfigSettings.Mocks
{
    internal class MockEditorConfigSettingsPresenterProvider : IEditorConfigSettingsPresenterProvider
    {
        private MockEditorConfigSettingsPresenter Presenter { get; set; }

        public async Task<MockEditorConfigSettingsPresenter> GetPresenterAsync()
        {
            while (true)
            {
                if (Presenter is not null)
                {
                    return Presenter;
                }

                await Task.Yield();
            }
        }

        public Task ShowAsync(IEditorConfigSettingsDataRepository dataRepository, CancellationToken token)
        {
            Presenter = new MockEditorConfigSettingsPresenter(dataRepository);
            dataRepository?.RegisterPresenter(Presenter);
            return Presenter.ShowAsync();
        }
    }

    internal class MockEditorConfigSettingsDataRepositoryProvider : IEditorConfigSettingsDataRepositoryProvider
    {
        internal MockEditorConfigSettingsDataRepository DataRepository { get; } = new MockEditorConfigSettingsDataRepository();

        public IEditorConfigSettingsDataRepository GetDataRepository(IEditorConfigSettingsBroker broker) => DataRepository;
    }

    internal class MockEditorConfigSettingsDataRepository : IEditorConfigSettingsDataRepository
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
