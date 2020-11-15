// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Concurrent;

namespace Microsoft.CodeAnalysis
{
    internal sealed class CachingAnalyzerConfigSet
    {
        private readonly ConcurrentDictionary<string, AnalyzerConfigOptionsResult> _sourcePathToResult = new();
        private readonly Func<string, AnalyzerConfigOptionsResult> _computeFunction;
        internal readonly AnalyzerConfigSet UnderlyingSet;

        public AnalyzerConfigOptionsResult GlobalConfigOptions => UnderlyingSet.GlobalConfigOptions;

        public CachingAnalyzerConfigSet(AnalyzerConfigSet underlyingSet)
        {
            UnderlyingSet = underlyingSet;
            _computeFunction = UnderlyingSet.GetOptionsForSourcePath;
        }

        public AnalyzerConfigOptionsResult GetOptionsForSourcePath(string sourcePath)
        {
            return _sourcePathToResult.GetOrAdd(sourcePath, _computeFunction);
        }
    }
}
