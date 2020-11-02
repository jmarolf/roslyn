// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Test.Utilities;
using Xunit;

namespace Microsoft.CodeAnalysis.Editor.UnitTests.EditorConfigSettings
{
    public partial class EditorConfigSettingsBrokerTests
    {
        [Fact, UseExportProvider]
        public async Task TestShowSettingsDocumentCancellation()
        {
            using var workspace = CreateWorkspace();
            var (broker, _, _) = CreateBroker();
            var document = CreateDocument(workspace);
            using var source = new CancellationTokenSource();
            var token = source.Token;
            source.Cancel();
            _ = await Assert.ThrowsAsync<OperationCanceledException>(async () => await broker.ShowEditorConfigSettingsAsync(document, token));
        }

        [Fact, UseExportProvider]
        public async Task TestShowSettingsProjectCancellation()
        {
            using var workspace = CreateWorkspace();
            var (broker, _, _) = CreateBroker();
            var project = CreateProject(workspace);
            using var source = new CancellationTokenSource();
            var token = source.Token;
            source.Cancel();
            _ = await Assert.ThrowsAsync<OperationCanceledException>(async () => await broker.ShowEditorConfigSettingsAsync(project, token));
        }

        [Fact, UseExportProvider]
        public async Task TestShowSettingsSolutionCancellation()
        {
            using var workspace = CreateWorkspace();
            var (broker, _, _) = CreateBroker();
            var solution = CreateSolution(workspace);
            using var source = new CancellationTokenSource();
            var token = source.Token;
            source.Cancel();
            _ = await Assert.ThrowsAsync<OperationCanceledException>(async () => await broker.ShowEditorConfigSettingsAsync(solution, token));
        }
    }
}
