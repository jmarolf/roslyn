// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Immutable;
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
                AssertCorrectSeverity(results, 3, ReportDiagnostic.Error);
                sync.Release();
            }));
            await sync.WaitAsync();
            var data = dataSource.GetCurrentDataSnapshot();
            AssertCorrectSeverity(data, 3, ReportDiagnostic.Error);
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
                AssertCorrectSeverity(results, 3, ReportDiagnostic.Suppress);
                sync.Release();
            }));
            await sync.WaitAsync();
            var data = dataSource.GetCurrentDataSnapshot();
            AssertCorrectSeverity(data, 3, ReportDiagnostic.Suppress);
        }

        [Fact]
        public void TestDocuementDataSourceCallPresenterCancelledAsync()
        {
            var dataSource = CreateDocuementDataSource();
            var presenter = new TestPresenter();
            dataSource.RegisterPresenter(presenter);
            presenter.Cancel();
            Assert.Throws<InvalidOperationException>(() =>
            {
                _ = dataSource.GetCurrentDataSnapshot();
            });
        }

        [Fact]
        public void TestGetCodeStyleOptions()
        {
            var codeStyleDataSource = CreateCodeStyleDataSource();
            var results = codeStyleDataSource.GetCurrentDataSnapshot();
            Assert.Equal(37, results.Length);
        }

        private static void AssertCorrectSeverity(ImmutableArray<EditorConfigSetting> results, int expectedLength, ReportDiagnostic expectedReportDiagnostic)
        {
            foreach (var result in results)
            {
                Assert.Equal(expectedReportDiagnostic, result.Severity);
            }

            Assert.Equal(expectedLength, results.Length);
        }
    }
}
