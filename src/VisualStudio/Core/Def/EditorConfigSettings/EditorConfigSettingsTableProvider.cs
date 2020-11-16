// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editor.EditorConfigSettings;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.Internal.VisualStudio.Shell.TableControl;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.Shell.TableControl;
using Microsoft.VisualStudio.Shell.TableManager;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.LanguageServices.EditorConfigSettings
{
    [Export(typeof(EditorConfigSettingsTableProvider))]
    internal class EditorConfigSettingsTableProvider
    {
        private readonly IWpfTableControlProvider _tableControlProvider;
        private readonly EditorConfigSettingsPresenter _dataSource;
        private readonly ITableManager _tableManager;

        [ImportingConstructor]
        [Obsolete(MefConstruction.ImportingConstructorMessage, error: true)]
        public EditorConfigSettingsTableProvider(
            ITableManagerProvider tableMangerProvider,
            IWpfTableControlProvider tableControlProvider)
        {
            _tableControlProvider = tableControlProvider;

            _tableManager = tableMangerProvider.GetTableManager(nameof(EditorConfigSettingsPresenter));
            _tableControlProvider = tableControlProvider;
        }

        internal static class EditorConfigSettingsColumnDefinitions
        {
            private const string Prefix = "editorconfig.";

            public const string SolutionName = Prefix + EditorConfigSettingsTableKeyNames.SolutionName;
            public const string ProjectName = Prefix + EditorConfigSettingsTableKeyNames.ProjectName;
            public const string ReferenceType = Prefix + EditorConfigSettingsTableKeyNames.ReferenceType;
            public const string ReferenceName = Prefix + EditorConfigSettingsTableKeyNames.ReferenceName;
            public const string UpdateAction = Prefix + EditorConfigSettingsTableKeyNames.UpdateAction;

            public static readonly ImmutableArray<string> ColumnNames = ImmutableArray.Create(
                SolutionName,
                ProjectName,
                ReferenceType,
                ReferenceName,
                UpdateAction);
        }

        internal static class EditorConfigSettingsTableKeyNames
        {
            public const string SolutionName = "solutionname";
            public const string ProjectName = "projectname";
            public const string Language = "language";
            public const string ReferenceType = "referencetype";
            public const string ReferenceName = "referencename";
            public const string UpdateAction = "updateaction";
        }

        [Export(typeof(ITableColumnDefinition))]
        [Name(EditorConfigSettingsColumnDefinitions.SolutionName)]
        internal class SolutionNameColumnDefinition : TableColumnDefinitionBase
        {
            [ImportingConstructor]
            [Obsolete(MefConstruction.ImportingConstructorMessage, error: true)]
            public SolutionNameColumnDefinition()
            {
            }

            public override string Name => EditorConfigSettingsColumnDefinitions.SolutionName;

            public override IEntryBucket CreateBucketForEntry(ITableEntryHandle entry)
            {
                return entry.TryGetValue(EditorConfigSettingsTableKeyNames.SolutionName, out string name)
                    ? new ImageEntryBucket(KnownMonikers.Solution, name)
                    : null;
            }
        }

        [Export(typeof(ITableColumnDefinition))]
        [Name(EditorConfigSettingsColumnDefinitions.ProjectName)]
        internal class ProjectNameColumnDefinition : TableColumnDefinitionBase
        {
            [ImportingConstructor]
            [Obsolete(MefConstruction.ImportingConstructorMessage, error: true)]
            public ProjectNameColumnDefinition()
            {
            }

            public override string Name => EditorConfigSettingsColumnDefinitions.ProjectName;

            public override IEntryBucket CreateBucketForEntry(ITableEntryHandle entry)
            {
                return entry.TryGetValue(EditorConfigSettingsColumnDefinitions.ProjectName, out string name)
                    ? new ImageEntryBucket(GetImageMoniker(entry), name)
                    : null;
            }

            private ImageMoniker GetImageMoniker(ITableEntryHandle entry)
            {
                return entry.TryGetValue(EditorConfigSettingsTableKeyNames.Language, out string languageName) && languageName == LanguageNames.VisualBasic
                    ? KnownMonikers.VBProjectNode
                    : KnownMonikers.CSProjectNode;
            }
        }

        [Export(typeof(ITableColumnDefinition))]
        [Name(EditorConfigSettingsColumnDefinitions.ReferenceType)]
        internal class ReferenceTypeColumnDefinition : TableColumnDefinitionBase
        {
            [ImportingConstructor]
            [Obsolete(MefConstruction.ImportingConstructorMessage, error: true)]
            public ReferenceTypeColumnDefinition()
            {
            }

            public override string Name => EditorConfigSettingsColumnDefinitions.ReferenceType;

            public override IEntryBucket CreateBucketForEntry(ITableEntryHandle entry)
            {
                return entry.TryGetValue<ReferenceType>(EditorConfigSettingsTableKeyNames.ReferenceType, out var referenceType)
                    ? new ImageEntryBucket(GetImageMoniker(entry), referenceType.ToString())
                    : null;
            }

            private ImageMoniker GetImageMoniker(ITableEntryHandle entry)
            {
                return entry.TryGetValue(EditorConfigSettingsTableKeyNames.ReferenceType, out ReferenceType referenceType)
                    ? referenceType switch
                    {
                        ReferenceType.Package => KnownMonikers.PackageReference,
                        ReferenceType.Project => KnownMonikers.Library,
                        ReferenceType.Assembly => KnownMonikers.Reference,
                        _ => default
                    }
                    : default;
            }
        }

        public enum ReferenceType
        {
            Package,
            Project,
            Assembly
        }

        [Export(typeof(ITableColumnDefinition))]
        [Name(EditorConfigSettingsColumnDefinitions.ReferenceName)]
        internal class ReferenceNameColumnDefinition : TableColumnDefinitionBase
        {
            [ImportingConstructor]
            [Obsolete(MefConstruction.ImportingConstructorMessage, error: true)]
            public ReferenceNameColumnDefinition()
            {
            }

            public override string Name => EditorConfigSettingsColumnDefinitions.ReferenceName;
            public override string DisplayName => "Reference"; //TODO: Localize
            public override bool IsFilterable => false;
            public override double MinWidth => 200;

            public override bool TryCreateColumnContent(ITableEntryHandle entry, bool singleColumnView, out FrameworkElement content)
            {
                content = CreateGridElement(GetImageMoniker(entry), GetText(entry), isBold: false);
                return true;
            }

            private ImageMoniker GetImageMoniker(ITableEntryHandle entry)
            {
                return entry.TryGetValue(EditorConfigSettingsTableKeyNames.ReferenceType, out ReferenceType referenceType)
                    ? referenceType switch
                    {
                        ReferenceType.Package => KnownMonikers.PackageReference,
                        ReferenceType.Project => KnownMonikers.Library,
                        ReferenceType.Assembly => KnownMonikers.Reference,
                        _ => default
                    }
                    : default;
            }

            private string GetText(ITableEntryHandle entry)
            {
                return entry.TryGetValue(EditorConfigSettingsTableKeyNames.ReferenceName, out string text)
                    ? text
                    : string.Empty;
            }
        }

        [Export(typeof(ITableColumnDefinition))]
        [Name(EditorConfigSettingsColumnDefinitions.UpdateAction)]
        internal class UpdateActionColumnDefinition : TableColumnDefinitionBase
        {
            [ImportingConstructor]
            [Obsolete(MefConstruction.ImportingConstructorMessage, error: true)]
            public UpdateActionColumnDefinition()
            {
            }

            public override string Name => EditorConfigSettingsColumnDefinitions.UpdateAction;
            public override string DisplayName => "Action"; //TODO: Localize
            public override bool IsFilterable => false;
            public override double MinWidth => 100;

            public override bool TryCreateColumnContent(ITableEntryHandle entry, bool singleColumnView, out FrameworkElement content)
            {
                var combobox = new ComboBox();
                combobox.IsEditable = false;
                combobox.ItemsSource = new[] { "Keep", "Treat as Used", "Remove" };

                if (entry.TryGetValue(EditorConfigSettingsTableKeyNames.UpdateAction, out UpdateAction action))
                {
                    combobox.SelectedItem = action switch
                    {
                        UpdateAction.TreatAsUsed => "Treat as Used",
                        _ => "Keep"
                    };
                }

                combobox.SelectionChanged += (object sender, SelectionChangedEventArgs e) =>
                {
                    var action = combobox.SelectedItem switch
                    {
                        "Keep" => UpdateAction.None,
                        "Treat as Used" => UpdateAction.TreatAsUsed,
                        "Remove" => UpdateAction.Remove,
                        _ => UpdateAction.None
                    };

                    entry.TrySetValue(EditorConfigSettingsTableKeyNames.UpdateAction, action);
                };

                content = combobox;
                return true;
            }

            public override bool TryCreateStringContent(ITableEntryHandle entry, bool truncatedText, bool singleColumnView, out string content)
            {
                content = entry.TryGetValue(EditorConfigSettingsTableKeyNames.UpdateAction, out UpdateAction action)
                    ? action.ToString()
                    : string.Empty;
                return !string.IsNullOrEmpty(content);
            }
        }

        private enum UpdateAction
        {
            None,
            TreatAsUsed,
            Remove
        }

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
        }

        /// <summary>
        /// Creates an element to display within the TableControl comprised of both an image and text string.
        /// </summary>
        internal static FrameworkElement CreateGridElement(ImageMoniker imageMoniker, string text, bool isBold)
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

        internal IWpfTableControl4 CreateTableControl()
        {
            return (IWpfTableControl4)_tableControlProvider.CreateControl(
                _tableManager,
                autoSubscribe: true,
                BuildColumnStates(),
                EditorConfigSettingsColumnDefinitions.ReferenceName,
                EditorConfigSettingsColumnDefinitions.UpdateAction);
        }

        private ImmutableArray<ColumnState> BuildColumnStates()
        {
            return ImmutableArray.Create(
                new ColumnState2(EditorConfigSettingsColumnDefinitions.SolutionName, isVisible: false, width: 200, sortPriority: 0, descendingSort: false, groupingPriority: 1),
                new ColumnState2(EditorConfigSettingsColumnDefinitions.ProjectName, isVisible: false, width: 200, sortPriority: 0, descendingSort: false, groupingPriority: 2),
                new ColumnState2(EditorConfigSettingsColumnDefinitions.ReferenceType, isVisible: false, width: 200, sortPriority: 0, descendingSort: false, groupingPriority: 3),
                new ColumnState(EditorConfigSettingsColumnDefinitions.ReferenceName, isVisible: true, width: 0, sortPriority: 0, descendingSort: false),
                new ColumnState(EditorConfigSettingsColumnDefinitions.UpdateAction, isVisible: true, width: 0, sortPriority: 0, descendingSort: false));
        }
    }
}
