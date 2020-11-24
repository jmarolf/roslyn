// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Microsoft.CodeAnalysis.CodeStyle;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Options;

namespace Microsoft.CodeAnalysis.Editor
{
    internal class EditorConfigSetting
    {
        public EditorConfigSetting(string id, string title, string description, string category, bool enabled, DiagnosticSeverity severity)
        {
            Id = id;
            Title = title;
            Description = description;
            Category = category;
            Severity = severity;
            IsEnabled = enabled;
        }

        public string Id { get; }
        public string Title { get; }
        public string Description { get; }
        public string Category { get; }
        public DiagnosticSeverity Severity { get; }
        public bool IsEnabled { get; }

        public static EditorConfigSetting Create(DiagnosticDescriptor descriptor, ReportDiagnostic effectiveSeverity)
        {
            DiagnosticSeverity severity = default;
            if (effectiveSeverity == ReportDiagnostic.Default)
            {
                severity = descriptor.DefaultSeverity;
            }
            else if (effectiveSeverity.ToDiagnosticSeverity() is DiagnosticSeverity severity1)
            {
                severity = severity1;
            }

            var enabled = effectiveSeverity == ReportDiagnostic.Suppress ? false : true;

            return new EditorConfigSetting
            (
                id: descriptor.Id,
                title: descriptor.Title.ToString(),
                description: descriptor.Description.ToString(),
                category: descriptor.Category,
                severity: severity,
                enabled: enabled
            );
        }

        public static EditorConfigSetting Create(IOption2 option2, OptionSet optionSet)
        {
            var option = option2.OptionDefinition;
            // TODO(jmarolf): refactor this into two classes VB and C# specific
            var currentValueObject = option2.IsPerLanguage ? optionSet.GetOption(new OptionKey2(option2, LanguageNames.CSharp)) : optionSet.GetOption(new OptionKey2(option2));
            if (currentValueObject is null)
            {
                throw new ArgumentException(nameof(option2));
            }


            if (currentValueObject is ICodeStyleOption codeStyleOption)
            {
                return new EditorConfigSetting
                (
                    id: "Built In",
                    title: option.Name,
                    description: option.Group.Description,
                    category: option.Feature,
                    enabled: codeStyleOption.Value == null ? false : true,
                    severity: codeStyleOption.Notification.Severity.ToDiagnosticSeverity() ?? default
                );
            }

            return new EditorConfigSetting
            (
                id: "Built In",
                title: option.Name,
                description: option.Group.Description,
                category: option.Feature,
                enabled: option.DefaultValue == null ? false : true,
                severity:  default
            );
        }
    }
}
