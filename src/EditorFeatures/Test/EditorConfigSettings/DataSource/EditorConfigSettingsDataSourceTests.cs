// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Editor.UnitTests.EditorConfigSettings.Mocks;
using Xunit;

namespace Microsoft.CodeAnalysis.Editor.UnitTests.EditorConfigSettings.DataSource
{
    public partial class EditorConfigSettingsDataSourceTests
    {
        [Fact]
        public async Task Test()
        {
            var dataSource = CreateDataSource();
            var foundData = dataSource.GetCurrentDataSnapshot();
        }

        private static EditorConfigSettingsDataSource CreateDataSource()
        {
            return new EditorConfigSettingsDataSource(null);
        }
    }
}
