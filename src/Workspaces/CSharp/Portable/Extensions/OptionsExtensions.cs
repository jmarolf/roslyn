// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis.AddImports;
using Microsoft.CodeAnalysis.CodeStyle;
using Microsoft.CodeAnalysis.CSharp.Formatting;
using Microsoft.CodeAnalysis.Options;

namespace Microsoft.CodeAnalysis.CSharp.Extensions
{
    internal static class OptionsExtensions
    {
        public static string[] GetValidValues(this OptionDefinition option)
        {
            return option.DefaultValue.GetValidValues();
        }

        private static string[] GetValidValues<T>(this T value)
            => value switch
            {
                AccessibilityModifiersRequired => Enum.GetNames(typeof(AccessibilityModifiersRequired)),
                AddImportPlacement => Enum.GetNames(typeof(AddImportPlacement)),
                BinaryOperatorSpacingOptions => Enum.GetNames(typeof(BinaryOperatorSpacingOptions)),
                bool => new[] { "True", "False" },
                ExpressionBodyPreference => Enum.GetNames(typeof(ExpressionBodyPreference)),
                LabelPositionOptions => Enum.GetNames(typeof(LabelPositionOptions)),
                NotificationOption2 => Enum.GetNames(typeof(ReportDiagnostic)),
                PreferBracesPreference => Enum.GetNames(typeof(PreferBracesPreference)),
                UnusedParametersPreference => Enum.GetNames(typeof(UnusedParametersPreference)),
                UnusedValuePreference => Enum.GetNames(typeof(UnusedValuePreference)),
                ICodeStyleOption o => o.Value.GetValidValues(),
                string s => new[] { s },
                _ => Array.Empty<string>()
            };
    }
}
