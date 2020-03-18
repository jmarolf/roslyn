﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.Diagnostics.Analyzers.NamingStyles;
using Microsoft.CodeAnalysis.Editor.UnitTests.Extensions;
using Microsoft.CodeAnalysis.Editor.UnitTests.Workspaces;
using Microsoft.CodeAnalysis.Shared.Utilities;
using Microsoft.CodeAnalysis.Test.Utilities;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.UnitTests;
using Roslyn.Utilities;
using Roslyn.Test.Utilities;
using Xunit;

#if CODE_STYLE
using EditorConfigFileGenerator = Microsoft.CodeAnalysis.Options.EditorConfigFileGenerator;
using Microsoft.CodeAnalysis.Internal.Options;
#else
using Microsoft.CodeAnalysis.CodeStyle;
using Microsoft.CodeAnalysis.Options;
#endif

namespace Microsoft.CodeAnalysis.Editor.UnitTests.CodeActions
{
    [UseExportProvider]
    public abstract partial class AbstractCodeActionOrUserDiagnosticTest
    {
        public struct TestParameters
        {
            internal readonly IDictionary<OptionKey, object> options;
            internal readonly object fixProviderData;
            internal readonly ParseOptions parseOptions;
            internal readonly CompilationOptions compilationOptions;
            internal readonly int index;
            internal readonly CodeActionPriority? priority;
            internal readonly bool retainNonFixableDiagnostics;
            internal readonly bool includeDiagnosticsOutsideSelection;
            internal readonly string title;

            internal TestParameters(
                ParseOptions parseOptions = null,
                CompilationOptions compilationOptions = null,
                IDictionary<OptionKey, object> options = null,
                object fixProviderData = null,
                int index = 0,
                CodeActionPriority? priority = null,
                bool retainNonFixableDiagnostics = false,
                bool includeDiagnosticsOutsideSelection = false,
                string title = null)
            {
                this.parseOptions = parseOptions;
                this.compilationOptions = compilationOptions;
                this.options = options;
                this.fixProviderData = fixProviderData;
                this.index = index;
                this.priority = priority;
                this.retainNonFixableDiagnostics = retainNonFixableDiagnostics;
                this.includeDiagnosticsOutsideSelection = includeDiagnosticsOutsideSelection;
                this.title = title;
            }

            public TestParameters WithParseOptions(ParseOptions parseOptions)
                => new TestParameters(parseOptions, compilationOptions, options, fixProviderData, index, priority, retainNonFixableDiagnostics, includeDiagnosticsOutsideSelection, title);

            public TestParameters WithOptions(IDictionary<OptionKey, object> options)
                => new TestParameters(parseOptions, compilationOptions, options, fixProviderData, index, priority, retainNonFixableDiagnostics, includeDiagnosticsOutsideSelection, title);

            public TestParameters WithFixProviderData(object fixProviderData)
                => new TestParameters(parseOptions, compilationOptions, options, fixProviderData, index, priority, retainNonFixableDiagnostics, includeDiagnosticsOutsideSelection, title);

            public TestParameters WithIndex(int index)
                => new TestParameters(parseOptions, compilationOptions, options, fixProviderData, index, priority, retainNonFixableDiagnostics, includeDiagnosticsOutsideSelection, title);

            public TestParameters WithRetainNonFixableDiagnostics(bool retainNonFixableDiagnostics)
                => new TestParameters(parseOptions, compilationOptions, options, fixProviderData, index, priority, retainNonFixableDiagnostics, includeDiagnosticsOutsideSelection, title);

            public TestParameters WithIncludeDiagnosticsOutsideSelection(bool includeDiagnosticsOutsideSelection)
                => new TestParameters(parseOptions, compilationOptions, options, fixProviderData, index, priority, retainNonFixableDiagnostics, includeDiagnosticsOutsideSelection, title);
        }

        private const string AutoGeneratedAnalyzerConfigHeader = @"# auto-generated .editorconfig for code style options";

        protected abstract string GetLanguage();
        protected abstract ParseOptions GetScriptOptions();

