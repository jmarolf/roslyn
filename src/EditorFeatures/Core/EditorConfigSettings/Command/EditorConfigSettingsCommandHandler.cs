// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Editor;
using Microsoft.CodeAnalysis.Editor.Implementation.IntelliSense;
using Microsoft.CodeAnalysis.Editor.Shared.Utilities;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.VisualStudio.Commanding;
using Microsoft.VisualStudio.Threading;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.Editor.Commanding
{
    [Export(typeof(ICommandHandler))]
    [ContentType(ContentTypeNames.RoslynContentType)]
    [Name(PredefinedCommandHandlerNames.EditorConfigSettings)]
    internal class EditorConfigSettingsCommandHandler : ICommandHandler<EditorConfigSettingsCommandArgs>
    {
        private readonly IThreadingContext _threadingContext;
        private readonly IDocumentProvider _documentProvider;
        private readonly IEditorConfigSettingsBroker _broker;

        public string DisplayName => EditorFeaturesResources.View_editorconfig_settings;

        [ImportingConstructor]
        [Obsolete(MefConstruction.ImportingConstructorMessage, error: true)]
        public EditorConfigSettingsCommandHandler(
            IThreadingContext threadingContext,
            IDocumentProvider documentProvider,
            IEditorConfigSettingsBroker broker)
        {
            _threadingContext = threadingContext;
            _documentProvider = documentProvider;
            _broker = broker;
        }

        public bool ExecuteCommand(EditorConfigSettingsCommandArgs args, CommandExecutionContext executionContext)
        {
            var token = executionContext.OperationContext.UserCancellationToken;
            _ = _threadingContext.JoinableTaskFactory.RunAsync(async () =>
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }

                // ensure we switch to the background thread.
                await TaskScheduler.Default;
                // Launch the tabular data control
                var document = _documentProvider.GetDocument(args.TextView.TextSnapshot, token);
                await _broker.ShowEditorConfigSettingsAsync(document, token).ConfigureAwait(false);
            });
            return true;
        }

        public CommandState GetCommandState(EditorConfigSettingsCommandArgs args) => CommandState.Available;
    }
}
