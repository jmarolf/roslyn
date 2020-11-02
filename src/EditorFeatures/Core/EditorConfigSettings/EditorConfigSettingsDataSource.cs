// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Editor.Shared.Utilities;
using Microsoft.CodeAnalysis.Options;
using Microsoft.CodeAnalysis.Shared.Extensions;

namespace Microsoft.CodeAnalysis.Editor
{
    internal class EditorConfigSettingsDataSource
        : IEditorConfigSettingsDataSource
    {
        private IEditorConfigSettingsPresenter _presenter;
        private bool searchEnded = false;

        // Disallow concurrent modification of results
        private readonly object gate = new object();
        private readonly Document _document;

        public EditorConfigSettingsDataSource(Document document)
        {
            _document = document;
        }

        public void AddRange(ImmutableArray<EditorConfigSetting> results, IEnumerable<string>? additionalColumns = null)
        {
            if (searchEnded)
            {
                throw new InvalidOperationException("The search has already ended");
            }

            lock (gate)
            {
                // add items to _resultsPerOrigin dictionary
            }

            NotifyPresenter(results, additionalColumns);
        }

        private void NotifyPresenter(ImmutableArray<EditorConfigSetting> results, IEnumerable<string>? additionalColumns = null)
        {
            _ = Task.Run(async delegate
            {
                await _presenter.NotifyOfUpdateAsync(results, additionalColumns).ConfigureAwait(false);
            });
        }

        public ImmutableArray<EditorConfigSetting> GetCurrentDataSnapshot()
        {
            if (searchEnded)
            {
                throw new InvalidOperationException("The search has already ended"); // TODO(jmarolf): update string
            }

            return ImmutableArray<EditorConfigSetting>.Empty;
        }

        private async Task GetOptionsForTypeAsync(Document document,
                                                  IDiagnosticAnalyzerService diagnosticAnalyzerService,
                                                  CancellationToken token)
        {
            var project = document.Project;
            var language = project.Language;
            var optionsProvider = project.CompilationOptions!.SyntaxTreeOptionsProvider!; // TODO(jmarolf): what are the cases where this is null?
            var tree = await document.GetSyntaxTreeAsync(token).ConfigureAwait(false);

            var analyzerReferences = document.Project.AnalyzerReferences;
            foreach (var analyzerReference in analyzerReferences)
            {
                var diagnostics = analyzerReference.GetAnalyzers(language)
                    .SelectMany(a => diagnosticAnalyzerService.AnalyzerInfoCache.GetDiagnosticDescriptors(a))
                    .GroupBy(d => d.Id)
                    .OrderBy(g => g.Key, StringComparer.CurrentCulture)
                    .Select(g =>
                    {
                        var selectedDiagnostic = g.OrderBy(d => d).First(); // TODO(jmarolf): write customer comparer
                        var severity = selectedDiagnostic.DefaultSeverity;
                        if (project.CompilationOptions is not null &&
                            selectedDiagnostic.GetEffectiveSeverity(project.CompilationOptions).ToDiagnosticSeverity() is DiagnosticSeverity compilerSeverity)
                        {
                            severity = compilerSeverity;
                        }
                        if (tree is not null &&
                            optionsProvider.TryGetDiagnosticValue(tree, selectedDiagnostic.Id, token, out var treeSeverity) &&
                            treeSeverity.ToDiagnosticSeverity() is DiagnosticSeverity specificSeverity)
                        {
                            severity = specificSeverity;
                        }

                        return new EditorConfigSetting(selectedDiagnostic, severity);
                    });
            }
        }

        public void RegisterPresenter(IEditorConfigSettingsPresenter presenter)
        {
            _presenter = presenter ?? throw new ArgumentNullException(nameof(presenter));
            _presenter.SearchDismissed += OnSearchDismissed;
        }

        private void OnSearchDismissed(object sender, EventArgs e)
        {
            FreeResources();
        }

        private void FreeResources()
        {
            if (searchEnded)
                return;

            _presenter.SearchDismissed -= OnSearchDismissed;
            searchEnded = true;
        }
    }
}