        protected TestWorkspace CreateWorkspaceFromOptions(
            string initialMarkup, TestParameters parameters)
        {
            var workspace = TestWorkspace.IsWorkspaceElement(initialMarkup)
                 ? TestWorkspace.Create(initialMarkup, openDocuments: false)
                 : CreateWorkspaceFromFile(initialMarkup, parameters);

            // For CodeStyle layer testing, we create an .editorconfig at project root
            // to apply the options as workspace options are not available in CodeStyle layer.
            // Otherwise, we apply the options directly to the workspace.

#if CODE_STYLE
            // We need to ensure that our projects/documents are rooted for
            // execution from CodeStyle layer as we will be adding a rooted .editorconfig to each project
            // to apply the options.
            if (parameters.options != null)
            {
                MakeProjectsAndDocumentsRooted(workspace);
                AddAnalyzerConfigDocumentWithOptions(workspace, parameters.options);
            }
#else
            workspace.ApplyOptions(parameters.options);
#endif

            return workspace;
        }

        private static void MakeProjectsAndDocumentsRooted(TestWorkspace workspace)
        {
            const string defaultRootFilePath = @"z:\";
            var newSolution = workspace.CurrentSolution;
            foreach (var projectId in workspace.CurrentSolution.ProjectIds)
            {
                var project = newSolution.GetProject(projectId);

                string projectRootFilePath;
                if (!PathUtilities.IsAbsolute(project.FilePath))
                {
                    projectRootFilePath = defaultRootFilePath;
                    newSolution = newSolution.WithProjectFilePath(projectId, Path.Combine(projectRootFilePath, project.FilePath));
                }
                else
                {
                    projectRootFilePath = PathUtilities.GetPathRoot(project.FilePath);
                }

                foreach (var documentId in project.DocumentIds)
                {
                    var document = newSolution.GetDocument(documentId);
                    if (!PathUtilities.IsAbsolute(document.FilePath))
                    {
                        newSolution = newSolution.WithDocumentFilePath(documentId, Path.Combine(projectRootFilePath, document.FilePath));
                    }
                    else
                    {
                        Assert.Equal(projectRootFilePath, PathUtilities.GetPathRoot(document.FilePath));
                    }
                }
            }

            var applied = workspace.TryApplyChanges(newSolution);
            Assert.True(applied);
            return;
        }

        private static void AddAnalyzerConfigDocumentWithOptions(TestWorkspace workspace, IDictionary<OptionKey, object> options)
        {
            Debug.Assert(options != null);
            var analyzerConfigText = GenerateAnalyzerConfigText(options);

            var newSolution = workspace.CurrentSolution;
            foreach (var project in workspace.Projects)
            {
                Assert.True(PathUtilities.IsAbsolute(project.FilePath));
                var projectRootFilePath = PathUtilities.GetPathRoot(project.FilePath);
                var documentId = DocumentId.CreateNewId(project.Id);
                newSolution = newSolution.AddAnalyzerConfigDocument(
                    documentId,
                    ".editorconfig",
                    SourceText.From(analyzerConfigText),
                    filePath: Path.Combine(projectRootFilePath, ".editorconfig"));
            }

            var applied = workspace.TryApplyChanges(newSolution);
            Assert.True(applied);
            return;

            static string GenerateAnalyzerConfigText(IDictionary<OptionKey, object> options)
            {
                var textBuilder = new StringBuilder();

                // Add an auto-generated header at the top so we can skip this file in expected baseline validation.
                textBuilder.AppendLine(AutoGeneratedAnalyzerConfigHeader);
                textBuilder.AppendLine();

                foreach (var (optionKey, value) in options)
                {
                    foreach (var location in optionKey.Option.StorageLocations)
                    {
                        if (location is IEditorConfigStorageLocation2 editorConfigStorageLocation)
                        {
                            var editorConfigString = editorConfigStorageLocation.GetEditorConfigString(value, default);
                            if (editorConfigString != null)
                            {
                                textBuilder.AppendLine(GetSectionHeader(optionKey));
                                textBuilder.AppendLine(editorConfigString);
                                textBuilder.AppendLine();
                                break;
                            }

                            Assert.False(true, "Unexpected non-editorconfig option");
                        }
                        else if (value is NamingStylePreferences namingStylePreferences)
                        {
                            textBuilder.AppendLine(GetSectionHeader(optionKey));
                            EditorConfigFileGenerator.AppendNamingStylePreferencesToEditorConfig(namingStylePreferences, optionKey.Language, textBuilder);
                            textBuilder.AppendLine();
                            break;
                        }
                    }
                }

                return textBuilder.ToString();

                static string GetSectionHeader(OptionKey optionKey)
                {
                    if (optionKey.Option.IsPerLanguage)
                    {
                        switch (optionKey.Language)
                        {
                            case LanguageNames.CSharp:
                                return "[*.cs]";
                            case LanguageNames.VisualBasic:
                                return "[*.vb]";
                        }
                    }

                    return "[*]";
                }
            }
        }

