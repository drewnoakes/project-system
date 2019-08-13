// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;

using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.ProjectSystem.Properties;
using Microsoft.VisualStudio.ProjectSystem.VS.Tree.Dependencies.CrossTarget;
using Microsoft.VisualStudio.ProjectSystem.VS.Utilities;

namespace Microsoft.VisualStudio.ProjectSystem.VS.Tree.Dependencies.Subscriptions.RuleHandlers
{
    internal abstract class DependenciesRuleHandlerBase
        : IDependenciesRuleHandler,
          IProjectDependenciesSubTreeProviderInternal
    {
        public string EvaluatedRuleName { get; }
        public string ResolvedRuleName { get; }

        protected DependenciesRuleHandlerBase(
            string evaluatedRuleName,
            string resolvedRuleName)
        {
            Requires.NotNullOrWhiteSpace(evaluatedRuleName, nameof(evaluatedRuleName));
            Requires.NotNullOrWhiteSpace(resolvedRuleName, nameof(resolvedRuleName));

            EvaluatedRuleName = evaluatedRuleName;
            ResolvedRuleName = resolvedRuleName;
        }

        #region IDependenciesRuleHandler

        public abstract ImageMoniker ImplicitIcon { get; }

        public virtual void Handle(
            IImmutableDictionary<string, IProjectChangeDescription> changesByRuleName,
            ITargetFramework targetFramework,
            CrossTargetDependenciesChangesBuilder changesBuilder,
            RuleSource source)
        {
            // We receive data from two sources, and handle them differently.

            // TODO process *every* evaluation, but ignore build data that's older than the latest evaluation (i think...)
            // TODO WAS: ignore evaluation updates that occur *after* the same joint update (check (project?) version(s)) -- do this at a higher level? ProjectDataSources.ConfiguredProjectVersion

            if (source == RuleSource.Evaluation)
            {
                IProjectChangeDescription evaluatedChanges = changesByRuleName[EvaluatedRuleName];

                ProcessEvaluation(evaluatedChanges, null);
            }
            else if (source == RuleSource.Joint)
            {
                IProjectChangeDescription evaluatedChanges = changesByRuleName[EvaluatedRuleName];
                IProjectChangeDescription resolvedChanges = changesByRuleName[ResolvedRuleName];

                ProcessEvaluation(evaluatedChanges, resolvedChanges);
                ProcessBuild(evaluatedChanges, resolvedChanges);
            }

            return;

            void ProcessEvaluation(IProjectChangeDescription evaluatedChanges, IProjectChangeDescription? resolvedChanges)
            {
                // Evaluation data can create and remove tree items, which build data cannot.

                // Evaluation has completed. We don't yet have DTB data for this project version to know whether they are resolved,
                // but we still want to show something in the tree to keep the UI responsive.

                // We want newly added tree items to appear as resolved until we know (via DTB) whether they are unresolved.
                // To achieve that we could set resolved=true for all evaluated items and let the resolved rule determine
                // Items observed via evaluation are used to create tree items.
                // They do not set the resolved state of that dependency one way or another.

                // NOTE we check counts to avoid allocating enumerators

                HashSet<string>? resolvedItemSpecs = null;

                if (evaluatedChanges.Difference.AddedItems.Count != 0)
                {
                    // NOTE each evaluated item is (potentially) added twice. Initially by an evaluation, again when that evaluation's design time build completes.
                    resolvedItemSpecs = GetResolvedItemSpecs();

                    foreach (string addedItem in evaluatedChanges.Difference.AddedItems)
                    {
                        bool resolved = resolvedItemSpecs?.Contains(addedItem) ?? true; // innocent until proven guilty

                        changesBuilder.Added(targetFramework, CreateDependencyModelForRule(addedItem, evaluatedChanges.After, resolved));
                    }
                }

                if (evaluatedChanges.Difference.ChangedItems.Count != 0)
                {
                    resolvedItemSpecs ??= GetResolvedItemSpecs();

                    foreach (string changedItem in evaluatedChanges.Difference.ChangedItems)
                    {
                        bool resolved = resolvedItemSpecs?.Contains(changedItem) ?? true; // innocent until proven guilty

                        // For changes we try to add new dependency. If it is a resolved dependency, it would just override
                        // old one with new properties. If it is unresolved dependency, it would be added only when there no
                        // resolved version in the snapshot. TODO MAKE THIS TRUE!!!!!!!!!!!!!!!!!!
                        changesBuilder.Added(targetFramework, CreateDependencyModelForRule(changedItem, evaluatedChanges.After, resolved));
                    }
                }

                if (evaluatedChanges.Difference.RemovedItems.Count != 0)
                {
                    foreach (string removedItem in evaluatedChanges.Difference.RemovedItems)
                    {
                        changesBuilder.Removed(targetFramework, ProviderType, removedItem);
                    }
                }

                System.Diagnostics.Debug.Assert(evaluatedChanges.Difference.RenamedItems.Count == 0, "Project rule diff should not contain renamed items");

                return;

                HashSet<string>? GetResolvedItemSpecs()
                {
                    if (resolvedChanges == null)
                        return null;

                    return new HashSet<string>(EnumerateOriginalItemSpecs()); // TODO comparator

                    IEnumerable<string> EnumerateOriginalItemSpecs()
                    {
                        foreach ((string itemSpec, IImmutableDictionary<string, string> properties) in resolvedChanges.After.Items)
                        {
                            // TODO review this -- can OriginalItemSpec be null?
                            string modelId = properties.GetStringProperty(ResolvedAssemblyReference.OriginalItemSpecProperty) ?? itemSpec;

                            yield return modelId;
                        }
                    }
                }

                IDependencyModel CreateDependencyModelForRule(string itemSpec, IProjectRuleSnapshot projectRuleSnapshot, bool resolved)
                {
                    IImmutableDictionary<string, string> properties = projectRuleSnapshot.GetProjectItemProperties(itemSpec)!;

                    bool isImplicit = properties.GetBoolProperty(ProjectItemMetadata.IsImplicitlyDefined) ?? false;

                    return CreateDependencyModel(
                        itemSpec,
                        originalItemSpec: itemSpec,
                        resolved,
                        isImplicit,
                        properties);
                }
            }

            void ProcessBuild(IProjectChangeDescription evaluatedChanges, IProjectChangeDescription resolvedChanges)
            {
                // TODO items that are never resolved will not feature in resolvedChanges, yet we need to mark them as such

                // Items observed via design-time builds add more metadata to tree items already added due to evaluation.

                // NOTE we check counts to avoid allocating enumerators

                if (resolvedChanges.Difference.AddedItems.Count != 0)
                {
                    foreach (string addedItem in resolvedChanges.Difference.AddedItems)
                    {
                        IDependencyModel model = CreateDependencyModelForRule(addedItem, resolvedChanges.After, resolved: true);

                        if (!evaluatedChanges.After.Items.ContainsKey(model.Id))
                        {
                            // Design-time builds are not allowed to add items.
                            // TODO consider logging diagnostic information here to help people understand why their items aren't appearing
                            continue;
                        }

                        changesBuilder.Added(targetFramework, model);
                    }
                }

                if (resolvedChanges.Difference.ChangedItems.Count != 0)
                {
                    foreach (string changedItem in resolvedChanges.Difference.ChangedItems)
                    {
                        IDependencyModel model = CreateDependencyModelForRule(changedItem, resolvedChanges.After, resolved: true);

                        if (!evaluatedChanges.After.Items.ContainsKey(model.Id))
                        {
                            // Design-time builds are not allowed to add items.
                            // TODO can we actually hit this?
                            continue;
                        }

                        // For changes we try to add new dependency. If it is a resolved dependency, it would just override
                        // old one with new properties. If it is unresolved dependency, it would be added only when there no
                        // resolved version in the snapshot.
                        changesBuilder.Added(targetFramework, model);
                    }
                }

                if (resolvedChanges.Difference.RemovedItems.Count != 0)
                {
                    foreach (string removedItem in resolvedChanges.Difference.RemovedItems)
                    {
                        IImmutableDictionary<string, string> properties = resolvedChanges.Before.GetProjectItemProperties(removedItem)!;

                        // NOTE for packages the original item spec may be in the "Name" property (?)
                        string originalItemSpec = properties.GetStringProperty(ResolvedAssemblyReference.OriginalItemSpecProperty)!;

                        Assumes.NotNull(originalItemSpec);

                        if (!evaluatedChanges.After.Items.ContainsKey(originalItemSpec))
                        {
                            // Design-time builds are not allowed to remove items.
                            continue;
                        }

                        // The item is no longer resolved 
                        changesBuilder.Added(targetFramework, CreateDependencyModelForRule(removedItem, resolvedChanges.After, resolved: false));
                    }
                }

                System.Diagnostics.Debug.Assert(resolvedChanges.Difference.RenamedItems.Count == 0, "Project rule diff should not contain renamed items");

                return;

                IDependencyModel CreateDependencyModelForRule(string itemSpec, IProjectRuleSnapshot projectRuleSnapshot, bool resolved)
                {
                    IImmutableDictionary<string, string> properties = projectRuleSnapshot.GetProjectItemProperties(itemSpec)!;

                    string originalItemSpec = properties.GetStringProperty(ResolvedAssemblyReference.OriginalItemSpecProperty)!;

                    Assumes.NotNull(originalItemSpec); // TODO add XamlRuleTests theory that asserts this

                    bool isImplicit = properties.GetBoolProperty(ProjectItemMetadata.IsImplicitlyDefined) ?? false;

                    return CreateDependencyModel(
                        itemSpec,
                        originalItemSpec,
                        resolved,
                        isImplicit,
                        properties);
                }
            }
        }

        protected virtual IDependencyModel CreateDependencyModel(
            string path,
            string originalItemSpec,
            bool resolved,
            bool isImplicit,
            IImmutableDictionary<string, string> properties)
        {
            // Should be overridden by subclasses, unless they override and replace 'Handle'.
            // Not 'abstract' because a subclass could replace 'Handle', in which case they don't need this method.
            throw new NotImplementedException();
        }

        #endregion

        #region IProjectDependenciesSubTreeProvider

        public abstract string ProviderType { get; }

        public abstract IDependencyModel CreateRootDependencyNode();

        public event EventHandler<DependenciesChangedEventArgs>? DependenciesChanged;

        protected void FireDependenciesChanged(DependenciesChangedEventArgs args)
        {
            DependenciesChanged?.Invoke(this, args);
        }

        #endregion
    }
}
