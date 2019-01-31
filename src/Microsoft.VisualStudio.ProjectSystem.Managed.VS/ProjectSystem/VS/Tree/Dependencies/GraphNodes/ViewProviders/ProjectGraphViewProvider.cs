// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

using Microsoft.VisualStudio.GraphModel;
using Microsoft.VisualStudio.ProjectSystem.VS.Tree.Dependencies.CrossTarget;
using Microsoft.VisualStudio.ProjectSystem.VS.Tree.Dependencies.Snapshot;

namespace Microsoft.VisualStudio.ProjectSystem.VS.Tree.Dependencies.GraphNodes.ViewProviders
{
    /// <summary>
    /// Provides the graph of project reference dependencies.
    /// Allows drilling into the transitive dependencies of a given <c>&lt;ProjectReference&gt;</c>.
    /// </summary>
    [Export(typeof(IDependenciesGraphViewProvider))]
    [AppliesTo(ProjectCapability.DependenciesTree)]
    [Order(Order)]
    internal sealed class ProjectGraphViewProvider : GraphViewProviderBase
    {
        public const int Order = 110;

        private readonly IAggregateDependenciesSnapshotProvider _aggregateSnapshotProvider;
        private readonly ITargetFrameworkProvider _targetFrameworkProvider;

        [ImportingConstructor]
        public ProjectGraphViewProvider(
            IDependenciesGraphBuilder builder,
            IAggregateDependenciesSnapshotProvider aggregateSnapshotProvider,
            ITargetFrameworkProvider targetFrameworkProvider)
            : base(builder)
        {
            _aggregateSnapshotProvider = aggregateSnapshotProvider;
            _targetFrameworkProvider = targetFrameworkProvider;
        }

        public override bool SupportsDependency(IDependency dependency)
        {
            // Only supports project reference dependencies
            return dependency.IsProject();
        }

        public override bool HasChildren(IDependency dependency)
        {
            ITargetedDependenciesSnapshot targetedSnapshot = _aggregateSnapshotProvider.GetSnapshot(dependency);

            return targetedSnapshot?.TopLevelDependencies.Length != 0;
        }

        public override void BuildGraph(
            IGraphContext graphContext,
            IProjectIdentity projectId,
            IDependency dependency,
            GraphNode dependencyGraphNode,
            ITargetedDependenciesSnapshot targetedSnapshot)
        {
            // store refreshed dependency
            dependencyGraphNode.SetValue(DependenciesGraphSchema.DependencyIdProperty, dependency.Id);
            dependencyGraphNode.SetValue(DependenciesGraphSchema.ResolvedProperty, dependency.Resolved);

            ITargetedDependenciesSnapshot otherProjectTargetedSnapshot = _aggregateSnapshotProvider.GetSnapshot(dependency);

            if (otherProjectTargetedSnapshot == null)
            {
                return;
            }

            foreach (IDependency childDependency in otherProjectTargetedSnapshot.TopLevelDependencies)
            {
                if (!childDependency.Visible)
                {
                    continue;
                }

                Builder.AddGraphNode(
                    graphContext,
                    dependency.ProjectId,
                    dependencyGraphNode,
                    childDependency.ToViewModel(otherProjectTargetedSnapshot));
            }
        }

        /// <summary>
        /// Returns true if the updated dependency's project matches the updated snapshot's project,
        /// meaning the project dependency has changed and we want to try and update.
        /// </summary>
        /// <inheritdoc />
        public override bool ShouldApplyChanges(IProjectIdentity nodeProjectId, IProjectIdentity updatedSnapshotProjectId, IDependency updatedDependency)
        {
            return Equals(updatedDependency.ProjectId, updatedSnapshotProjectId);
        }

        public override bool ApplyChanges(
            IGraphContext graphContext,
            IProjectIdentity nodeProjectId,
            IDependency updatedDependency,
            GraphNode dependencyGraphNode,
            ITargetedDependenciesSnapshot targetedSnapshot)
        {
            ITargetedDependenciesSnapshot referencedProjectSnapshot = _aggregateSnapshotProvider.GetSnapshot(updatedDependency);

            if (referencedProjectSnapshot == null)
            {
                return false;
            }

            return ApplyChangesInternal(
                graphContext,
                updatedDependency,
                dependencyGraphNode,
                // Project references list all top level dependencies as direct children
                updatedChildren: referencedProjectSnapshot.TopLevelDependencies,
                // Pass the ID of the referenced project
                nodeProjectId: updatedDependency.ProjectId,
                targetedSnapshot: referencedProjectSnapshot);
        }

        public override bool MatchSearchResults(
            IDependency topLevelDependency,
            Dictionary<IProjectIdentity, HashSet<IDependency>> searchResultsPerContext,
            out HashSet<IDependency> topLevelDependencyMatches)
        {
            topLevelDependencyMatches = new HashSet<IDependency>();

            if (!topLevelDependency.Flags.Contains(DependencyTreeFlags.ProjectNodeFlags))
            {
                return false;
            }

            if (!topLevelDependency.Visible)
            {
                return true;
            }

            IProjectIdentity projectId = topLevelDependency.ProjectId;

            if (!searchResultsPerContext.TryGetValue(projectId, out HashSet<IDependency> contextResults)
                || contextResults.Count == 0)
            {
                return true;
            }

            ITargetFramework nearestTargetFramework = _targetFrameworkProvider.GetNearestFramework(
                topLevelDependency.TargetFramework,
                contextResults.Select(x => x.TargetFramework));

            if (nearestTargetFramework == null)
            {
                return true;
            }

            IEnumerable<IDependency> targetedResultsFromContext =
                contextResults.Where(x => nearestTargetFramework.Equals(x.TargetFramework));

            topLevelDependencyMatches.AddRange(targetedResultsFromContext);

            return true;
        }
    }
}
