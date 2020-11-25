// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Immutable;
using System.ComponentModel.Composition;
using Microsoft.CodeAnalysis.Editor.EditorConfigSettings;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.Internal.VisualStudio.Shell.TableControl;
using Microsoft.VisualStudio.Shell.TableControl;
using Microsoft.VisualStudio.Shell.TableManager;

namespace Microsoft.VisualStudio.LanguageServices.EditorConfigSettings
{
    [Export(typeof(EditorConfigSettingsTableProvider))]
    internal partial class EditorConfigSettingsTableProvider
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

        internal ITableManager TableManager => _tableManager;

        internal IWpfTableControl4 CreateTableControl()
        {
            return (IWpfTableControl4)_tableControlProvider.CreateControl(
                _tableManager,
                autoSubscribe: true,
                BuildColumnStates(),
                EditorConfigSettingsColumnDefinitions.IdName,
                EditorConfigSettingsColumnDefinitions.TitleName,
                EditorConfigSettingsColumnDefinitions.DescriptionName,
                EditorConfigSettingsColumnDefinitions.CategoryName,
                EditorConfigSettingsColumnDefinitions.SeverityName);
        }

        private ImmutableArray<ColumnState> BuildColumnStates()
        {
            return ImmutableArray.Create(
                new ColumnState(EditorConfigSettingsColumnDefinitions.IdName, isVisible: true, width: 0, sortPriority: 0, descendingSort: true),
                new ColumnState(EditorConfigSettingsColumnDefinitions.TitleName, isVisible: true, width: 0, sortPriority: 0, descendingSort: false),
                new ColumnState(EditorConfigSettingsColumnDefinitions.DescriptionName, isVisible: true, width: 0, sortPriority: 0, descendingSort: false),
                new ColumnState(EditorConfigSettingsColumnDefinitions.CategoryName, isVisible: true, width: 0, sortPriority: 0, descendingSort: false),
                new ColumnState(EditorConfigSettingsColumnDefinitions.SeverityName, isVisible: true, width: 0, sortPriority: 0, descendingSort: false));
        }
    }
}
