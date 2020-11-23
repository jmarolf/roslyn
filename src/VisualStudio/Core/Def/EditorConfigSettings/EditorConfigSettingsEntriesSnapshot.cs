// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Immutable;
using System.Windows;
using System.Windows.Controls;
using Microsoft.VisualStudio.LanguageServices.EditorConfigSettings;
using Microsoft.VisualStudio.Shell.TableControl;

namespace Microsoft.CodeAnalysis.Editor.EditorConfigSettings
{
    internal class EditorConfigSettingsEntriesSnapshot : WpfTableEntriesSnapshotBase
    {
        private EditorConfigSettingsSnapshotFactory _editorConfigSettingsSnapshotFactory;

        public EditorConfigSettingsEntriesSnapshot(ImmutableArray<EditorConfigSetting> data,
                                                   EditorConfigSettingsSnapshotFactory editorConfigSettingsSnapshotFactory,
                                                   int currentVersionNumber)
        {
            Results = data;
            VersionNumber = currentVersionNumber;
        }

        public ImmutableArray<EditorConfigSetting> Results { get; }

        public override int VersionNumber { get; }
        public override int Count => Results.Length;

        public override bool TryGetValue(int index, string keyName, out object content)
        {
            EditorConfigSetting result = null;
            try
            {
                if (index < 0 || index > Results.Length)
                {
                    throw new InvalidOperationException("Investigate why Table Control requests data for invalid indices");
                    content = null;
                    return false;
                }

                result = Results[index];

                if (result == null)
                {
                    throw new InvalidOperationException("Investigate why Platform stores a null result");
                    content = null;
                    return false;
                }
            }
            catch (Exception ex)
            {
                // TODO(jmarolf): Handle Exception
                content = null;
                return false;
            }

            return TryGetValue(result, keyName, out content);
        }

        internal bool TryGetValue(EditorConfigSetting result, string keyName, out object? content)
        {
            content = keyName switch
            {
                EditorConfigSettingsColumnDefinitions.TitleName => result.Title,
                EditorConfigSettingsColumnDefinitions.DescriptionName => result.Description,
                EditorConfigSettingsColumnDefinitions.CategoryName => "Default",
                EditorConfigSettingsColumnDefinitions.SeverityName => Enum.GetName(typeof(ReportDiagnostic), result.Severity), // TODO(jmarolf): convert to icon
                _ => "Default",
            };

            return content is null ? false : true;
        }
    }
}
