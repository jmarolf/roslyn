﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#nullable enable

using System;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis.ErrorReporting;
using Microsoft.CodeAnalysis.Shared.Extensions;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis
{
    internal partial class SolutionState
    {
        /// <inheritdoc cref="Solution.GetOriginatingProjectId"/>
        public ProjectId? GetOriginatingProjectId(ISymbol? symbol)
        {
            LazyInitialization.EnsureInitialized(ref _unrootedSymbolToProjectId, s_createTable);

            // Walk up the symbol so we can get to the containing namespace/assembly that will be used to map
            // back to a project.

            while (symbol != null)
            {
                var result = GetProjectIdDirectly(symbol, _unrootedSymbolToProjectId);
                if (result != null)
                    return result;

                symbol = symbol.ContainingSymbol;
            }

            return null;
        }

        private ProjectId? GetProjectIdDirectly(
            ISymbol symbol, ConditionalWeakTable<ISymbol, ProjectId?> unrootedSymbolToProjectId)
        {
            if (symbol.IsKind(SymbolKind.Namespace, out INamespaceSymbol? ns))
            {
                if (ns.ContainingCompilation != null)
                {
                    // A namespace that spans a compilation.  These don't belong to an assembly/module directly.
                    // However, as we're looking for the project this corresponds to, we can look for the
                    // source-module component (the first in the constituent namespaces) and then search using that.
                    return GetOriginatingProjectId(ns.ConstituentNamespaces[0]);
                }
            }
            else if (symbol.IsKind(SymbolKind.Assembly) ||
                     symbol.IsKind(SymbolKind.NetModule) ||
                     symbol.IsKind(SymbolKind.DynamicType))
            {
                if (!unrootedSymbolToProjectId.TryGetValue(symbol, out var projectId))
                {
                    foreach (var (id, tracker) in _projectIdToTrackerMap)
                    {
                        if (tracker.ContainsAssemblyOrModuleOrDynamic(symbol))
                        {
                            projectId = id;
                            break;
                        }
                    }

                    // Have to lock as there's no atomic AddOrUpdate in netstandard2.0 and we could throw if two
                    // threads tried to add the same item.
#if !NETCOREAPP
                    lock (unrootedSymbolToProjectId)
                    {
                        unrootedSymbolToProjectId.Remove(symbol);
                        unrootedSymbolToProjectId.Add(symbol, projectId);
                    }
#else
                    unrootedSymbolToProjectId.AddOrUpdate(symbol, projectId);
#endif
                }

                return projectId;
            }
            else if (symbol.IsKind(SymbolKind.TypeParameter, out ITypeParameterSymbol? typeParameter) &&
                     typeParameter.TypeParameterKind == TypeParameterKind.Cref)
            {
                // Cref type parameters don't belong to any containing symbol.  But we can map them to a doc/project
                // using the declaring syntax of the type parameter itself.
                var tree = typeParameter.Locations[0].SourceTree;
                var doc = this.GetDocumentState(tree, projectId: null);
                return doc?.Id.ProjectId;
            }
            else if (SymbolKey.IsBodyLevelSymbol(symbol))
            {
                // If this is a method-body-level symbol, then we will have it's syntax tree.  Since  we already have a
                // mapping from syntax-trees to docs, so we can immediately map this back to it's originating project.
                //
                // Note: we don't do this for all source symbols, only method-body-level ones.  That's because other
                // source symbols may be *retargetted*.  So you can have the same symbol retargetted into multiple
                // projects, but which have the same syntax-tree (which is only in one project).  We need to actually
                // check it's assembly symbol so that we get the actual project it is from (the original project, or the
                // retargetted project).
                var syntaxTree = symbol.DeclaringSyntaxReferences[0].SyntaxTree;
                var documentId = DocumentState.GetDocumentIdForTree(syntaxTree);
                if (documentId == null)
                {
                    try
                    {
                        throw new InvalidOperationException(
                            $"We should always be able to map a source symbol back to a document:\r\n{symbol.Kind}\r\n{symbol.Name}\r\n{syntaxTree.FilePath}");
                    }
                    catch (Exception ex) when (FatalError.ReportWithoutCrash(ex))
                    {
                        return null;
                    }
                }

                return documentId.ProjectId;
            }

            return null;
        }
    }
}