        protected abstract TestWorkspace CreateWorkspaceFromFile(string initialMarkup, TestParameters parameters);

        private TestParameters WithRegularOptions(TestParameters parameters)
            => parameters.WithParseOptions(parameters.parseOptions?.WithKind(SourceCodeKind.Regular));

        private TestParameters WithScriptOptions(TestParameters parameters)
            => parameters.WithParseOptions(parameters.parseOptions?.WithKind(SourceCodeKind.Script) ?? GetScriptOptions());

        protected async Task TestMissingInRegularAndScriptAsync(
            string initialMarkup,
            TestParameters parameters = default)
        {
            await TestMissingAsync(initialMarkup, WithRegularOptions(parameters));
            await TestMissingAsync(initialMarkup, WithScriptOptions(parameters));
        }

        protected async Task TestMissingAsync(
            string initialMarkup,
            TestParameters parameters = default)
        {
            using (var workspace = CreateWorkspaceFromOptions(initialMarkup, parameters))
            {
                var (actions, _) = await GetCodeActionsAsync(workspace, parameters);
                Assert.True(actions.Length == 0, "An action was offered when none was expected");
            }
        }

        protected async Task TestDiagnosticMissingAsync(
            string initialMarkup, TestParameters parameters = default)
        {
            using (var workspace = CreateWorkspaceFromOptions(initialMarkup, parameters))
            {
                var diagnostics = await GetDiagnosticsWorkerAsync(workspace, parameters);
                Assert.True(0 == diagnostics.Length, $"Expected no diagnostics, but got {diagnostics.Length}");
            }
        }

        protected abstract Task<(ImmutableArray<CodeAction>, CodeAction actionToInvoke)> GetCodeActionsAsync(
            TestWorkspace workspace, TestParameters parameters);

        protected abstract Task<ImmutableArray<Diagnostic>> GetDiagnosticsWorkerAsync(
            TestWorkspace workspace, TestParameters parameters);

        protected Task TestSmartTagTextAsync(string initialMarkup, string displayText, int index)
            => TestSmartTagTextAsync(initialMarkup, displayText, new TestParameters(index: index));

        protected Task TestSmartTagGlyphTagsAsync(string initialMarkup, ImmutableArray<string> glyphTags, int index)
            => TestSmartTagGlyphTagsAsync(initialMarkup, glyphTags, new TestParameters(index: index));

        protected async Task TestSmartTagTextAsync(
            string initialMarkup,
            string displayText,
            TestParameters parameters = default)
        {
            using (var workspace = CreateWorkspaceFromOptions(initialMarkup, parameters))
            {
                var (_, action) = await GetCodeActionsAsync(workspace, parameters);
                Assert.Equal(displayText, action.Title);
            }
        }

        protected async Task TestSmartTagGlyphTagsAsync(
            string initialMarkup,
            ImmutableArray<string> glyph,
            TestParameters parameters = default)
        {
            using (var workspace = CreateWorkspaceFromOptions(initialMarkup, parameters))
            {
                var (_, action) = await GetCodeActionsAsync(workspace, parameters);
                Assert.Equal(glyph, action.Tags);
            }
        }

