// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Editor.UnitTests.Workspaces;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Remote;
using Microsoft.CodeAnalysis.Remote.Testing;
using Microsoft.CodeAnalysis.Serialization;
using Microsoft.CodeAnalysis.SolutionCrawler;
using Microsoft.CodeAnalysis.Test.Utilities;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Test.Utilities;
using Roslyn.Utilities;
using Xunit;

namespace Microsoft.CodeAnalysis.CSharp.UnitTests
{
    [UseExportProvider]
    [Trait(Traits.Feature, Traits.Features.RemoteHost)]
    public class WorkspaceAPITests
    {
        [Fact]
        [WorkItem(48216, "https://github.com/dotnet/roslyn/issues/48216")]
        public static async Task TestApplyAnalyzerReferenceAsync()
        {
            var code = @"class Test { void Method() { } }";
            using var workspace = TestWorkspace.CreateCSharp(code);

            var eventFired = new SemaphoreSlim(0);
            var solutionChangeCalled = 0;
            Assert.Empty(workspace.CurrentSolution.AnalyzerReferences);
            workspace.WorkspaceChanged += OnWorkspaceChanged;
            var analyzerService = Assert.IsType<DiagnosticAnalyzerService>(((IMefHostExportProvider)workspace.Services.HostServices).GetExportedValue<IDiagnosticAnalyzerService>());

            var incrementalAnalyzer = analyzerService.CreateIncrementalAnalyzer(workspace);

            var analyzer1 = new TestDiagnosticAnalyzer1();
            var analyzer2 = new TestDiagnosticAnalyzer2();
            var analyzersMap = new Dictionary<string, ImmutableArray<DiagnosticAnalyzer>>
            {
                { LanguageNames.CSharp, ImmutableArray.Create<DiagnosticAnalyzer>(analyzer1) },
                { LanguageNames.VisualBasic, ImmutableArray.Create<DiagnosticAnalyzer>(analyzer2) }
            };
            var references = new[] { new TestAnalyzerReferenceByLanguage(analyzersMap) };

            Assert.True(workspace.SetCurrentSolution(s => s.WithAnalyzerReferences(references), WorkspaceChangeKind.SolutionChanged));
            var analyzerReference = Assert.Single(workspace.CurrentSolution.AnalyzerReferences);
            Assert.Equal(2, analyzerReference.GetAnalyzersForAllLanguages().Length);

            await eventFired.WaitAsync(TimeSpan.FromSeconds(1));
            Assert.Equal(1, solutionChangeCalled);
            return;

            void OnWorkspaceChanged(object sender, WorkspaceChangeEventArgs e)
            {
                if (e.Kind == WorkspaceChangeKind.SolutionChanged)
                {
                    eventFired?.Release();
                    solutionChangeCalled++;
                }
            }
        }

        private class TestDiagnosticAnalyzer1 : DiagnosticAnalyzer
        {
            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }

            public override void Initialize(AnalysisContext context) { }
        }

        private class TestDiagnosticAnalyzer2 : DiagnosticAnalyzer
        {
            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }

            public override void Initialize(AnalysisContext context) { }
        }

    }
}
