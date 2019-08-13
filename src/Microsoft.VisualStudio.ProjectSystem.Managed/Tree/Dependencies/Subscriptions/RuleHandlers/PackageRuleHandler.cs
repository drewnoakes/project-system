﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.Composition;

using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.ProjectSystem.VS.Tree.Dependencies.CrossTarget;
using Microsoft.VisualStudio.ProjectSystem.VS.Tree.Dependencies.Models;
using Microsoft.VisualStudio.ProjectSystem.VS.Utilities;

namespace Microsoft.VisualStudio.ProjectSystem.VS.Tree.Dependencies.Subscriptions.RuleHandlers
{
    [Export(DependencyRulesSubscriber.DependencyRulesSubscriberContract, typeof(IDependenciesRuleHandler))]
    [Export(typeof(IProjectDependenciesSubTreeProvider))]
    [AppliesTo(ProjectCapability.DependenciesTree)]
    internal sealed partial class PackageRuleHandler : DependenciesRuleHandlerBase
    {
        public const string ProviderTypeString = "NuGetDependency";

        private static readonly SubTreeRootDependencyModel s_rootModel = new SubTreeRootDependencyModel(
            ProviderTypeString,
            Resources.PackagesNodeName,
            new DependencyIconSet(
                icon: ManagedImageMonikers.NuGetGrey,
                expandedIcon: ManagedImageMonikers.NuGetGrey,
                unresolvedIcon: ManagedImageMonikers.NuGetGreyWarning,
                unresolvedExpandedIcon: ManagedImageMonikers.NuGetGreyWarning),
            DependencyTreeFlags.NuGetSubTreeRootNode);

        private readonly ITargetFrameworkProvider _targetFrameworkProvider;

        [ImportingConstructor]
        public PackageRuleHandler(ITargetFrameworkProvider targetFrameworkProvider)
            : base(PackageReference.SchemaName, ResolvedPackageReference.SchemaName)
        {
            _targetFrameworkProvider = targetFrameworkProvider;
        }

        public override string ProviderType => ProviderTypeString;

        public override ImageMoniker ImplicitIcon => ManagedImageMonikers.NuGetGreyPrivate;

        public override void Handle(
            IImmutableDictionary<string, IProjectChangeDescription> changesByRuleName,
            ITargetFramework targetFramework,
            CrossTargetDependenciesChangesBuilder changesBuilder,
            RuleSource source)
        {
            var caseInsensitiveUnresolvedChanges = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (changesByRuleName.TryGetValue(EvaluatedRuleName, out IProjectChangeDescription unresolvedChanges))
            {
                caseInsensitiveUnresolvedChanges.AddRange(unresolvedChanges.After.Items.Keys);

                if (unresolvedChanges.Difference.AnyChanges)
                {
                    HandleChangesForRule(
                        unresolvedChanges,
                        changesBuilder,
                        targetFramework,
                        resolved: false);
                }
            }

            if (changesByRuleName.TryGetValue(ResolvedRuleName, out IProjectChangeDescription resolvedChanges)
                && resolvedChanges.Difference.AnyChanges)
            {
                HandleChangesForRule(
                    resolvedChanges,
                    changesBuilder,
                    targetFramework,
                    resolved: true,
                    unresolvedChanges: caseInsensitiveUnresolvedChanges);
            }
        }

        private void HandleChangesForRule(
            IProjectChangeDescription projectChange,
            CrossTargetDependenciesChangesBuilder changesBuilder,
            ITargetFramework targetFramework,
            bool resolved,
            HashSet<string>? unresolvedChanges = null)
        {
            Requires.NotNull(targetFramework, nameof(targetFramework));

            foreach (string removedItem in projectChange.Difference.RemovedItems)
            {
                if (PackageDependencyMetadata.TryGetMetadata(
                    removedItem,
                    resolved,
                    properties: projectChange.Before.GetProjectItemProperties(removedItem) ?? ImmutableDictionary<string, string>.Empty,
                    unresolvedChanges,
                    targetFramework,
                    _targetFrameworkProvider,
                    out PackageDependencyMetadata metadata))
                {
                    changesBuilder.Removed(targetFramework, ProviderTypeString, metadata.OriginalItemSpec);
                }
            }

            foreach (string changedItem in projectChange.Difference.ChangedItems)
            {
                if (PackageDependencyMetadata.TryGetMetadata(
                    changedItem,
                    resolved,
                    properties: projectChange.After.GetProjectItemProperties(changedItem) ?? ImmutableDictionary<string, string>.Empty,
                    unresolvedChanges,
                    targetFramework,
                    _targetFrameworkProvider,
                    out PackageDependencyMetadata metadata))
                {
                    changesBuilder.Removed(targetFramework, ProviderTypeString, metadata.OriginalItemSpec);
                    changesBuilder.Added(targetFramework, metadata.CreateDependencyModel());
                }
            }

            foreach (string addedItem in projectChange.Difference.AddedItems)
            {
                if (PackageDependencyMetadata.TryGetMetadata(
                    addedItem,
                    resolved,
                    properties: projectChange.After.GetProjectItemProperties(addedItem) ?? ImmutableDictionary<string, string>.Empty,
                    unresolvedChanges,
                    targetFramework,
                    _targetFrameworkProvider,
                    out PackageDependencyMetadata metadata))
                {
                    changesBuilder.Added(targetFramework, metadata.CreateDependencyModel());
                }
            }
        }

        public override IDependencyModel CreateRootDependencyNode() => s_rootModel;
    }
}