        protected async Task TestExactActionSetOfferedAsync(
            string initialMarkup,
            IEnumerable<string> expectedActionSet,
            TestParameters parameters = default)
        {
            using (var workspace = CreateWorkspaceFromOptions(initialMarkup, parameters))
            {
                var (actions, _) = await GetCodeActionsAsync(workspace, parameters);

                var actualActionSet = actions.Select(a => a.Title);
                Assert.True(actualActionSet.SequenceEqual(expectedActionSet),
                    "Expected: " + string.Join(", ", expectedActionSet) +
                    "\nActual: " + string.Join(", ", actualActionSet));
            }
        }

        protected async Task TestActionCountAsync(
            string initialMarkup,
            int count,
            TestParameters parameters = default)
        {
            using (var workspace = CreateWorkspaceFromOptions(initialMarkup, parameters))
            {
                var (actions, _) = await GetCodeActionsAsync(workspace, parameters);

                Assert.Equal(count, actions.Length);
            }
        }

        internal Task TestInRegularAndScriptAsync(
            string initialMarkup,
            string expectedMarkup,
            int index = 0,
            CodeActionPriority? priority = null,
            CompilationOptions compilationOptions = null,
            IDictionary<OptionKey, object> options = null,
            object fixProviderData = null,
            ParseOptions parseOptions = null,
            string title = null)
        {
            return TestInRegularAndScript1Async(
                initialMarkup, expectedMarkup, index,
                new TestParameters(parseOptions, compilationOptions, options, fixProviderData, index, priority, title: title));
        }

        internal async Task TestInRegularAndScript1Async(
            string initialMarkup,
            string expectedMarkup,
            int index = 0,
            TestParameters parameters = default)
        {
            parameters = parameters.WithIndex(index);
            await TestAsync(initialMarkup, expectedMarkup, WithRegularOptions(parameters));
            await TestAsync(initialMarkup, expectedMarkup, WithScriptOptions(parameters));
        }

        internal Task TestAsync(
            string initialMarkup, string expectedMarkup,
            ParseOptions parseOptions,
            CompilationOptions compilationOptions = null,
            int index = 0, IDictionary<OptionKey, object> options = null,
            object fixProviderData = null,
            CodeActionPriority? priority = null)
        {
            return TestAsync(
                initialMarkup,
                expectedMarkup,
                new TestParameters(
                    parseOptions, compilationOptions, options, fixProviderData, index, priority));
        }

        private async Task TestAsync(
            string initialMarkup,
            string expectedMarkup,
            TestParameters parameters)
        {
            MarkupTestFile.GetSpans(
                initialMarkup.NormalizeLineEndings(),
                out var initialMarkupWithoutSpans, out IDictionary<string, ImmutableArray<TextSpan>> initialSpanMap);

            const string UnnecessaryMarkupKey = "Unnecessary";
            var unnecessarySpans = initialSpanMap.GetOrAdd(UnnecessaryMarkupKey, _ => ImmutableArray<TextSpan>.Empty);

            MarkupTestFile.GetSpans(
                expectedMarkup.NormalizeLineEndings(),
                out var expected, out IDictionary<string, ImmutableArray<TextSpan>> expectedSpanMap);

            var conflictSpans = expectedSpanMap.GetOrAdd("Conflict", _ => ImmutableArray<TextSpan>.Empty);
            var renameSpans = expectedSpanMap.GetOrAdd("Rename", _ => ImmutableArray<TextSpan>.Empty);
            var warningSpans = expectedSpanMap.GetOrAdd("Warning", _ => ImmutableArray<TextSpan>.Empty);
            var navigationSpans = expectedSpanMap.GetOrAdd("Navigation", _ => ImmutableArray<TextSpan>.Empty);

            using (var workspace = CreateWorkspaceFromOptions(initialMarkup, parameters))
            {
                // Ideally this check would always run, but there are several hundred tests that would need to be
                // updated with {|Unnecessary:|} spans.
                if (unnecessarySpans.Any())
                {
                    var allDiagnostics = await GetDiagnosticsWorkerAsync(workspace, parameters
                        .WithRetainNonFixableDiagnostics(true)
                        .WithIncludeDiagnosticsOutsideSelection(true));

                    TestDiagnosticTags(allDiagnostics, unnecessarySpans, WellKnownDiagnosticTags.Unnecessary, UnnecessaryMarkupKey, initialMarkupWithoutSpans);
                }

                var (_, action) = await GetCodeActionsAsync(workspace, parameters);
                await TestActionAsync(
                    workspace, expected, action,
                    conflictSpans, renameSpans, warningSpans, navigationSpans,
                    parameters);
            }
        }

