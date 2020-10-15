using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Editor.EditorConfigSettings;

namespace Microsoft.CodeAnalysis.Editor.UnitTests.EditorConfigSettings.Mocks
{
    public class MockAnalyzerSetting : IAnalyzerSetting
    {
        public string? Id { get; }
        public string? Title { get; }
        public string? Description { get; }
        public string? Category { get; }
        public string? Language { get; }
        public DiagnosticSeverity Severity { get; private set; }

        public void SetSeverity(DiagnosticSeverity severity)
        {
            Severity = severity;
        }
    }
}
