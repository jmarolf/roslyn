// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Microsoft.CodeAnalysis.Editor
{
    internal class EditorConfigSolutionSettingsDataSource : EditorConfigSettingsDataSource
    {
        public EditorConfigSolutionSettingsDataSource(Solution solution, AnalyzerConfigSet analyzerConfigSet, IDiagnosticAnalyzerService diagnosticAnalyzerService)
        {
            BeginEditorConfigSettingsSearch(solution, analyzerConfigSet, diagnosticAnalyzerService);
        }

        private void BeginEditorConfigSettingsSearch(Solution solution, AnalyzerConfigSet analyzerConfigSet, IDiagnosticAnalyzerService diagnosticAnalyzerService)
        {
            Task.Run(() =>
            {
                var path = solution.FilePath!;
                foreach (var project in solution.Projects)
                {
                    var compilationOptions = project.CompilationOptions!;
                    var analyzerReferences = project.AnalyzerReferences;
                    GetOptionsForType(compilationOptions, path, analyzerReferences, diagnosticAnalyzerService, analyzerConfigSet, token: CancellationTokenSource.Token);
                }
            }, CancellationTokenSource.Token);
        }
    }
}
