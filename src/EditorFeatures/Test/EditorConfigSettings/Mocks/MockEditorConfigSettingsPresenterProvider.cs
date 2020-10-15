// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.CodeAnalysis.Editor.UnitTests.EditorConfigSettings.Mocks
{
    internal class MockEditorConfigSettingsPresenterProvider : IEditorConfigSettingsPresenterProvider
    {
        public Task ShowAsync(IEditorConfigSettingsDataRepository dataRepository, CancellationToken token)
        {
            var presenter = new MockEditorConfigSettingsPresenter(dataRepository);
            dataRepository?.RegisterPresenter(presenter);
            return presenter.ShowAsync();
        }
    }
}
