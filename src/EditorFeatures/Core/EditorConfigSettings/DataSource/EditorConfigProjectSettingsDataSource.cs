// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.CodeAnalysis.Editor
{
    internal class EditorConfigProjectSettingsDataSource : EditorConfigSettingsDataSource
    {
        public EditorConfigProjectSettingsDataSource(Project project, AnalyzerConfigSet analyzerConfigSet, IDiagnosticAnalyzerService diagnosticAnalyzerService)
        {
            BeginEditorConfigSettingsSearch(project, analyzerConfigSet, diagnosticAnalyzerService);
        }

        private void BeginEditorConfigSettingsSearch(Project project, AnalyzerConfigSet analyzerConfigSet, IDiagnosticAnalyzerService diagnosticAnalyzerService)
        {
            Task.Run(() =>
            {
                var compilationOptions = project.CompilationOptions!;
                var path = project.FilePath!;
                var analyzerReferences = project.AnalyzerReferences;
                GetOptionsForType(compilationOptions, path, analyzerReferences, diagnosticAnalyzerService, analyzerConfigSet, token: CancellationTokenSource.Token);
            }, CancellationTokenSource.Token);
        }
    }
}
