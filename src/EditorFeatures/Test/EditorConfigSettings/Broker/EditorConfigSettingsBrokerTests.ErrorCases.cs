// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Test.Utilities;
using Xunit;

namespace Microsoft.CodeAnalysis.Editor.UnitTests.EditorConfigSettings
{
    public partial class EditorConfigSettingsBrokerTests
    {
        [Fact]
        public void TestShowSettingsAsyncPassingNullDocument()
        {
            var (broker, _, _) = CreateBroker();
            var exception = Assert.Throws<ArgumentNullException>(() => broker.ShowEditorConfigSettings((Document)null!));
            Assert.Equal("document", exception.ParamName);
        }

        [Fact, UseExportProvider]
        public void TestShowSettingsAsyncPassingNullDocumentPath()
        {
            using var workspace = CreateWorkspace();
            var (broker, _, _) = CreateBroker();
            var document = CreateDocument(workspace, filePath: null);
            _ = Assert.Throws<ArgumentException>(() => broker.ShowEditorConfigSettings(document));
        }

        [Fact]
        public async Task TestShowSettingsAsyncPassingNullDocumentAsync()
        {
            var (broker, _, _) = CreateBroker();
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(async () => await broker.ShowEditorConfigSettingsAsync((Document)null!, default));
            Assert.Equal("document", exception.ParamName);
        }

        [Fact, UseExportProvider]
        public async Task TestShowSettingsAsyncPassingNullDocumentPathAsync()
        {
            using var workspace = CreateWorkspace();
            var (broker, _, _) = CreateBroker();
            var document = CreateDocument(workspace, filePath: null);
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await broker.ShowEditorConfigSettingsAsync(document, default));
        }

        [Fact]
        public void TestShowSettingsAsyncPassingNullProject()
        {
            var (broker, _, _) = CreateBroker();
            var exception = Assert.Throws<ArgumentNullException>(() => broker.ShowEditorConfigSettings((Project)null!));
            Assert.Equal("project", exception.ParamName);
        }

        [Fact, UseExportProvider]
        public void TestShowSettingsAsyncPassingNullProjectPath()
        {
            using var workspace = CreateWorkspace();
            var (broker, _, _) = CreateBroker();
            var project = CreateProject(workspace, projectPath: null);
            _ = Assert.Throws<ArgumentException>(() => broker.ShowEditorConfigSettings(project));
        }

        [Fact]
        public async Task TestShowSettingsAsyncPassingNullProjectAsync()
        {
            var (broker, _, _) = CreateBroker();
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(async () => await broker.ShowEditorConfigSettingsAsync((Project)null!, default));
            Assert.Equal("project", exception.ParamName);
        }

        [Fact, UseExportProvider]
        public async Task TestShowSettingsAsyncPassingNullProjectPathAsync()
        {
            using var workspace = CreateWorkspace();
            var (broker, _, _) = CreateBroker();
            var project = CreateProject(workspace, projectPath: null);
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await broker.ShowEditorConfigSettingsAsync(project, default));
        }

        [Fact]
        public void TestShowSettingsAsyncPassingNullSolution()
        {
            var (broker, _, _) = CreateBroker();
            var exception = Assert.Throws<ArgumentNullException>(() => broker.ShowEditorConfigSettings((Solution)null!));
            Assert.Equal("solution", exception.ParamName);
        }

        [Fact, UseExportProvider]
        public void TestShowSettingsAsyncPassingNullSolutionPath()
        {
            using var workspace = CreateWorkspace();
            var (broker, _, _) = CreateBroker();
            var solution = CreateSolution(workspace, null);
            _ = Assert.Throws<ArgumentException>(() => broker.ShowEditorConfigSettings(solution));
        }

        [Fact]
        public async Task TestShowSettingsAsyncPassingNullSolutionAsync()
        {
            var (broker, _, _) = CreateBroker();
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(async () => await broker.ShowEditorConfigSettingsAsync((Solution)null!, default));
            Assert.Equal("solution", exception.ParamName);
        }

        [Fact, UseExportProvider]
        public async Task TestShowSettingsAsyncPassingNullSolutionPathAsync()
        {
            using var workspace = CreateWorkspace();
            var (broker, _, _) = CreateBroker();
            var solution = CreateSolution(workspace, null);
            _ = await Assert.ThrowsAsync<ArgumentException>(async () => await broker.ShowEditorConfigSettingsAsync(solution, default));
        }
    }
}
