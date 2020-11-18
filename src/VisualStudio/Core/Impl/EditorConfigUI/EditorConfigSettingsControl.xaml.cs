// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Windows;
using Microsoft.CodeAnalysis.Editor.EditorConfigSettings;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell.TableManager;

namespace Microsoft.VisualStudio.LanguageServices.Implementation.EditorConfigUI
{
    /// <summary>
    /// Interaction logic for EditorConfigSettingsDialog.xaml
    /// </summary>
    internal partial class EditorConfigSettingsControl : DialogWindow, IEditorConfigSettingsWindow
    {

        public string RemoveUnusedReferences => "Remove Unused References";
        public string HelpText => "Choose which action you would like to perform on the unused references.";
        public string RemoveAll => "Remove All";
        public string Apply => "Apply";
        public string Cancel => ServicesVSResources.Cancel;

        public EditorConfigSettingsControl(FrameworkElement element, ITableManager tableManager)
        {
            InitializeComponent();

            TablePanel.Child = element;
            Manager = tableManager;
        }

        public ITableManager Manager { get; }

        public event EventHandler Closed;

        public void ShowWindow()
        {
            this.Show();
        }

        protected override void OnClosed(EventArgs e)
        {
            Closed?.Invoke(this, e);
        }
    }
}
