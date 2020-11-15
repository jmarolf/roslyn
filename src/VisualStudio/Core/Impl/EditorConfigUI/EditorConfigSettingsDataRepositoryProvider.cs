// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Editor;
using Microsoft.CodeAnalysis.Host.Mef;

namespace Microsoft.VisualStudio.LanguageServices.Implementation.EditorConfigUI
{
    [Export(typeof(IEditorConfigSettingsDataRepositoryProvider))]
    internal class EditorConfigSettingsDataRepositoryProvider : IEditorConfigSettingsDataRepositoryProvider
    {
        private readonly VisualStudioWorkspace _workspace;
        private readonly IDiagnosticAnalyzerService _diagnosticAnalyzerService;

        [ImportingConstructor]
        [Obsolete(MefConstruction.ImportingConstructorMessage, error: true)]
        public EditorConfigSettingsDataRepositoryProvider(VisualStudioWorkspace workspace, IDiagnosticAnalyzerService diagnosticAnalyzerService)
        {
            _workspace = workspace;
            _diagnosticAnalyzerService = diagnosticAnalyzerService;
        }

        public IEditorConfigSettingsDataSource GetDataRepository(IEditorConfigSettingsBroker broker, string path)
        {
            var document = _workspace.CurrentSolution.Projects.SelectMany(p => p.Documents).Where(d => d.FilePath == path).FirstOrDefault();
            var optionSet = document.Project.State.GetAnalyzerConfigSetAsync(default).GetAwaiter().GetResult();
            return new EditorConfigDocumentSettingsDataSource(document, optionSet, _diagnosticAnalyzerService);
        }
    }
}
