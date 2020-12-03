// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;

using Constants = Microsoft.VisualStudio.OLE.Interop.Constants;
using VSConstants = Microsoft.VisualStudio.VSConstants;
using ErrorHandler = Microsoft.VisualStudio.ErrorHandler;
using IOleDataObject = Microsoft.VisualStudio.OLE.Interop.IDataObject;

namespace Microsoft.VisualStudio.LanguageServices.Implementation.SettingsUI
{
    internal sealed class EditorConfigUIEditorPane : WindowPane,
                                                     IOleCommandTarget,
                                                     IVsPersistDocData,
                                                     IPersistFileFormat,
                                                     IVsToolboxUser
    {
        public EditorConfigUIEditorPane()
            :base(null)
        {
            // Create the USer Control here
        }

        public int GetGuidEditorType(out Guid pClassID)
        {
            throw new NotImplementedException();
        }

        public int IsDocDataDirty(out int pfDirty)
        {
            throw new NotImplementedException();
        }

        public int SetUntitledDocPath(string pszDocDataPath)
        {
            throw new NotImplementedException();
        }

        public int LoadDocData(string pszMkDocument)
        {
            throw new NotImplementedException();
        }

        public int SaveDocData(VSSAVEFLAGS dwSave, out string pbstrMkDocumentNew, out int pfSaveCanceled)
        {
            throw new NotImplementedException();
        }

        public int Close()
        {
            throw new NotImplementedException();
        }

        public int OnRegisterDocData(uint docCookie, IVsHierarchy pHierNew, uint itemidNew)
        {
            throw new NotImplementedException();
        }

        public int RenameDocData(uint grfAttribs, IVsHierarchy pHierNew, uint itemidNew, string pszMkDocumentNew)
        {
            throw new NotImplementedException();
        }

        public int IsDocDataReloadable(out int pfReloadable)
        {
            throw new NotImplementedException();
        }

        public int ReloadDocData(uint grfFlags)
        {
            throw new NotImplementedException();
        }

        public int GetClassID(out Guid pClassID)
        {
            throw new NotImplementedException();
        }

        public int IsDirty(out int pfIsDirty)
        {
            throw new NotImplementedException();
        }

        public int InitNew(uint nFormatIndex)
        {
            throw new NotImplementedException();
        }

        public int Load(string pszFilename, uint grfMode, int fReadOnly)
        {
            throw new NotImplementedException();
        }

        public int Save(string pszFilename, int fRemember, uint nFormatIndex)
        {
            throw new NotImplementedException();
        }

        public int SaveCompleted(string pszFilename)
        {
            throw new NotImplementedException();
        }

        public int GetCurFile(out string ppszFilename, out uint pnFormatIndex)
        {
            throw new NotImplementedException();
        }

        public int GetFormatList(out string ppszFormatList)
        {
            throw new NotImplementedException();
        }

        public int IsSupported(IOleDataObject pDO)
        {
            throw new NotImplementedException();
        }

        public int ItemPicked(IOleDataObject pDO)
        {
            throw new NotImplementedException();
        }
    }
}
