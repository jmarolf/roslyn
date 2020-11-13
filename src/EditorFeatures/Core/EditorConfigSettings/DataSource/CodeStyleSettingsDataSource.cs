// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CodeStyle;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Options;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.Editor
{
    internal class CodeStyleSettingsDataSource : EditorConfigSettingsDataSourceBase
    {
        public CodeStyleSettingsDataSource(OptionSet optionSet)
        {
            var options = EnumerateOptions();
            var builder = ArrayBuilder<EditorConfigSetting>.GetInstance();
            foreach (var option in options)
            {
                foreach (var storagelocation in option.StorageLocations)
                {
                    if (storagelocation is IEditorConfigStorageLocation2)
                    {
                        builder.Add(EditorConfigSetting.Create(option, optionSet));
                        break;
                    }
                }
            }

            AddRange(builder.ToImmutableAndFree());
        }

        private static ImmutableArray<IOption2> EnumerateOptions()
        {
            var builder = ArrayBuilder<IOption2>.GetInstance();
            builder.AddRange(FormattingOptions2.AllOptions);
            builder.AddRange(GenerationOptions.AllOptions);
            builder.AddRange(CodeStyleOptions2.AllOptions);
            return builder.ToImmutableAndFree();
        }
    }
}
