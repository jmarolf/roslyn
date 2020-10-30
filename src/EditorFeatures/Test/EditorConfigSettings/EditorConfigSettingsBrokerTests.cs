// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Immutable;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Editor.UnitTests.EditorConfigSettings.Mocks;
using Microsoft.CodeAnalysis.Remote.Testing;
using Microsoft.CodeAnalysis.Test.Utilities;
using Microsoft.CodeAnalysis.Text;
using Xunit;

namespace Microsoft.CodeAnalysis.Editor.UnitTests.EditorConfigSettings
{
    public class EditorConfigSettingsBrokerTests
    {
        [Fact, UseExportProvider]
        public async Task TestShowSettingsAsyncNoResults()
        {
            // Create workspace services
            using var workspace = CreateWorkspace();
            var (broker, presenterProvider, _) = CreateBroker();
            var project = workspace.AddProject("TestProject", LanguageNames.CSharp);
            var filePath = "TestFilePath";
            var documentInfo = DocumentInfo.Create(
                    DocumentId.CreateNewId(project.Id),
                    "Empty",
                    loader: TextLoader.From(TextAndVersion.Create(SourceText.From(""), VersionStamp.Create(), filePath)),
                    filePath: filePath);
            var document = workspace.AddDocument(documentInfo);
            await broker.ShowEditorConfigSettingsAsync(document, default);
            // Wait for settings to populate

            // verify that settings were populated
            var presenter = await presenterProvider.GetPresenterAsync();
            await presenter.WaitForShowToBeCalled();

            Assert.Equal(0, presenter.ResultCount);
            Assert.False(presenter.HasSingleResult);
            Assert.Equal(1, presenter.ShowCalled);
        }

        [Fact, UseExportProvider]
        public async Task TestShowSettingsAsyncOneResults()
        {
            // Create workspace services
            using var workspace = CreateWorkspace();
            var (broker, presenterProvider, data) = CreateBroker();
            var project = workspace.AddProject("TestProject", LanguageNames.CSharp);
            var filePath = "TestFilePath";
            var documentInfo = DocumentInfo.Create(
                    DocumentId.CreateNewId(project.Id),
                    "Empty",
                    loader: TextLoader.From(TextAndVersion.Create(SourceText.From(""), VersionStamp.Create(), filePath)),
                    filePath: filePath);
            var document = workspace.AddDocument(documentInfo);
            await broker.ShowEditorConfigSettingsAsync(document, default);
            // Wait for settings to populate

            // verify that settings were populated
            var presenter = await presenterProvider.GetPresenterAsync();
            await presenter.WaitForShowToBeCalled();

            Assert.Equal(0, presenter.ResultCount);
            Assert.False(presenter.HasSingleResult);
            Assert.Equal(1, presenter.ShowCalled);

            data.AddRange(ImmutableArray.Create(new EditorConfigSetting()));
            await presenter.WaitForNotifyToBeCalled();

            Assert.Equal(1, presenter.ResultCount);
            Assert.True(presenter.HasSingleResult);
            Assert.Equal(1, presenter.NotifyOfUpdateCalled);

        }

        private static (IEditorConfigSettingsBroker, MockEditorConfigSettingsPresenterProvider, MockEditorConfigSettingsDataRepository) CreateBroker()
        {
            var presenterProvider = new MockEditorConfigSettingsPresenterProvider();
            var dataProvider = new MockEditorConfigSettingsDataRepositoryProvider();
            var broker = new EditorConfigSettingsBroker(presenterProvider, dataProvider);
            return (broker, presenterProvider, dataProvider.DataRepository);
        }

        private static readonly TestComposition s_composition = FeaturesTestCompositions.Features.WithTestHostParts(TestHost.InProcess);
        private static AdhocWorkspace CreateWorkspace() => new AdhocWorkspace(s_composition.GetHostServices());
    }
}