        private static void TestDiagnosticTags(
            ImmutableArray<Diagnostic> diagnostics,
            ImmutableArray<TextSpan> expectedSpans,
            string diagnosticTag,
            string markupKey,
            string initialMarkupWithoutSpans)
        {
            var diagnosticsWithTag = diagnostics
                .Where(d => d.Descriptor.CustomTags.Contains(diagnosticTag))
                .OrderBy(s => s.Location.SourceSpan.Start)
                .ToImmutableArray();

            if (expectedSpans.Length != diagnosticsWithTag.Length)
            {
                AssertEx.Fail(BuildFailureMessage(expectedSpans, diagnosticTag, markupKey, initialMarkupWithoutSpans, diagnosticsWithTag));
            }

            for (var i = 0; i < expectedSpans.Length; i++)
            {
                var actual = diagnosticsWithTag[i].Location.SourceSpan;
                var expected = expectedSpans[i];
                Assert.Equal(expected, actual);
            }
        }

        private static string BuildFailureMessage(
            ImmutableArray<TextSpan> expectedSpans,
            string diagnosticTag,
            string markupKey,
            string initialMarkupWithoutSpans,
            ImmutableArray<Diagnostic> diagnosticsWithTag)
        {
            var message = $"Expected {expectedSpans.Length} diagnostic spans with custom tag '{diagnosticTag}', but there were {diagnosticsWithTag.Length}.";

            if (expectedSpans.Length == 0)
            {
                message += $" If a diagnostic span tagged '{diagnosticTag}' is expected, surround the span in the test markup with the following syntax: {{|Unnecessary:...}}";

                var segments = new List<(int originalStringIndex, string segment)>();

                foreach (var diagnostic in diagnosticsWithTag)
                {
                    var documentOffset = initialMarkupWithoutSpans.IndexOf(diagnosticsWithTag.First().Location.SourceTree.ToString());
                    if (documentOffset == -1) continue;

                    segments.Add((documentOffset + diagnostic.Location.SourceSpan.Start, "{|" + markupKey + ":"));
                    segments.Add((documentOffset + diagnostic.Location.SourceSpan.End, "|}"));
                }

                if (segments.Any())
                {
                    message += Environment.NewLine
                        + "Example:" + Environment.NewLine
                        + Environment.NewLine
                        + InsertSegments(initialMarkupWithoutSpans, segments);
                }
            }

            return message;
        }

        private static string InsertSegments(string originalString, IEnumerable<(int originalStringIndex, string segment)> segments)
        {
            var builder = new StringBuilder();

            var positionInOriginalString = 0;

            foreach (var (originalStringIndex, segment) in segments.OrderBy(s => s.originalStringIndex))
            {
                builder.Append(originalString, positionInOriginalString, originalStringIndex - positionInOriginalString);
                builder.Append(segment);

                positionInOriginalString = originalStringIndex;
            }

            builder.Append(originalString, positionInOriginalString, originalString.Length - positionInOriginalString);
            return builder.ToString();
        }

        internal async Task<Tuple<Solution, Solution>> TestActionAsync(
            TestWorkspace workspace, string expected,
            CodeAction action,
            ImmutableArray<TextSpan> conflictSpans,
            ImmutableArray<TextSpan> renameSpans,
            ImmutableArray<TextSpan> warningSpans,
            ImmutableArray<TextSpan> navigationSpans,
            TestParameters parameters)
        {
            var operations = await VerifyActionAndGetOperationsAsync(workspace, action, parameters);
            return await TestOperationsAsync(
                workspace, expected, operations, conflictSpans, renameSpans,
                warningSpans, navigationSpans, expectedChangedDocumentId: null, parseOptions: parameters.parseOptions);
        }

