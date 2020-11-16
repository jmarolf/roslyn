// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Host.Mef;

namespace Microsoft.CodeAnalysis.Editor
{
    [Export(typeof(IEditorConfigSettingsBroker)), Shared]
    internal class EditorConfigSettingsBroker : IEditorConfigSettingsBroker
    {
        private readonly IEditorConfigSettingsPresenterProvider _editorPresenationProvider;
        private readonly IEditorConfigSettingsDataRepositoryProvider _editorConfigSettingsDataRepositoryProvider;

        [ImportingConstructor]
        // TODO(jmarolf): update tests to consume this via MEF
        // [Obsolete(MefConstruction.ImportingConstructorMessage, error: true)]
        public EditorConfigSettingsBroker(
            IEditorConfigSettingsPresenterProvider editorPresenationProvider,
            IEditorConfigSettingsDataRepositoryProvider editorConfigSettingsDataRepositoryProvider
            )
        {
            _editorPresenationProvider = editorPresenationProvider;
            _editorConfigSettingsDataRepositoryProvider = editorConfigSettingsDataRepositoryProvider;
        }

        public void ShowEditorConfigSettings(Document document)
        {
            if (document is null)
                throw new ArgumentNullException(nameof(document));
            if (document.FilePath is null)
                throw new ArgumentException($"Path for {document} must have a non-null path."); // TODO(jmarolf): add resource

            _ = ShowEditorConfigSettingsAsync(document.FilePath, default);
        }

        public void ShowEditorConfigSettings(Project project)
        {
            if (project is null)
                throw new ArgumentNullException(nameof(project));
            if (project.FilePath is null)
                throw new ArgumentException($"Path for {project} must have a non-null path."); // TODO(jmarolf): add resource

            _ = ShowEditorConfigSettingsAsync(project.FilePath, default);
        }

        public void ShowEditorConfigSettings(Solution solution)
        {
            if (solution is null)
                throw new ArgumentNullException(nameof(solution));
            if (solution.FilePath is null)
                throw new ArgumentException($"Path for {solution} must have a non-null path."); // TODO(jmarolf): add resource

            _ = ShowEditorConfigSettingsAsync(solution.FilePath, default);
        }

        public Task ShowEditorConfigSettingsAsync(Project project, CancellationToken token)
        {
            if (project is null)
                throw new ArgumentNullException(nameof(project));
            if (project.FilePath is null)
                throw new ArgumentException($"Path for {project} must have a non-null path."); // TODO(jmarolf): add resource

            return ShowEditorConfigSettingsAsync(project.FilePath, token);
        }

        public Task ShowEditorConfigSettingsAsync(Solution solution, CancellationToken token)
        {
            if (solution is null)
                throw new ArgumentNullException(nameof(solution));
            if (solution.FilePath is null)
                throw new ArgumentException($"Path for {solution} must have a non-null path."); // TODO(jmarolf): add resource

            return ShowEditorConfigSettingsAsync(solution.FilePath, token);
        }

        public Task ShowEditorConfigSettingsAsync(Document document, CancellationToken token)
        {
            if (document is null)
                throw new ArgumentNullException(nameof(document));
            if (document.FilePath is null)
                throw new ArgumentException($"Path for {document} must have a non-null path."); // TODO(jmarolf): add resource

            return ShowEditorConfigSettingsAsync(document.FilePath, token);
        }

        private Task ShowEditorConfigSettingsAsync(string path, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            var dataRepository = _editorConfigSettingsDataRepositoryProvider.GetDataRepository(this, path);
            return _editorPresenationProvider.ShowAsync(dataRepository, token);
        }
    }
}
