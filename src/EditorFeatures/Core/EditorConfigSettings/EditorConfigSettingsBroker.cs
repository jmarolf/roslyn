// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.CodeAnalysis.Editor
{
    [Export(typeof(IEditorConfigSettingsBroker))]
    internal class EditorConfigSettingsBroker : IEditorConfigSettingsBroker
    {
        private readonly IEditorConfigSettingsPresenterProvider _editorPresenationProvider;
        private readonly IEditorConfigSettingsDataRepositoryProvider _editorConfigSettingsDataRepositoryProvider;

        [ImportingConstructor]
        public EditorConfigSettingsBroker(IEditorConfigSettingsPresenterProvider editorPresenationProvider,
                                          IEditorConfigSettingsDataRepositoryProvider editorConfigSettingsDataRepositoryProvider)
        {
            _editorPresenationProvider = editorPresenationProvider;
            _editorConfigSettingsDataRepositoryProvider = editorConfigSettingsDataRepositoryProvider;
        }

        public async Task ShowEditorConfigSettingsAsync(Document document, CancellationToken token)
        {
            // Cancellation source will be also controlled by the presenter to cancel populating data when user closes the UI
            var cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(token);
            var dataRepository = _editorConfigSettingsDataRepositoryProvider.GetDataRepository(this);
            var presenter = _editorPresenationProvider;
            _ = presenter.ShowAsync(dataRepository, cancellationSource.Token);
        }
    }
}
