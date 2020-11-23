// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.Shell.TableControl;

namespace Microsoft.VisualStudio.LanguageServices.EditorConfigSettings
{
    internal partial class EditorConfigSettingsTableProvider
    {
        /// <summary>
        /// Used for columns that will be grouped on. Displays an image and text string.
        /// </summary>
        internal class ImageEntryBucket : StringEntryBucket
        {
            public readonly ImageMoniker ImageMoniker;

            public ImageEntryBucket(ImageMoniker imageMoniker, string name, object tooltip = null, StringComparer comparer = null, bool expandedByDefault = true)
                : base(name, tooltip, comparer, expandedByDefault)
            {
                ImageMoniker = imageMoniker;
            }

            public override bool TryCreateColumnContent(out FrameworkElement content)
            {
                content = CreateGridElement(ImageMoniker, Name, isBold: true);
                return true;
            }

            /// <summary>
            /// Creates an element to display within the TableControl comprised of both an image and text string.
            /// </summary>
            private static FrameworkElement CreateGridElement(ImageMoniker imageMoniker, string text, bool isBold)
            {
                var stackPanel = new StackPanel();
                stackPanel.Orientation = Orientation.Horizontal;
                stackPanel.HorizontalAlignment = HorizontalAlignment.Stretch;

                var block = new TextBlock();
                block.VerticalAlignment = VerticalAlignment.Center;
                block.Inlines.Add(new Run(text)
                {
                    FontWeight = isBold ? FontWeights.Bold : FontWeights.Normal
                });


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
}