        protected async Task<Tuple<Solution, Solution>> TestOperationsAsync(
            TestWorkspace workspace,
            string expectedText,
            ImmutableArray<CodeActionOperation> operations,
            ImmutableArray<TextSpan> conflictSpans,
            ImmutableArray<TextSpan> renameSpans,
            ImmutableArray<TextSpan> warningSpans,
            ImmutableArray<TextSpan> navigationSpans,
            DocumentId expectedChangedDocumentId,
            ParseOptions parseOptions = null)
        {
            var appliedChanges = ApplyOperationsAndGetSolution(workspace, operations);
            var oldSolution = appliedChanges.Item1;
            var newSolution = appliedChanges.Item2;

            if (TestWorkspace.IsWorkspaceElement(expectedText))
            {
                await VerifyAgainstWorkspaceDefinitionAsync(expectedText, newSolution);
                return Tuple.Create(oldSolution, newSolution);
            }

            var document = GetDocumentToVerify(expectedChangedDocumentId, oldSolution, newSolution);

            var fixedRoot = await document.GetSyntaxRootAsync();
            var actualText = fixedRoot.ToFullString();

            // To help when a user just writes a test (and supplied no 'expectedText') just print
            // out the entire 'actualText' (without any trimming).  in the case that we have both,
            // call the normal AssertEx helper which will print out a good diff.
            if (expectedText == "")
            {
                Assert.Equal((object)expectedText, actualText);
            }
            else
            {
                AssertEx.EqualOrDiff(expectedText, actualText);
            }

            TestAnnotations(conflictSpans, ConflictAnnotation.Kind);
            TestAnnotations(renameSpans, RenameAnnotation.Kind);
            TestAnnotations(warningSpans, WarningAnnotation.Kind);
            TestAnnotations(navigationSpans, NavigationAnnotation.Kind);

            return Tuple.Create(oldSolution, newSolution);

            void TestAnnotations(ImmutableArray<TextSpan> expectedSpans, string annotationKind)
            {
                var annotatedItems = fixedRoot.GetAnnotatedNodesAndTokens(annotationKind).OrderBy(s => s.SpanStart).ToList();

                Assert.True(expectedSpans.Length == annotatedItems.Count,
                    $"Annotations of kind '{annotationKind}' didn't match. Expected: {expectedSpans.Length}. Actual: {annotatedItems.Count}.");

                for (var i = 0; i < Math.Min(expectedSpans.Length, annotatedItems.Count); i++)
                {
                    var actual = annotatedItems[i].Span;
                    var expected = expectedSpans[i];
                    Assert.Equal(expected, actual);
                }
            }
        }

        protected static Document GetDocumentToVerify(DocumentId expectedChangedDocumentId, Solution oldSolution, Solution newSolution)
        {
            Document document;
            // If the expectedChangedDocumentId is not mentioned then we expect only single document to be changed
            if (expectedChangedDocumentId == null)
            {
                var projectDifferences = SolutionUtilities.GetSingleChangedProjectChanges(oldSolution, newSolution);

                var documentId = projectDifferences.GetChangedDocuments().FirstOrDefault() ?? projectDifferences.GetAddedDocuments().FirstOrDefault();
                Assert.NotNull(documentId);
                document = newSolution.GetDocument(documentId);
            }
            else
            {
                // This method obtains only the document changed and does not check the project state.
                document = newSolution.GetDocument(expectedChangedDocumentId);
            }

            return document;
        }

