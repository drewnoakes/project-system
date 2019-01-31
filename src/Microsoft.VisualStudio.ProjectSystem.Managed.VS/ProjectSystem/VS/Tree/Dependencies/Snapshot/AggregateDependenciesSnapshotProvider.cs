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
        private ImmutableDictionary<string, IDependenciesSnapshotProvider> _snapshotProviderByProjectPath
            = ImmutableDictionary.Create<string, IDependenciesSnapshotProvider>(StringComparer.OrdinalIgnoreCase);

        /// <summary>Used to serialize writes to <see cref="_snapshotProviderByProjectPath"/>.</summary>
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
                string projectPath = snapshotProvider.CurrentSnapshot.ProjectPath;

                _snapshotProviderByProjectPath = _snapshotProviderByProjectPath.Add(projectPath, snapshotProvider);

                snapshotProvider.SnapshotChanged += OnSnapshotChanged;
                snapshotProvider.ProjectPathChanged += OnProjectPathChanged;
                snapshotProvider.SnapshotProviderUnloading += OnSnapshotProviderUnloading;
            }

            void OnSnapshotProviderUnloading(object sender, SnapshotProviderUnloadingEventArgs e)
            {
                // Project has unloaded, so remove it from the cache and unregister event handlers
                SnapshotProviderUnloading?.Invoke(this, e);

                lock (_updateLock)
                {
                    bool removed = ImmutableInterlocked.TryRemove(
                        ref _snapshotProviderByProjectPath,
                        snapshotProvider.CurrentSnapshot.ProjectPath,
                        out IDependenciesSnapshotProvider removedProvider);

                    if (removed)
                    {
                        Assumes.True(ReferenceEquals(snapshotProvider, removedProvider), "Unexpected provider removed from map");

                        snapshotProvider.SnapshotChanged -= OnSnapshotChanged;
                        snapshotProvider.ProjectPathChanged -= OnProjectPathChanged;
                        snapshotProvider.SnapshotProviderUnloading -= OnSnapshotProviderUnloading;
                    }
                    else
                    {
                        System.Diagnostics.Debug.Fail(
                            $"Received {nameof(snapshotProvider.SnapshotProviderUnloading)} for unknown project: {snapshotProvider.CurrentSnapshot.ProjectPath}");
                    }
                }
            }

            void OnProjectPathChanged(object sender, ProjectPathChangedEventArgs e)
            {
                // The project path has changed, so update the key under which the provider is kept
                lock (_updateLock)
                {
                    ImmutableDictionary<string, IDependenciesSnapshotProvider> dic = _snapshotProviderByProjectPath;

                    if (dic.TryGetValue(e.OldPath, out IDependenciesSnapshotProvider provider))
                    {
                        System.Diagnostics.Debug.Assert(
                            ReferenceEquals(e.SnapshotProvider, provider),
                            $"Received {nameof(snapshotProvider.ProjectPathChanged)} for incorrect provider");

                        dic = dic
                            .Remove(e.OldPath)
                            .SetItem(e.NewPath, provider);

                        _snapshotProviderByProjectPath = dic;
                    }
                    else
                    {
                        System.Diagnostics.Debug.Fail(
                            $"Received {nameof(snapshotProvider.ProjectPathChanged)} for unknown project path (within lock): {e.OldPath}");
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
        public IDependenciesSnapshot GetSnapshot(string projectFilePath)
        {
            Requires.NotNullOrEmpty(projectFilePath, nameof(projectFilePath));

            if (!_snapshotProviderByProjectPath.TryGetValue(projectFilePath, out IDependenciesSnapshotProvider snapshotProvider))
            {
                // BUG we end up in here when the project has been renamed and the previous path is being remembered elsewhere
            }

            return snapshotProvider?.CurrentSnapshot;
        }

        /// <inheritdoc />
        public ITargetedDependenciesSnapshot GetSnapshot(IDependency dependency)
        {
            IDependenciesSnapshot snapshot = GetSnapshot(dependency.FullPath);

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
            ImmutableDictionary<string, IDependenciesSnapshotProvider> dic = _snapshotProviderByProjectPath;

            ImmutableArray<IDependenciesSnapshot>.Builder builder = ImmutableArray.CreateBuilder<IDependenciesSnapshot>(dic.Count);

            foreach ((string _, IDependenciesSnapshotProvider provider) in dic)
            {
                builder.Add(provider.CurrentSnapshot);
            }

            return builder.MoveToImmutable();
        }
    }
}
