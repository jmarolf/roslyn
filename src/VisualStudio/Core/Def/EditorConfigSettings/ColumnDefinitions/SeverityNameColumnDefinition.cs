// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.Shell.TableControl;
using Microsoft.VisualStudio.Shell.TableManager;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.LanguageServices.EditorConfigSettings
{
    [Export(typeof(ITableColumnDefinition))]
    [Name(EditorConfigSettingsColumnDefinitions.SeverityName)]
    internal class SeverityNameColumnDefinition : TableColumnDefinitionBase
    {
        [ImportingConstructor]
        [Obsolete(MefConstruction.ImportingConstructorMessage, error: true)]
        public SeverityNameColumnDefinition()
        {
        }

        public override string Name => EditorConfigSettingsColumnDefinitions.SeverityName;
        public override string DisplayName => "Severity"; //TODO: Localize
        public override bool IsFilterable => true;
        public override double MinWidth => 50;

        public override bool TryCreateColumnContent(ITableEntryHandle entry, bool singleColumnView, out FrameworkElement content)
        {
            var hidden = CreateGridElement(KnownMonikers.None, "Disabled");
            var suggestion = CreateGridElement(KnownMonikers.StatusInformation, "Suggestion");
            var warning = CreateGridElement(KnownMonikers.StatusWarning, "Warning");
            var error = CreateGridElement(KnownMonikers.StatusError, "Error");
            var comboBox = new ComboBox()
            {
                ItemsSource = new[]
                {
                    hidden,
                    suggestion,
                    warning,
                    error
                }
            };

            if (entry.TryGetValue(EditorConfigSettingsColumnDefinitions.SeverityName, out DiagnosticSeverity severity))
            {
                switch (severity)
                {
                    case DiagnosticSeverity.Hidden:
                        comboBox.SelectedItem = hidden;
                        break;
                    case DiagnosticSeverity.Info:
                        comboBox.SelectedItem = suggestion;
                        break;
                    case DiagnosticSeverity.Warning:
                        comboBox.SelectedItem = warning;
                        break;
                    case DiagnosticSeverity.Error:
                        comboBox.SelectedItem = error;
                        break;
                    default:
                        break;
                }
            }

            if (entry.TryGetValue(EditorConfigSettingsColumnDefinitions.EnabledName, out bool enabled))
            {
                comboBox.IsEnabled = enabled;
            }

            content = comboBox;
            return true;
        }

        private static FrameworkElement CreateGridElement(ImageMoniker imageMoniker, string text)
        {
            var stackPanel = new StackPanel();
            stackPanel.Orientation = Orientation.Horizontal;
            stackPanel.HorizontalAlignment = HorizontalAlignment.Stretch;

            var block = new TextBlock();
            block.VerticalAlignment = VerticalAlignment.Center;
            block.Text = text;

            if (!imageMoniker.IsNullImage())
            {
                // If we have an image and text, then create some space between them.
                block.Margin = new Thickness(5.0, 0.0, 0.0, 0.0);

                var image = new CrispImage();
                image.VerticalAlignment = VerticalAlignment.Center;
                image.Moniker = imageMoniker;
                image.Width = image.Height = 16.0;

                stackPanel.Children.Add(image);
            }

            // Always add the textblock last so it can follow the image.
            stackPanel.Children.Add(block);

            return stackPanel;
        }
    }
}
