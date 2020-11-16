// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.CodeAnalysis.Editor.EditorConfigSettings;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell.TableManager;

namespace Microsoft.VisualStudio.LanguageServices.Implementation.EditorConfigUI
{
    /// <summary>
    /// Interaction logic for EditorConfigSettingsDialog.xaml
    /// </summary>
    internal partial class EditorConfigSettingsWindow : DialogWindow, IEditorConfigSettingsWindow
    {

        public string RemoveUnusedReferences => "Remove Unused References";
        public string HelpText => "Choose which action you would like to perform on the unused references.";
        public string RemoveAll => "Remove All";
        public string Apply => "Apply";
        public string Cancel => ServicesVSResources.Cancel;

        public EditorConfigSettingsWindow(FrameworkElement element)
        {
            InitializeComponent();

            TablePanel.Child = element;
        }

        public ITableManager Manager { get; }
    }
}
