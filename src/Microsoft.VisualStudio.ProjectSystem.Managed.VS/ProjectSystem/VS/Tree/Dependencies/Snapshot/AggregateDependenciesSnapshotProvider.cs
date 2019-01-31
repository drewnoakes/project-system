// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.Composition;

using Microsoft.VisualStudio.ProjectSystem.VS.Tree.Dependencies.CrossTarget;

namespace Microsoft.VisualStudio.ProjectSystem.VS.Tree.Dependencies.Snapshot
{
    /// <inheritdoc />
    [Export(typeof(IAggregateDependenciesSnapshotProvider))]
    [AppliesTo(ProjectCapability.DependenciesTree)]
    internal sealed class AggregateDependenciesSnapshotProvider : IAggregateDependenciesSnapshotProvider
    {
        private ImmutableDictionary<IProjectIdentity, IDependenciesSnapshotProvider> _snapshotProviderByProjectId
            = ImmutableDictionary.Create<IProjectIdentity, IDependenciesSnapshotProvider>();

        /// <summary>Used to serialize writes to <see cref="_snapshotProviderByProjectId"/>.</summary>
        private readonly object _updateLock = new object();

        private readonly ITargetFrameworkProvider _targetFrameworkProvider;

        [ImportingConstructor]
        public AggregateDependenciesSnapshotProvider(ITargetFrameworkProvider targetFrameworkProvider)
        {
            _targetFrameworkProvider = targetFrameworkProvider;
        }

        /// <inheritdoc />
        public event EventHandler<SnapshotChangedEventArgs> SnapshotChanged;

        /// <inheritdoc />
        public event EventHandler<SnapshotProviderUnloadingEventArgs> SnapshotProviderUnloading;

        /// <inheritdoc />
        public void RegisterSnapshotProvider(IDependenciesSnapshotProvider snapshotProvider)
        {
            if (snapshotProvider == null)
            {
                return;
            }

            lock (_updateLock)
            {
                IProjectIdentity projectId = snapshotProvider.CurrentSnapshot.ProjectId;

                _snapshotProviderByProjectId = _snapshotProviderByProjectId.Add(projectId, snapshotProvider);

                snapshotProvider.SnapshotChanged += OnSnapshotChanged;
                snapshotProvider.SnapshotProviderUnloading += OnSnapshotProviderUnloading;
            }

            void OnSnapshotProviderUnloading(object sender, SnapshotProviderUnloadingEventArgs e)
            {
                // Project has unloaded, so remove it from the cache and unregister event handlers
                SnapshotProviderUnloading?.Invoke(this, e);

                lock (_updateLock)
                {
                    bool removed = ImmutableInterlocked.TryRemove(
                        ref _snapshotProviderByProjectId,
                        snapshotProvider.CurrentSnapshot.ProjectId,
                        out IDependenciesSnapshotProvider removedProvider);

                    if (removed)
                    {
                        Assumes.True(ReferenceEquals(snapshotProvider, removedProvider), "Unexpected provider removed from map");

                        snapshotProvider.SnapshotChanged -= OnSnapshotChanged;
                        snapshotProvider.SnapshotProviderUnloading -= OnSnapshotProviderUnloading;
                    }
                    else
                    {
                        System.Diagnostics.Debug.Fail(
                            $"Received {nameof(snapshotProvider.SnapshotProviderUnloading)} for unknown project: {snapshotProvider.CurrentSnapshot.ProjectId}");
                    }
                }
            }

            void OnSnapshotChanged(object sender, SnapshotChangedEventArgs e)
            {
                // Propagate the change event
                SnapshotChanged?.Invoke(this, e);
            }
        }

        /// <inheritdoc />
        public IDependenciesSnapshot GetSnapshot(IProjectIdentity projectId)
        {
            Requires.NotNull(projectId, nameof(projectId));

            if (!_snapshotProviderByProjectId.TryGetValue(projectId, out IDependenciesSnapshotProvider snapshotProvider))
            {
                // BUG we end up in here when the project has been renamed and the previous path is being remembered elsewhere
            }

            return snapshotProvider?.CurrentSnapshot;
        }

        /// <inheritdoc />
        public ITargetedDependenciesSnapshot GetSnapshot(IDependency dependency)
        {
            IDependenciesSnapshot snapshot = GetSnapshot(dependency.ProjectId);

            if (snapshot == null)
            {
                return null;
            }

            ITargetFramework targetFramework = _targetFrameworkProvider.GetNearestFramework(
                dependency.TargetFramework, snapshot.Targets.Keys);

            if (targetFramework == null)
            {
                return null;
            }

            return snapshot.Targets[targetFramework];
        }

        /// <inheritdoc />
        public ImmutableArray<IDependenciesSnapshot> GetSnapshots()
        {
            // Copy reference to immutable collection for consistency within this method
            ImmutableDictionary<IProjectIdentity, IDependenciesSnapshotProvider> dic = _snapshotProviderByProjectId;

            ImmutableArray<IDependenciesSnapshot>.Builder builder = ImmutableArray.CreateBuilder<IDependenciesSnapshot>(dic.Count);

            foreach ((IProjectIdentity _, IDependenciesSnapshotProvider provider) in dic)
            {
                builder.Add(provider.CurrentSnapshot);
            }

            return builder.MoveToImmutable();
        }
    }
}
