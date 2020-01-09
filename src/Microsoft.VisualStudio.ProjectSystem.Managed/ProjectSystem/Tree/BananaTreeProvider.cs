// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Microsoft.VisualStudio.ProjectSystem.VS;
using Microsoft.VisualStudio.Threading;

namespace Microsoft.VisualStudio.ProjectSystem.Tree
{
    [Export(ExportContractNames.ProjectTreeProviders.PhysicalViewRootGraft, typeof(IProjectTreeProvider))]
    [AppliesTo(ProjectCapability.AlwaysAvailable)]
    internal sealed class BananaTreeProvider : ProjectTreeProviderBase
    {
        private static readonly ProjectImageMoniker s_rootIcon = ManagedImageMonikers.Sound.ToProjectSystemType();
        //private static readonly ProjectImageMoniker s_nodeIcon = ManagedImageMonikers.TargetFile.ToProjectSystemType();
        //private static readonly ProjectImageMoniker s_nodeImplicitIcon = ManagedImageMonikers.TargetFilePrivate.ToProjectSystemType();

        public static ProjectTreeFlags ProjectImport { get; } = ProjectTreeFlags.Create("ProjectImport");
        public static ProjectTreeFlags ProjectImportImplicit { get; } = ProjectTreeFlags.Create("ProjectImportImplicit");

        private static readonly ProjectTreeFlags s_rootFlags = ProjectTreeFlags.Create(
            ProjectTreeFlags.Common.BubbleUp |              // sort to top of tree, not alphabetically
            ProjectTreeFlags.Common.VirtualFolder |
            ProjectTreeFlags.Common.DisableAddItemFolder) + 
            ProjectTreeFlags.Create("BananaFlag");

//        private static readonly ProjectTreeFlags s_projectImportFlags = ProjectImport | ProjectTreeFlags.FileOnDisk;
//        private static readonly ProjectTreeFlags s_projectImportImplicitFlags = s_projectImportFlags + ProjectImportImplicit;

//        private readonly IActiveConfiguredProjectSubscriptionService _projectSubscriptionService;
        private readonly IUnconfiguredProjectTasksService _unconfiguredProjectTasksService;
        private readonly IProjectThreadingService _threadService;

        [ImportingConstructor]
        internal BananaTreeProvider(
            IProjectThreadingService threadingService,
//            IActiveConfiguredProjectSubscriptionService projectSubscriptionService,
            IUnconfiguredProjectTasksService unconfiguredProjectTasksService,
            IProjectThreadingService threadService,
            UnconfiguredProject unconfiguredProject)
            : base(threadingService, unconfiguredProject)
        {
//            _projectSubscriptionService = projectSubscriptionService;
            _unconfiguredProjectTasksService = unconfiguredProjectTasksService;
            _threadService = threadService;
        }

        protected override void Initialize()
        {
            base.Initialize();

            _threadService.JoinableTaskFactory.RunAsync(async () => 
                await _unconfiguredProjectTasksService.LoadedProjectAsync(
                    async () =>
                    {
                        await TaskScheduler.Default.SwitchTo(alwaysYield: true);

                        _unconfiguredProjectTasksService.UnloadCancellationToken.ThrowIfCancellationRequested();

                        lock (SyncObject)
                        {
                            Verify.NotDisposed(this);

                            _ = SubmitTreeUpdateAsync(
                                (currentTree, configuredProjectExports1, token) =>
                                {
                                    // Update (make visible) or create a new tree if no prior one exists
                                    IProjectTree tree = currentTree == null
                                        ? NewTree("Banana", icon: s_rootIcon, flags: s_rootFlags)
                                        : currentTree.Value.Tree.SetVisible(true);

                                    return Task.FromResult(new TreeUpdateResult(tree));
                                });
                        }
                    }));
        }

//        protected override void Dispose(bool disposing)
//        {
//            if (disposing)
//            {
//                _subscriptions?.Dispose();
//            }
//
//            base.Dispose(disposing);
//        }

        protected override ConfiguredProjectExports GetActiveConfiguredProjectExports(ConfiguredProject newActiveConfiguredProject)
        {
            Requires.NotNull(newActiveConfiguredProject, nameof(newActiveConfiguredProject));

            return GetActiveConfiguredProjectExports<MyConfiguredProjectExports>(newActiveConfiguredProject);
        }

        [Export]
        private sealed class MyConfiguredProjectExports : ConfiguredProjectExports
        {
            [ImportingConstructor]
            public MyConfiguredProjectExports(ConfiguredProject configuredProject)
                : base(configuredProject)
            {
            }
        }
    }
}
