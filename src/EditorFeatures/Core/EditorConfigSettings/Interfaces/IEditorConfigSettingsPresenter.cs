// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace Microsoft.CodeAnalysis.Editor
{
    internal interface IEditorConfigSettingsPresenter
    {
        event EventHandler<EventArgs>? SearchDismissed;

        Task NotifyOfUpdateAsync(ImmutableArray<EditorConfigSetting> results, IEnumerable<string>? additionalColumns);
        Task ShowAsync();
    }
}
