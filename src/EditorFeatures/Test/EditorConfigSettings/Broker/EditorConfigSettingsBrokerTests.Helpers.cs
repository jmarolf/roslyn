// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CodeAnalysis.Editor.UnitTests.EditorConfigSettings.Mocks;
using Microsoft.CodeAnalysis.Remote.Testing;
using Microsoft.CodeAnalysis.Test.Utilities;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis.Editor.UnitTests.EditorConfigSettings
{
    public partial class EditorConfigSettingsBrokerTests
    {
        private static (IEditorConfigSettingsBroker, MockEditorConfigSettingsPresenterProvider, MockEditorConfigSettingsDataRepository) CreateBroker()
        {
            var presenterProvider = new MockEditorConfigSettingsPresenterProvider();
            var dataProvider = new MockEditorConfigSettingsDataRepositoryProvider();
            var broker = new EditorConfigSettingsBroker(presenterProvider, dataProvider);
            return (broker, presenterProvider, dataProvider.DataRepository);
        }

        private static Document CreateDocument(AdhocWorkspace workspace, string? filePath = "TestFilePath")
        {
            var project = CreateProject(workspace);
            var documentInfo = DocumentInfo.Create(
                    DocumentId.CreateNewId(project.Id),
                    "Empty",
                    loader: TextLoader.From(TextAndVersion.Create(SourceText.From(""), VersionStamp.Create(), filePath)),
                    filePath: filePath);
            var document = workspace.AddDocument(documentInfo);
            return document;
        }
        private static Project CreateProject(AdhocWorkspace workspace, string? projectPath = "TestProjectPath")
        {
            var name = "TestProject";
            var info = ProjectInfo.Create(ProjectId.CreateNewId(), VersionStamp.Create(), name, name, LanguageNames.CSharp, filePath: projectPath);
            return workspace.AddProject(info);
        }

        private static Solution CreateSolution(AdhocWorkspace workspace, string? solutionPath = "TestSolutionPath")
        {
            var solutionInfo = SolutionInfo.Create(SolutionId.CreateNewId(), VersionStamp.Create(), filePath: solutionPath);
            return workspace.CreateSolution(solutionInfo);
        }

        private static readonly TestComposition s_composition = FeaturesTestCompositions.Features.WithTestHostParts(TestHost.InProcess);
        private static AdhocWorkspace CreateWorkspace() => new AdhocWorkspace(s_composition.GetHostServices());
    }
}
