using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.CodeAnalysis.Editor.EditorConfigSettings
{
    internal interface IAnalyzerSetting
    {
        string? Id { get; }
        string? Title { get; }
        string? Description { get; }
        string? Category { get; }
        string? Language { get; }
        DiagnosticSeverity Severity { get; }

        void SetSeverity(DiagnosticSeverity severity);
    }

    internal interface IAnalyzerSettingFromEditorConfig : IAnalyzerSetting
    {
        string? EditorConfigPath { get; }
    }

    internal interface IAnalyzerSettingFromVisualStudio : IAnalyzerSetting
    {
    }
}
