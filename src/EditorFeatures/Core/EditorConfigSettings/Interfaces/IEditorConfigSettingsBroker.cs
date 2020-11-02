// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Host.Mef;

namespace Microsoft.CodeAnalysis.Editor
{
    internal interface IEditorConfigSettingsBroker
    {
        /// <summary>
        /// Shows the applicable settings for the given document.
        /// Returns immediately and asynchronously brings up the settings UI
        /// </summary>
        /// <param name="document">Document to use to populate the settings from</param>
        void ShowEditorConfigSettings(Document document);

        /// <summary>
        /// Shows the applicable settings for the given document.
        /// Returns a task that completes when the UI has been shown
        /// </summary>
        /// <param name="document">Document to use to populate the settings from</param>
        Task ShowEditorConfigSettingsAsync(Document document, CancellationToken token);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="project"></param>
        void ShowEditorConfigSettings(Project project);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="project"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task ShowEditorConfigSettingsAsync(Project project, CancellationToken token);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="solution"></param>
        void ShowEditorConfigSettings(Solution solution);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="solution"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task ShowEditorConfigSettingsAsync(Solution solution, CancellationToken token);
    }
}
