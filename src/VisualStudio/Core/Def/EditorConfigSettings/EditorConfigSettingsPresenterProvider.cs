// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Editor.Shared.Utilities;
using Microsoft.CodeAnalysis.Host.Mef;

namespace Microsoft.CodeAnalysis.Editor.EditorConfigSettings
{
    [Export(typeof(IEditorConfigSettingsPresenterProvider)), Shared]
    internal class EditorConfigSettingsPresenterProvider : IEditorConfigSettingsPresenterProvider
    {
        private readonly IThreadingContext _threadingContext;

        [ImportingConstructor]
        [Obsolete(MefConstruction.ImportingConstructorMessage, error: true)]
        public EditorConfigSettingsPresenterProvider(IThreadingContext threadingContext)
        {
            _threadingContext = threadingContext;
        }

        public Task ShowAsync(IEditorConfigSettingsDataSource dataRepository, CancellationToken token)
        {
            var presenter = new EditorConfigSettingsPresenter(_threadingContext, dataRepository, new CancellationTokenSource());
            return presenter.ShowAsync();
        }
    }
}
