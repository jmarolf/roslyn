// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Microsoft.CodeAnalysis.CodeStyle;
using Microsoft.CodeAnalysis.Options;

namespace Microsoft.CodeAnalysis.Editor
{
    internal class EditorConfigSetting
    {
        public EditorConfigSetting(string title, string description, string currentValue, string[] validValues, ReportDiagnostic? severity = null)
        {
            Title = title;
            Description = description;
            CurrentValue = currentValue;
            ValidValues = validValues;
            Severity = severity;
        }

        public string Title { get; }
        public string Description { get; }
        public string CurrentValue { get; }
        public string[] ValidValues { get; }
        public ReportDiagnostic? Severity { get; }

        public static EditorConfigSetting Create(DiagnosticDescriptor descriptor, ReportDiagnostic effectiveSeverity)
        {
            return new EditorConfigSetting
            (
                title: descriptor.Title.ToString(),
                description: descriptor.Description.ToString(),
                currentValue: effectiveSeverity == ReportDiagnostic.Suppress ? "Diabled" : "Enabled",
                validValues: new[] { "disabled", "enabled" },
                severity: effectiveSeverity
            );
        }

        public static EditorConfigSetting Create(IOption2 option2, OptionSet optionSet)
        {
            var option = option2.OptionDefinition;
            var validValues = option.GetValidValues();
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
                    title: option.Name,
                    description: option.Group.Description,
                    currentValue: codeStyleOption.Value?.ToString() ?? "None",
                    validValues: validValues,
                    severity: codeStyleOption.Notification.Severity
                );
            }

            return new EditorConfigSetting
            (
                title: option.Name,
                description: option.Group.Description,
                currentValue: currentValueObject?.ToString() ?? "None",
                validValues: validValues
            );
        }
    }
}
