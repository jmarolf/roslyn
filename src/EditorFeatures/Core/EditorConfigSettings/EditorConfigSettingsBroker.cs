// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Host.Mef;

namespace Microsoft.CodeAnalysis.Editor
{
    [Export(typeof(IEditorConfigSettingsBroker))]
    internal class EditorConfigSettingsBroker : IEditorConfigSettingsBroker
    {
        private readonly IEditorConfigSettingsPresenterProvider _editorPresenationProvider;

        [ImportingConstructor]
        [Obsolete(MefConstruction.ImportingConstructorMessage, error: true)]
        public EditorConfigSettingsBroker(IEditorConfigSettingsPresenterProvider editorPresenationProvider)
        {
            _editorPresenationProvider = editorPresenationProvider;
        }

        public async Task ShowEditorConfigSettingsAsync(Document document, CancellationToken token)
        {
            // Cancellation source will be also controlled by the presenter to cancel populating data when user closes the UI
            var cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(token);
            var dataRepository = new EditorConfigSettingsDataRepository(this);
            var presenter = _editorPresenationProvider;
            _ = presenter.ShowAsync(dataRepository, cancellationSource.Token);
        }
    }
}
