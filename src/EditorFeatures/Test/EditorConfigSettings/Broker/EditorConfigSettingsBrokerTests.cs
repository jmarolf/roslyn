// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Test.Utilities;
using Xunit;

namespace Microsoft.CodeAnalysis.Editor.UnitTests.EditorConfigSettings
{
    public partial class EditorConfigSettingsBrokerTests
    {
        [Fact, UseExportProvider]
        public async Task TestShowSettingsDocumentNoResults()
        {
            using var workspace = CreateWorkspace();
            var (broker, presenterProvider, _) = CreateBroker();
            var document = CreateDocument(workspace);
            broker.ShowEditorConfigSettings(document);

            var presenter = await presenterProvider.GetPresenterAsync();
            await presenter.WaitForShowToBeCalled();

            Assert.Equal(0, presenter.ResultCount);
            Assert.False(presenter.HasSingleResult);
            Assert.Equal(1, presenter.ShowCalled);
        }

        [Fact, UseExportProvider]
        public async Task TestShowSettingsProjectNoResults()
        {
            using var workspace = CreateWorkspace();
            var (broker, presenterProvider, _) = CreateBroker();
            var project = CreateProject(workspace);
            broker.ShowEditorConfigSettings(project);

            var presenter = await presenterProvider.GetPresenterAsync();
            await presenter.WaitForShowToBeCalled();

            Assert.Equal(0, presenter.ResultCount);
            Assert.False(presenter.HasSingleResult);
            Assert.Equal(1, presenter.ShowCalled);
        }

        [Fact, UseExportProvider]
        public async Task TestShowSettingsSolutionNoResults()
        {
            using var workspace = CreateWorkspace();
            var (broker, presenterProvider, _) = CreateBroker();
            var solution = CreateSolution(workspace);
            broker.ShowEditorConfigSettings(solution);

            var presenter = await presenterProvider.GetPresenterAsync();
            await presenter.WaitForShowToBeCalled();

            Assert.Equal(0, presenter.ResultCount);
            Assert.False(presenter.HasSingleResult);
            Assert.Equal(1, presenter.ShowCalled);
        }

        [Fact, UseExportProvider]
        public async Task TestShowSettingsDocumentAsyncNoResults()
        {
            using var workspace = CreateWorkspace();
            var (broker, presenterProvider, _) = CreateBroker();
            var document = CreateDocument(workspace);
            await broker.ShowEditorConfigSettingsAsync(document, default);
            var presenter = await presenterProvider.GetPresenterAsync();
            Assert.Equal(0, presenter.ResultCount);
            Assert.False(presenter.HasSingleResult);
            Assert.Equal(1, presenter.ShowCalled);
        }

        [Fact, UseExportProvider]
        public async Task TestShowSettingsProjectAsyncNoResults()
        {
            using var workspace = CreateWorkspace();
            var (broker, presenterProvider, _) = CreateBroker();
            var project = CreateProject(workspace);
            await broker.ShowEditorConfigSettingsAsync(project, default);
            var presenter = await presenterProvider.GetPresenterAsync();
            Assert.Equal(0, presenter.ResultCount);
            Assert.False(presenter.HasSingleResult);
            Assert.Equal(1, presenter.ShowCalled);
        }

        [Fact, UseExportProvider]
        public async Task TestShowSettingsSolutionAsyncNoResults()
        {
            using var workspace = CreateWorkspace();
            var (broker, presenterProvider, _) = CreateBroker();
            var solution = CreateSolution(workspace);
            await broker.ShowEditorConfigSettingsAsync(solution, default);
            var presenter = await presenterProvider.GetPresenterAsync();
            Assert.Equal(0, presenter.ResultCount);
            Assert.False(presenter.HasSingleResult);
            Assert.Equal(1, presenter.ShowCalled);
        }

        [Fact, UseExportProvider]
        public async Task TestShowSettingsDocumentOneResult()
        {
            using var workspace = CreateWorkspace();
            var (broker, presenterProvider, data) = CreateBroker();
            var document = CreateDocument(workspace);
            broker.ShowEditorConfigSettings(document);

            var presenter = await presenterProvider.GetPresenterAsync();
            await presenter.WaitForShowToBeCalled();

            Assert.Equal(0, presenter.ResultCount);
            Assert.False(presenter.HasSingleResult);
            Assert.Equal(1, presenter.ShowCalled);

            data.AddRange(ImmutableArray.Create(new EditorConfigSetting(null, null, null, null)));
            await presenter.WaitForNotifyToBeCalled();

            Assert.Equal(1, presenter.ResultCount);
            Assert.True(presenter.HasSingleResult);
            Assert.Equal(1, presenter.NotifyOfUpdateCalled);
        }

