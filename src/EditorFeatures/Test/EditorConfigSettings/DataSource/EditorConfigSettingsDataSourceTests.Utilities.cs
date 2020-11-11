// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Editor.UnitTests.Diagnostics;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Test.Utilities;
using Microsoft.CodeAnalysis.Text;
using Xunit;

namespace Microsoft.CodeAnalysis.Editor.UnitTests.EditorConfigSettings.DataSource
{
    public partial class EditorConfigSettingsDataSourceTests
    {
        private static EditorConfigSettingsDataSource CreateDocuementDataSource(string? editorConfig = null)
        {
            using var workspace = CreateWorkspace();
            var analyzerReference = new AnalyzerImageReference(ImmutableArray.Create<DiagnosticAnalyzer>(new Analyzer()));
            var document = GetDocumentFromIncompleteProject(workspace, new[] { analyzerReference });
            Assert.IsType<MockDiagnosticUpdateSourceRegistrationService>(((IMefHostExportProvider)workspace.Services.HostServices).GetExportedValue<IDiagnosticUpdateSourceRegistrationService>());
            var service = Assert.IsType<DiagnosticAnalyzerService>(((IMefHostExportProvider)workspace.Services.HostServices).GetExportedValue<IDiagnosticAnalyzerService>());

            var configs = ArrayBuilder<AnalyzerConfig>.GetInstance();
            editorConfig ??= @"
[*.cs]
dotnet_diagnostic.cs000.severity = none

[*.vb]
dotnet_diagnostic.cs000.severity = error";

            configs.Add(AnalyzerConfig.Parse(editorConfig, "Z:\\.editorconfig"));
            var set = AnalyzerConfigSet.Create(configs);
            return new EditorConfigDocumentSettingsDataSource(document, set, service);
        }

        private static Document GetDocumentFromIncompleteProject(AdhocWorkspace workspace, IEnumerable<AnalyzerReference> analyzerReferences)
        {
            var project = workspace.AddProject(
                ProjectInfo.Create(
                    ProjectId.CreateNewId(),
                    VersionStamp.Create(),
                    "CSharpProject",
                    "CSharpProject",
                    LanguageNames.CSharp,
                    analyzerReferences: analyzerReferences));

            var documentId = DocumentId.CreateNewId(project.Id);
            var documentLoader = TextLoader.From(TextAndVersion.Create(SourceText.From("class A { B B {get} }"), VersionStamp.Create()));
            var docInfo = DocumentInfo.Create(documentId, "test.cs", loader: documentLoader, filePath: "Z:\\test.cs");
            return workspace.AddDocument(docInfo);
        }

        private class Analyzer : DiagnosticAnalyzer
        {
            internal static readonly DiagnosticDescriptor s_syntaxRule = new DiagnosticDescriptor("syntax", "test", "test", "test", DiagnosticSeverity.Error, isEnabledByDefault: true);
            internal static readonly DiagnosticDescriptor s_semanticRule = new DiagnosticDescriptor("semantic", "test", "test", "test", DiagnosticSeverity.Error, isEnabledByDefault: true);
            internal static readonly DiagnosticDescriptor s_compilationRule = new DiagnosticDescriptor("compilation", "test", "test", "test", DiagnosticSeverity.Error, isEnabledByDefault: true);

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(s_syntaxRule, s_semanticRule, s_compilationRule);

            public override void Initialize(AnalysisContext context)
            {
                context.RegisterSyntaxTreeAction(c => c.ReportDiagnostic(Diagnostic.Create(s_syntaxRule, c.Tree.GetRoot().GetLocation())));
                context.RegisterSemanticModelAction(c => c.ReportDiagnostic(Diagnostic.Create(s_semanticRule, c.SemanticModel.SyntaxTree.GetRoot().GetLocation())));
                context.RegisterCompilationAction(c => c.ReportDiagnostic(Diagnostic.Create(s_compilationRule, c.Compilation.SyntaxTrees.First().GetRoot().GetLocation())));
            }
        }

        private static AdhocWorkspace CreateWorkspace(Type[]? additionalParts = null)
            => new AdhocWorkspace(s_featuresCompositionWithMockDiagnosticUpdateSourceRegistrationService.AddParts(additionalParts).GetHostServices());

        private static readonly TestComposition s_featuresCompositionWithMockDiagnosticUpdateSourceRegistrationService = FeaturesTestCompositions.Features
            .AddExcludedPartTypes(typeof(IDiagnosticUpdateSourceRegistrationService))
            .AddParts(typeof(MockDiagnosticUpdateSourceRegistrationService));

        private class TestPresenter : IEditorConfigSettingsPresenter
        {
            private readonly Action<ImmutableArray<EditorConfigSetting>> _notifyOfUpdate;

            public TestPresenter(Action<ImmutableArray<EditorConfigSetting>>? notifyOfUpdate = null)
            {
                _notifyOfUpdate = notifyOfUpdate ?? r  => { };
            }

            public event EventHandler<EventArgs>? SearchDismissed;

            public Task NotifyOfUpdateAsync(ImmutableArray<EditorConfigSetting> results, IEnumerable<string>? additionalColumns)
            {
                _notifyOfUpdate(results);
                return Task.CompletedTask;
            }

            public Task ShowAsync()
            {
                throw new NotImplementedException();
            }
        }
    }
}
