﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace Microsoft.CodeAnalysis.Editor.UnitTests.EditorConfigSettings
{
    internal class MockEditorConfigSettingsPresenter : IEditorConfigSettingsPresenter
    {
        public MockEditorConfigSettingsPresenter(IEditorConfigSettingsDataRepository? dataRepository = null)
        {
            ShowCalled = 0;
            NotifyOfUpdateCalled = 0;
            ResultCount = 0;
            HasSingleResult = false;
            DataRepository = dataRepository;
        }

        /// <summary>
        /// Never used by the mock class
        /// </summary>
        public event EventHandler<EventArgs>? SearchDismissed;

        internal int ShowCalled { get; private set; }
        internal IEditorConfigSettingsDataRepository? DataRepository { get; private set; }
        internal int NotifyOfUpdateCalled { get; private set; }
        internal int ResultCount { get; private set; }
        internal HashSet<string> AdditionalColumns { get; private set; } = new HashSet<string>();
        internal bool HasSingleResult { get; private set; }

        public Task NotifyOfUpdateAsync(ImmutableArray<EditorConfigSetting> singleResult, IEnumerable<string>? additionalColumns)
        {
            NotifyOfUpdateCalled++;
            HasSingleResult = singleResult != default;

            if (additionalColumns != null)
            {
                foreach (var column in additionalColumns)
                {
                    AdditionalColumns.Add(column);
                }
            }

            var results = DataRepository?.GetCurrentDataSnapshot();
            ResultCount = results?.Length ?? 0;

            return Task.CompletedTask;
        }

        public Task ShowAsync()
        {
            ShowCalled++;
            return Task.CompletedTask;
        }
    }
}
