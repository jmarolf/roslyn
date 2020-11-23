// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Immutable;

namespace Microsoft.VisualStudio.LanguageServices.EditorConfigSettings
{
    internal static class EditorConfigSettingsColumnDefinitions
    {
        private const string Prefix = "editorconfig.";

        public const string TitleName = Prefix + EditorConfigSettingsTableKeyNames.TitleName;
        public const string DescriptionName = Prefix + EditorConfigSettingsTableKeyNames.DescriptionName;
        public const string CategoryName = Prefix + EditorConfigSettingsTableKeyNames.CategoryName;
        public const string SeverityName = Prefix + EditorConfigSettingsTableKeyNames.SeverityName;

        public static readonly ImmutableArray<string> ColumnNames = ImmutableArray.Create(
            TitleName,
            DescriptionName,
            CategoryName,
            SeverityName);
    }
}
