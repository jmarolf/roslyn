// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.VisualStudio.Shell.TableControl;
using Microsoft.VisualStudio.Shell.TableManager;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.LanguageServices.EditorConfigSettings
{
    [Export(typeof(ITableColumnDefinition))]
    [Name(EditorConfigSettingsColumnDefinitions.EnabledName)]
    internal class EnabledColumnDefinition : TableColumnDefinitionBase
    {
        [ImportingConstructor]
        [Obsolete(MefConstruction.ImportingConstructorMessage, error: true)]
        public EnabledColumnDefinition()
        {
        }

        public override string Name => EditorConfigSettingsColumnDefinitions.EnabledName;
        public override string DisplayName => "Enabled"; //TODO: Localize
        public override bool IsFilterable => true;
        public override double MinWidth => 50;

        public override bool TryCreateColumnContent(ITableEntryHandle entry, bool singleColumnView, out FrameworkElement content)
        {
            var checkBox = new CheckBox();
            if (entry.TryGetValue(EditorConfigSettingsColumnDefinitions.EnabledName, out bool enabled))
            {
                checkBox.IsChecked = enabled;
            }

            content = checkBox;
            return true;
        }
    }
}
