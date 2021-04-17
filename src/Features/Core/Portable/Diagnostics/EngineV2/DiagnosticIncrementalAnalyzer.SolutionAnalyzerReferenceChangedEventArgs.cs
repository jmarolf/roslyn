﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Immutable;

namespace Microsoft.CodeAnalysis.Diagnostics.EngineV2
{
    internal partial class DiagnosticIncrementalAnalyzer
    {
        /// <summary>
        /// EventArgs for <see cref="StateManager.SolutionAnalyzerReferenceChanged"/>
        /// 
        /// this event args contains information such as <see cref="CodeAnalysis.Solution"/> the <see cref="AnalyzerReference"/> has changed
        /// and what <see cref="StateSet"/> has changed.
        /// </summary>
        private record SolutionAnalyzerReferenceChangedEventArgs(Solution Solution, ImmutableArray<StateSet> Added, ImmutableArray<StateSet> Removed)
            : EventArgs
        {
        }
    }
}
