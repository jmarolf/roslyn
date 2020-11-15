// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.CodeAnalysis.Editor
{
    internal class EditorConfigDocumentSettingsDataSource : EditorConfigSettingsDataSource
    {
        public EditorConfigDocumentSettingsDataSource(Document document, AnalyzerConfigSet analyzerConfigSet, IDiagnosticAnalyzerService diagnosticAnalyzerService)
        {
            BeginEditorConfigSettingsSearch(document, analyzerConfigSet, diagnosticAnalyzerService);
        }

        private void BeginEditorConfigSettingsSearch(Document document, AnalyzerConfigSet analyzerConfigSet, IDiagnosticAnalyzerService diagnosticAnalyzerService)
        {
            Task.Run(() =>
            {
                var compilationOptions = document.Project.CompilationOptions!;
                var path = document.FilePath!;
                var analyzerReferences = document.Project.AnalyzerReferences;
                GetOptionsForType(compilationOptions, path, analyzerReferences, diagnosticAnalyzerService, analyzerConfigSet, token: CancellationTokenSource.Token);
            }, CancellationTokenSource.Token);
        }
    }
}