        private static async Task VerifyAgainstWorkspaceDefinitionAsync(string expectedText, Solution newSolution)
        {
            using (var expectedWorkspace = TestWorkspace.Create(expectedText))
            {
                var expectedSolution = expectedWorkspace.CurrentSolution;
                Assert.Equal(expectedSolution.Projects.Count(), newSolution.Projects.Count());
                foreach (var project in newSolution.Projects)
                {
                    var expectedProject = expectedSolution.GetProjectsByName(project.Name).Single();
                    Assert.Equal(expectedProject.Documents.Count(), project.Documents.Count());

                    foreach (var doc in project.Documents)
                    {
                        var root = await doc.GetSyntaxRootAsync();
                        var expectedDocument = expectedProject.Documents.Single(d => d.Name == doc.Name);
                        var expectedRoot = await expectedDocument.GetSyntaxRootAsync();
                        VerifyExpectedDocumentText(expectedRoot.ToFullString(), root.ToFullString());
                    }

                    foreach (var additionalDoc in project.AdditionalDocuments)
                    {
                        var root = await additionalDoc.GetTextAsync();
                        var expectedDocument = expectedProject.AdditionalDocuments.Single(d => d.Name == additionalDoc.Name);
                        var expectedRoot = await expectedDocument.GetTextAsync();
                        VerifyExpectedDocumentText(expectedRoot.ToString(), root.ToString());
                    }

                    foreach (var analyzerConfigDoc in project.AnalyzerConfigDocuments)
                    {
                        var root = await analyzerConfigDoc.GetTextAsync();
                        var actualString = root.ToString();
                        if (actualString.StartsWith(AutoGeneratedAnalyzerConfigHeader))
                        {
                            // Skip validation for analyzer config file that is auto-generated by test framework
                            // for applying code style options.
                            continue;
                        }

                        var expectedDocument = expectedProject.AnalyzerConfigDocuments.Single(d => d.FilePath == analyzerConfigDoc.FilePath);
                        var expectedRoot = await expectedDocument.GetTextAsync();
                        VerifyExpectedDocumentText(expectedRoot.ToString(), actualString);
                    }
                }
            }

            return;

            // Local functions.
            static void VerifyExpectedDocumentText(string expected, string actual)
            {
                if (expected == "")
                {
                    Assert.Equal((object)expected, actual);
                }
                else
                {
                    Assert.Equal(expected, actual);
                }
            }
        }

        internal async Task<ImmutableArray<CodeActionOperation>> VerifyActionAndGetOperationsAsync(
            TestWorkspace workspace, CodeAction action, TestParameters parameters)
        {
            if (action is null)
            {
                var diagnostics = await GetDiagnosticsWorkerAsync(workspace, parameters.WithRetainNonFixableDiagnostics(true));

                throw new Exception("No action was offered when one was expected. Diagnostics from the compilation: " + string.Join("", diagnostics.Select(d => Environment.NewLine + d.ToString())));
            }

            if (parameters.priority != null)
            {
                Assert.Equal(parameters.priority.Value, action.Priority);
            }

            if (parameters.title != null)
            {
                Assert.Equal(parameters.title, action.Title);
            }

            return await action.GetOperationsAsync(CancellationToken.None);
        }

        protected Tuple<Solution, Solution> ApplyOperationsAndGetSolution(
            TestWorkspace workspace,
            IEnumerable<CodeActionOperation> operations)
        {
            Tuple<Solution, Solution> result = null;
            foreach (var operation in operations)
            {
                if (operation is ApplyChangesOperation && result == null)
                {
                    var oldSolution = workspace.CurrentSolution;
                    var newSolution = ((ApplyChangesOperation)operation).ChangedSolution;
                    result = Tuple.Create(oldSolution, newSolution);
                }
                else if (operation.ApplyDuringTests)
                {
                    var oldSolution = workspace.CurrentSolution;
                    operation.TryApply(workspace, new ProgressTracker(), CancellationToken.None);
                    var newSolution = workspace.CurrentSolution;
                    result = Tuple.Create(oldSolution, newSolution);
                }
            }

            if (result == null)
            {
                throw new InvalidOperationException("No ApplyChangesOperation found");
            }

            return result;
        }

        protected virtual ImmutableArray<CodeAction> MassageActions(ImmutableArray<CodeAction> actions)
            => actions;