        [Fact, UseExportProvider]
        public async Task TestShowSettingsProjectOneResult()
        {
            using var workspace = CreateWorkspace();
            var (broker, presenterProvider, data) = CreateBroker();
            var project = CreateProject(workspace);
            broker.ShowEditorConfigSettings(project);

            var presenter = await presenterProvider.GetPresenterAsync();
            await presenter.WaitForShowToBeCalled();

            Assert.Equal(0, presenter.ResultCount);
            Assert.False(presenter.HasSingleResult);
            Assert.Equal(1, presenter.ShowCalled);

            data.AddRange(ImmutableArray.Create(new EditorConfigSetting(null, null, null, null)));
            await presenter.WaitForNotifyToBeCalled();

            Assert.Equal(1, presenter.ResultCount);
            Assert.True(presenter.HasSingleResult);
            Assert.Equal(1, presenter.NotifyOfUpdateCalled);
        }

        [Fact, UseExportProvider]
        public async Task TestShowSettingsSolutionOneResult()
        {
            using var workspace = CreateWorkspace();
            var (broker, presenterProvider, data) = CreateBroker();
            var solution = CreateSolution(workspace);
            broker.ShowEditorConfigSettings(solution);

            var presenter = await presenterProvider.GetPresenterAsync();
            await presenter.WaitForShowToBeCalled();

            Assert.Equal(0, presenter.ResultCount);
            Assert.False(presenter.HasSingleResult);
            Assert.Equal(1, presenter.ShowCalled);

            data.AddRange(ImmutableArray.Create(new EditorConfigSetting(null, null, null, null)));
            await presenter.WaitForNotifyToBeCalled();

            Assert.Equal(1, presenter.ResultCount);
            Assert.True(presenter.HasSingleResult);
            Assert.Equal(1, presenter.NotifyOfUpdateCalled);
        }

        [Fact, UseExportProvider]
        public async Task TestShowSettingsDocumentAsyncOneResult()
        {
            using var workspace = CreateWorkspace();
            var (broker, presenterProvider, data) = CreateBroker();
            var document = CreateDocument(workspace);
            await broker.ShowEditorConfigSettingsAsync(document, default);
            var presenter = await presenterProvider.GetPresenterAsync();

            Assert.Equal(0, presenter.ResultCount);
            Assert.False(presenter.HasSingleResult);
            Assert.Equal(1, presenter.ShowCalled);

            data.AddRange(ImmutableArray.Create(new EditorConfigSetting(null, null, null, null)));
            await presenter.WaitForNotifyToBeCalled();

            Assert.Equal(1, presenter.ResultCount);
            Assert.True(presenter.HasSingleResult);
            Assert.Equal(1, presenter.NotifyOfUpdateCalled);
        }

        [Fact, UseExportProvider]
        public async Task TestShowSettingsProjectAsyncOneResult()
        {
            using var workspace = CreateWorkspace();
            var (broker, presenterProvider, data) = CreateBroker();
            var project = CreateProject(workspace);
            await broker.ShowEditorConfigSettingsAsync(project, default);
            var presenter = await presenterProvider.GetPresenterAsync();

            Assert.Equal(0, presenter.ResultCount);
            Assert.False(presenter.HasSingleResult);
            Assert.Equal(1, presenter.ShowCalled);

            data.AddRange(ImmutableArray.Create(new EditorConfigSetting(null, null, null, null)));
            await presenter.WaitForNotifyToBeCalled();

            Assert.Equal(1, presenter.ResultCount);
            Assert.True(presenter.HasSingleResult);
            Assert.Equal(1, presenter.NotifyOfUpdateCalled);
        }

        [Fact, UseExportProvider]
        public async Task TestShowSettingsSolutionAsyncOneResult()
        {
            using var workspace = CreateWorkspace();
            var (broker, presenterProvider, data) = CreateBroker();
            var solution = CreateSolution(workspace);
            await broker.ShowEditorConfigSettingsAsync(solution, default);
            var presenter = await presenterProvider.GetPresenterAsync();

            Assert.Equal(0, presenter.ResultCount);
            Assert.False(presenter.HasSingleResult);
            Assert.Equal(1, presenter.ShowCalled);

            data.AddRange(ImmutableArray.Create(new EditorConfigSetting(null, null, null, null)));
            await presenter.WaitForNotifyToBeCalled();

            Assert.Equal(1, presenter.ResultCount);
            Assert.True(presenter.HasSingleResult);
            Assert.Equal(1, presenter.NotifyOfUpdateCalled);
        }
    }
}
