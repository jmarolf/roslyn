// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Composition;
using Microsoft.CodeAnalysis.Editor.EditorConfigSettings;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.VisualStudio.LanguageServices.EditorConfigSettings;

namespace Microsoft.VisualStudio.LanguageServices.Implementation.EditorConfigUI
{
    [Export(typeof(IEditorConfigSettingsWindowProvider)), Shared]
    internal class EditorConfigSettingsDialogProvider : IEditorConfigSettingsWindowProvider
    {
        private readonly EditorConfigSettingsTableProvider _tableProvider;

        [ImportingConstructor]
        [Obsolete(MefConstruction.ImportingConstructorMessage, error: true)]
        public EditorConfigSettingsDialogProvider(EditorConfigSettingsTableProvider tableProvider)
        {
            _tableProvider = tableProvider;
        }

        public IEditorConfigSettingsWindow ShowWindow()
        {
            var manager = _tableProvider.TableManager;
            var tableControl = _tableProvider.CreateTableControl();
            tableControl.ShowGroupingLine = true;
            tableControl.DoColumnsAutoAdjust = true;
            var dialog = new EditorConfigSettingsWindow(tableControl.Control, manager);
            return dialog;
        }
    }
}
