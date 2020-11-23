// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Shared.Extensions;

namespace Microsoft.CodeAnalysis.Editor
{

    internal abstract class EditorConfigSettingsDataSource
        : EditorConfigSettingsDataSourceBase
    {
        protected void GetOptionsForType(CompilationOptions compilationOptions,
                                       string path,
                                       IEnumerable<AnalyzerReference> analyzerReferences,
                                       IDiagnosticAnalyzerService diagnosticAnalyzerService,
                                       AnalyzerConfigSet analyzerConfigSet,
                                       CancellationToken token)
        {
            foreach (var analyzerReference in analyzerReferences)
            {
                var configSettings = GetEditorConfigSettings(analyzerReference, path, analyzerConfigSet, compilationOptions, diagnosticAnalyzerService, token);
                AddRange(configSettings);
            }
        }

        private static ImmutableArray<EditorConfigSetting> GetEditorConfigSettings(AnalyzerReference analyzerReference,
                                                                                   string path,
                                                                                   AnalyzerConfigSet analyzerConfigSet,
                                                                                   CompilationOptions compilationOptions,
                                                                                   IDiagnosticAnalyzerService diagnosticAnalyzerService,
                                                                                   CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            var analyzerOptions = analyzerConfigSet.GetOptionsForSourcePath(path);
            var settings = analyzerReference.GetAnalyzersForAllLanguages() // TODO(jmarolf): is it ok to include for all language?
                    .SelectMany(a => diagnosticAnalyzerService.AnalyzerInfoCache.GetDiagnosticDescriptors(a))
                    .GroupBy(d => d.Id)
                    .OrderBy(g => g.Key, StringComparer.CurrentCulture)
                    .Select(g =>
                    {
                        var selectedDiagnostic = g.First(); // TODO(jmarolf): write customer comparer
                        var severity = selectedDiagnostic.GetEffectiveSeverity(compilationOptions, analyzerOptions);
                        return EditorConfigSetting.Create(selectedDiagnostic, severity);
                    });
            return settings.ToImmutableArray();
        }
    }
}
