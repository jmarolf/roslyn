// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.ComponentModel.Composition;
using System.Runtime.InteropServices;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace Microsoft.VisualStudio.LanguageServices.Implementation.SettingsUI
{
    /// <summary>
    /// This class contains a list of GUIDs specific to this sample, 
    /// especially the package GUID and the commands group GUID. 
    /// </summary>
    internal static class GuidStrings
    {
        public const string GuidEditorFactory = "68b46364-d378-42f2-9e72-37d86c5f4468";
    }

    /// <summary>
    /// List of the GUID objects.
    /// </summary>
    internal static class GuidList
    {
        public static readonly Guid guidEditorFactory = new Guid(GuidStrings.GuidEditorFactory);
    };

    [Export(typeof(EditorConfigUIFactory))]
    [Guid(GuidStrings.GuidEditorFactory)]
    internal sealed class EditorConfigUIFactory : IVsEditorFactory, IDisposable
    {
        private ServiceProvider? _vsServiceProvider;

        [ImportingConstructor]
        [Obsolete(MefConstruction.ImportingConstructorMessage, error: true)]
        public EditorConfigUIFactory()
        {
        }

        public int CreateEditorInstance(uint grfCreateDoc,
                                        string pszMkDocument,
                                        string pszPhysicalView,
                                        IVsHierarchy pvHier,
                                        uint itemid,
                                        IntPtr punkDocDataExisting,
                                        out IntPtr ppunkDocView,
                                        out IntPtr ppunkDocData,
                                        out string? pbstrEditorCaption,
                                        out Guid pguidCmdUI,
                                        out int pgrfCDW)
        {
            // Initialize to null
            ppunkDocView = IntPtr.Zero;
            ppunkDocData = IntPtr.Zero;
            pguidCmdUI = GuidList.guidEditorFactory;
            pgrfCDW = 0;
            pbstrEditorCaption = null;

            // Validate inputs
            if ((grfCreateDoc & (VSConstants.CEF_OPENFILE | VSConstants.CEF_SILENT)) == 0)
            {
                return VSConstants.E_INVALIDARG;
            }
            if (punkDocDataExisting != IntPtr.Zero)
            {
                return VSConstants.VS_E_INCOMPATIBLEDOCDATA;
            }

            // Create the Document (editor)
            var newEditor = new EditorConfigUIEditorPane();
            ppunkDocView = Marshal.GetIUnknownForObject(newEditor);
            ppunkDocData = Marshal.GetIUnknownForObject(newEditor);
            pbstrEditorCaption = "";

            return VSConstants.S_OK;
        }

        public int SetSite(IOleServiceProvider psp)
        {
            _vsServiceProvider = new ServiceProvider(psp);
            return VSConstants.S_OK;
        }

        public int Close() => VSConstants.S_OK;

        public int MapLogicalView(ref Guid rguidLogicalView, out string? pbstrPhysicalView)
        {
            pbstrPhysicalView = null;   // initialize out parameter

            // we support only a single physical view
            if (VSConstants.LOGVIEWID_Primary == rguidLogicalView)
            {
                // primary view uses NULL as pbstrPhysicalView
                return VSConstants.S_OK;
            }
            else
            {
                // you must return E_NOTIMPL for any unrecognized rguidLogicalView values
                return VSConstants.E_NOTIMPL;
            }
        }

        public void Dispose()
        {
            // Since we create a ServiceProvider which implements IDisposable we
            // also need to implement IDisposable to make sure that the ServiceProvider's
            // Dispose method gets called.
            if (_vsServiceProvider is not null)
            {
                _vsServiceProvider.Dispose();
                _vsServiceProvider = null;
            }
        }
    }
}
