using System;
using Microsoft.VisualStudio.Shell.TableManager;

namespace Microsoft.CodeAnalysis.Editor.EditorConfigSettings
{
    internal interface IEditorConfigSettingsWindow
    {
        event EventHandler Closed;
        ITableManager Manager { get; }
        void ShowWindow();
    }
}
