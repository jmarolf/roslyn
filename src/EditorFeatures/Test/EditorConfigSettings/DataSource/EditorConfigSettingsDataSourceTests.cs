// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Test.Utilities;
using Xunit;

namespace Microsoft.CodeAnalysis.Editor.UnitTests.EditorConfigSettings.DataSource
{
    [UseExportProvider]
    public partial class EditorConfigSettingsDataSourceTests
    {
        [Fact]
        public async Task TestDocuementDataSourceCallPresenterAsync()
        {
            var dataSource = CreateDocuementDataSource();
            var sync = new SemaphoreSlim(0);
            dataSource.RegisterPresenter(new TestPresenter(results =>
            {
                foreach (var result in results)
                {
                    Assert.Equal(ReportDiagnostic.Error, result.EffectiveSeverity);
                }
                Assert.Equal(3, results.Length);
                sync.Release();
            }));
            await sync.WaitAsync();
        }

        [Fact]
        public async Task TestDocuementDataSourceCallPresenterSettingsRespectedAsync()
        {
            var editorConfig = @"
[*.cs]
dotnet_diagnostic.syntax.severity = none
dotnet_diagnostic.semantic.severity = none
dotnet_diagnostic.compilation.severity = none
";
            var dataSource = CreateDocuementDataSource(editorConfig);
            var sync = new SemaphoreSlim(0);
            dataSource.RegisterPresenter(new TestPresenter(results =>
            {
                foreach (var result in results)
                {
                    Assert.Equal(ReportDiagnostic.Suppress, result.EffectiveSeverity);
                }
                Assert.Equal(3, results.Length);
                sync.Release();
            }));
            await sync.WaitAsync();
        }

        [Fact]
        public async Task TestDocuementDataSourceCallPresenterCancelledAsync()
        {
            var dataSource = CreateDocuementDataSource();
            var sync = new SemaphoreSlim(0);
            var presenter = new TestPresenter(results => { });
            dataSource.RegisterPresenter(presenter);
            await sync.WaitAsync();
        }
    }
}