        protected static ImmutableArray<CodeAction> FlattenActions(ImmutableArray<CodeAction> codeActions)
        {
            return codeActions.SelectMany(a => a.NestedCodeActions.Length > 0
                ? a.NestedCodeActions
                : ImmutableArray.Create(a)).ToImmutableArray();
        }

        protected static ImmutableArray<CodeAction> GetNestedActions(ImmutableArray<CodeAction> codeActions)
            => codeActions.SelectMany(a => a.NestedCodeActions).ToImmutableArray();

        internal (OptionKey, object) SingleOption<T>(Option<T> option, T enabled)
            => (new OptionKey(option), enabled);

        protected (OptionKey, object) SingleOption<T>(PerLanguageOption<T> option, T value)
            => (new OptionKey(option, this.GetLanguage()), value);

        protected (OptionKey, object) SingleOption<T>(Option<CodeStyleOption<T>> option, T enabled, NotificationOption notification)
            => SingleOption(option, new CodeStyleOption<T>(enabled, notification));

        protected (OptionKey, object) SingleOption<T>(Option<CodeStyleOption<T>> option, CodeStyleOption<T> codeStyle)
            => (new OptionKey(option), codeStyle);

        protected (OptionKey, object) SingleOption<T>(PerLanguageOption<CodeStyleOption<T>> option, T enabled, NotificationOption notification)
            => SingleOption(option, new CodeStyleOption<T>(enabled, notification));

        protected (OptionKey, object) SingleOption<T>(PerLanguageOption<CodeStyleOption<T>> option, CodeStyleOption<T> codeStyle)
            => SingleOption(option, codeStyle, language: GetLanguage());

        protected static (OptionKey, object) SingleOption<T>(PerLanguageOption<CodeStyleOption<T>> option, CodeStyleOption<T> codeStyle, string language)
            => (new OptionKey(option, language), codeStyle);

        protected IDictionary<OptionKey, object> Option<T>(Option<CodeStyleOption<T>> option, T enabled, NotificationOption notification)
            => OptionsSet(SingleOption(option, enabled, notification));

        protected IDictionary<OptionKey, object> Option<T>(Option<CodeStyleOption<T>> option, CodeStyleOption<T> codeStyle)
            => OptionsSet(SingleOption(option, codeStyle));

        protected IDictionary<OptionKey, object> Option<T>(PerLanguageOption<CodeStyleOption<T>> option, T enabled, NotificationOption notification)
            => OptionsSet(SingleOption(option, enabled, notification));

        protected IDictionary<OptionKey, object> Option<T>(Option<T> option, T value)
            => OptionsSet(SingleOption(option, value));

        protected IDictionary<OptionKey, object> Option<T>(PerLanguageOption<T> option, T value)
            => OptionsSet(SingleOption(option, value));

        protected IDictionary<OptionKey, object> Option<T>(PerLanguageOption<CodeStyleOption<T>> option, CodeStyleOption<T> codeStyle)
            => OptionsSet(SingleOption(option, codeStyle));

        internal static IDictionary<OptionKey, object> OptionsSet(
            params (OptionKey key, object value)[] options)
        {
            var result = new Dictionary<OptionKey, object>();
            foreach (var option in options)
            {
                result.Add(option.key, option.value);
            }

            return result;
        }

        /// <summary>
        /// Tests all the code actions for the given <paramref name="input"/> string.  Each code
        /// action must produce the corresponding output in the <paramref name="outputs"/> array.
        ///
        /// Will throw if there are more outputs than code actions or more code actions than outputs.
        /// </summary>
        protected Task TestAllInRegularAndScriptAsync(
            string input,
            params string[] outputs)
        {
            return TestAllInRegularAndScriptAsync(input, parameters: default, outputs);
        }

        protected async Task TestAllInRegularAndScriptAsync(
            string input,
            TestParameters parameters,
            params string[] outputs)
        {
            for (var index = 0; index < outputs.Length; index++)
            {
                var output = outputs[index];
                await TestInRegularAndScript1Async(input, output, index, parameters: parameters);
            }

            await TestActionCountAsync(input, outputs.Length, parameters);
        }
    }
}
