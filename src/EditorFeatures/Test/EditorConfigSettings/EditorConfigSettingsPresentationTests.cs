using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.CodeAnalysis.Editor.UnitTests.EditorConfigSettings
{
    public class EditorConfigSettingsPresentationTests
    {
        [Fact]
        public async Task TestShowSettingsAsync()
        {
            // Create workspace services
            using var workspace = CreateWorkspace();
            var (broker, presenter) = CreateBroker();
            var document = workspace.AddDocument(documentInfo: null);
            await broker.ShowEditorConfigSettingsAsync(document, default);
            // Wait for settings to populate

            // verify that settings were populated
            Assert.Equal(1, presenter.ResultCount);
            Assert.True(presenter.HasSingleResult);
            Assert.Equal(1, presenter.ShowCalled);
        }

        private (IEditorConfigSettingsBroker, MockEditorConfigSettingsPresenter) CreateBroker()
        {
            throw new NotImplementedException();
        }

        private AdhocWorkspace CreateWorkspace()
        {
            throw new NotImplementedException();
        }
    }
}
