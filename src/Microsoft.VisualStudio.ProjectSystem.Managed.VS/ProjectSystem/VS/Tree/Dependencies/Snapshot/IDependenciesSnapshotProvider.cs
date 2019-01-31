// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Threading;

namespace Microsoft.VisualStudio.ProjectSystem.VS.Tree.Dependencies.Snapshot
{
    /// <summary>
    /// Provides immutable dependencies snapshot for a given project.
    /// </summary>
    internal interface IDependenciesSnapshotProvider
    {
        /// <summary>
        /// Gets the current immutable dependencies snapshot for the project.
        /// </summary>
        /// <remarks>
        /// Never null.
        /// </remarks>
        IDependenciesSnapshot CurrentSnapshot { get; }

        /// <summary>
        /// Raised when the project's full path changes (i.e. due to being renamed).
        /// </summary>
        /// <remarks>
        /// This event fires after <see cref="CurrentSnapshot"/> is updated with the new path,
        /// but before <see cref="SnapshotChanged"/> fires for that update.
        /// </remarks>
        event EventHandler<ProjectPathChangedEventArgs> ProjectPathChanged;

        /// <summary>
        /// Raised when the project's dependencies snapshot changed.
        /// </summary>
        event EventHandler<SnapshotChangedEventArgs> SnapshotChanged;

        /// <summary>
        /// Raised when the project and its snapshot provider are unloading.
        /// </summary>
        event EventHandler<SnapshotProviderUnloadingEventArgs> SnapshotProviderUnloading;
    }

    internal sealed class SnapshotChangedEventArgs : EventArgs
    {
        public SnapshotChangedEventArgs(IDependenciesSnapshot snapshot, CancellationToken token)
        {
            Requires.NotNull(snapshot, nameof(snapshot));

            Snapshot = snapshot;
            Token = token;
        }

        public IDependenciesSnapshot Snapshot { get; }
        public CancellationToken Token { get; }
    }

    internal sealed class SnapshotProviderUnloadingEventArgs : EventArgs
    {
        public SnapshotProviderUnloadingEventArgs(IDependenciesSnapshotProvider snapshotProvider, CancellationToken token = default)
        {
            Requires.NotNull(snapshotProvider, nameof(snapshotProvider));

            SnapshotProvider = snapshotProvider;
            Token = token;
        }

        public IDependenciesSnapshotProvider SnapshotProvider { get; }
        public CancellationToken Token { get; }
    }

    internal sealed class ProjectPathChangedEventArgs : EventArgs
    {
        public string OldPath { get; }
        public string NewPath { get; }
        public IDependenciesSnapshotProvider SnapshotProvider { get; }

        public ProjectPathChangedEventArgs(string oldPath, string newPath, IDependenciesSnapshotProvider snapshotProvider)
        {
            Requires.NotNullOrWhiteSpace(oldPath, nameof(oldPath));
            Requires.NotNullOrWhiteSpace(newPath, nameof(newPath));
            Assumes.True(!StringComparers.Paths.Equals(oldPath, newPath), "Assume old and new paths are different.");
            Requires.NotNull(snapshotProvider, nameof(snapshotProvider));

            OldPath = oldPath;
            NewPath = newPath;
            SnapshotProvider = snapshotProvider;
        }
    }
}
