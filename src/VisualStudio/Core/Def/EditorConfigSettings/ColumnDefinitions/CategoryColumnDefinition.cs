// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.ComponentModel.Composition;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.VisualStudio.Shell.TableControl;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.LanguageServices.EditorConfigSettings
{
    [Export(typeof(ITableColumnDefinition))]
    [Name(EditorConfigSettingsColumnDefinitions.CategoryName)]
    internal class CategoryColumnDefinition : TableColumnDefinitionBase
    {
        [ImportingConstructor]
        [Obsolete(MefConstruction.ImportingConstructorMessage, error: true)]
        public CategoryColumnDefinition()
        {
        }

        public override string Name => EditorConfigSettingsColumnDefinitions.CategoryName;
        public override string DisplayName => "Category"; //TODO: Localize
        public override bool IsFilterable => true;
        public override double MinWidth => 80;

    }
}
