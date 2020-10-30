// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Microsoft.CodeAnalysis.Editor.UnitTests.EditorConfigSettings.Mocks
{
    internal class MockEditorConfigSettingsDataRepositoryProvider : IEditorConfigSettingsDataRepositoryProvider
    {
        internal MockEditorConfigSettingsDataRepository DataRepository { get; } = new MockEditorConfigSettingsDataRepository();

        public IEditorConfigSettingsDataRepository GetDataRepository(IEditorConfigSettingsBroker broker) => DataRepository;
    }
}
