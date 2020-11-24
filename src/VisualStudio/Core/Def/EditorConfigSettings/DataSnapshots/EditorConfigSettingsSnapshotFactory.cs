// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading;
using Microsoft.VisualStudio.Shell.TableManager;

namespace Microsoft.CodeAnalysis.Editor.EditorConfigSettings
{
    internal sealed class EditorConfigSettingsSnapshotFactory : TableEntriesSnapshotFactoryBase
    {
        private readonly IEditorConfigSettingsDataSource _dataSource;

        // State
        int currentVersionNumber = 0;
        int lastSnapshotVersionNumber = -1;
        private EditorConfigSettingsEntriesSnapshot lastSnapshot;

        // Disallow concurrent modification of state
        private readonly object gate = new object();

        public EditorConfigSettingsSnapshotFactory(IEditorConfigSettingsDataSource dataSource)
        {
            _dataSource = dataSource;
        }

        public override int CurrentVersionNumber => currentVersionNumber;

        public override ITableEntriesSnapshot GetCurrentSnapshot()
        {
            return GetSnapshot(CurrentVersionNumber);
        }

        public override ITableEntriesSnapshot GetSnapshot(int versionNumber)
        {
            lock (gate)
            {
                if (versionNumber == currentVersionNumber)
                {
                    if (lastSnapshotVersionNumber == currentVersionNumber)
                    {
                        return lastSnapshot;
                    }
                    else
                    {
                        var data = _dataSource.GetCurrentDataSnapshot();
                        var snapshot = new EditorConfigSettingsEntriesSnapshot(data, this, currentVersionNumber);

                        lastSnapshot = snapshot;
                        lastSnapshotVersionNumber = this.currentVersionNumber;

                        return snapshot;
                    }
                }
                else if (versionNumber < this.currentVersionNumber)
                {
                    // We can return null from this method.
                    // This will signal to Table Control to request current snapshot.
                    return null;
                }
                else // versionNumber > this.currentVersionNumber
                {
                    throw new InvalidOperationException($"Invalid GetSnapshot request. Requested version: {versionNumber}. Current version: {this.currentVersionNumber}");
                }
            }
        }

        internal void NotifyOfUpdate() => Interlocked.Increment(ref currentVersionNumber);
    }
}
