// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.VisualStudio.Shell.TableControl;
using Microsoft.VisualStudio.Shell.TableManager;
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
        public override double MinWidth => 80;
        public override bool DefaultVisible => false;
        public override TextWrapping TextWrapping => TextWrapping.NoWrap;

        private static string? GetCategoryName(ITableEntryHandle entry)
            => entry.TryGetValue(EditorConfigSettingsColumnDefinitions.CategoryName, out string? categoryName)
                ? categoryName
                : null;

        public override IEntryBucket? CreateBucketForEntry(ITableEntryHandle entry)
        {
            var categoryName = GetCategoryName(entry);
            return categoryName is not null ? new CategoryBucket(categoryName) : null;
        }

        private class CategoryBucket : StringEntryBucket, IEntryBucket2, IEntryBucket, IComparable<IEntryBucket>
        {
            public CategoryBucket(string categoryName)
                : base(categoryName, comparer: StringComparer.OrdinalIgnoreCase)
            {
                ShowCount = true;
            }

            /// <summary>
            /// We're not interested in handling events.
            /// </summary>
            public string? Identifier => null;

            /// <summary>
            /// We're not interested in handling events.
            /// </summary>
            public string? SourceTypeIdentifier => null;

            public bool ShowCount { get; set; }

            public IEntryBucket? Merge(IEntryBucket child) => null;

            public IEnumerable<IEntryBucket>? Split() => null;

            public bool TryGetValue(string key, out object? content)
            {
                content = null;
                return false;
            }
        }
    }
}
