// Licensed to the .NET Foundation under one or more agreements. The .NET Foundation licenses this file to you under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Immutable;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Composition;

namespace Microsoft.VisualStudio.ProjectSystem.Tree.Dependencies.CrossTarget
{
    /// <summary>
    ///     Creates <see cref="AggregateCrossTargetProjectContext"/> instances based on the
    ///     current <see cref="UnconfiguredProject"/>.
    /// </summary>
    [Export(typeof(AggregateCrossTargetProjectContextProvider))]
    [ProjectSystemContract(ProjectSystemContractScope.UnconfiguredProject, ProjectSystemContractProvider.Private, Cardinality = ImportCardinality.ExactlyOne)]
    internal class AggregateCrossTargetProjectContextProvider
    {
        private readonly IUnconfiguredProjectCommonServices _commonServices;
        private readonly IActiveConfiguredProjectsProvider _activeConfiguredProjectsProvider;

        [ImportingConstructor]
        public AggregateCrossTargetProjectContextProvider(
            IUnconfiguredProjectCommonServices commonServices,
            IActiveConfiguredProjectsProvider activeConfiguredProjectsProvider)
        {
            _commonServices = commonServices;
            _activeConfiguredProjectsProvider = activeConfiguredProjectsProvider;
        }

        /// <summary>
        ///     Creates a <see cref="AggregateCrossTargetProjectContext"/>.
        /// </summary>
        /// <returns>
        ///     The created <see cref="AggregateCrossTargetProjectContext"/>.
        /// </returns>
        public async Task<AggregateCrossTargetProjectContext> CreateProjectContextAsync()
        {
            // Get the set of active configured projects ignoring target framework.
            ActiveConfiguredObjects<ConfiguredProject>? activeConfiguredProjects = await _activeConfiguredProjectsProvider.GetActiveConfiguredProjectsAsync();

            if (activeConfiguredProjects == null)
            {
                throw new InvalidOperationException("There are no active configured projects.");
            }

            ProjectConfiguration activeProjectConfiguration = _commonServices.ActiveConfiguredProject.ProjectConfiguration;
            ImmutableArray<TargetFramework>.Builder targetFrameworks = ImmutableArray.CreateBuilder<TargetFramework>(initialCapacity: activeConfiguredProjects.Objects.Length);
            ImmutableDictionary<TargetFramework, ConfiguredProject>.Builder configuredProjectByTargetFramework = ImmutableDictionary.CreateBuilder<TargetFramework, ConfiguredProject>();
            TargetFramework activeTargetFramework = TargetFramework.Empty;

            foreach (ConfiguredProject configuredProject in activeConfiguredProjects.Objects)
            {
                TargetFramework targetFramework = await GetTargetFrameworkAsync(configuredProject);

                configuredProjectByTargetFramework.Add(targetFramework, configuredProject);

                targetFrameworks.Add(targetFramework);

                if (activeTargetFramework.Equals(TargetFramework.Empty) &&
                    configuredProject.ProjectConfiguration.Equals(activeProjectConfiguration))
                {
                    activeTargetFramework = targetFramework;
                }
            }

            // TODO validate this check...
            bool isCrossTargeting = activeConfiguredProjects.DimensionNames.Contains(ConfigurationGeneral.TargetFrameworkProperty);

            return new AggregateCrossTargetProjectContext(
                isCrossTargeting,
                targetFrameworks.MoveToImmutable(),
                configuredProjectByTargetFramework.ToImmutable(),
                activeTargetFramework);
        }

        private static async Task<TargetFramework> GetTargetFrameworkAsync(ConfiguredProject configuredProject)
        {
            ProjectProperties projectProperties = configuredProject.Services.ExportProvider.GetExportedValue<ProjectProperties>();
            ConfigurationGeneral configurationGeneralProperties = await projectProperties.GetConfigurationGeneralPropertiesAsync();

            string? targetFrameworkAlias = (await configurationGeneralProperties.TargetFramework.GetValueAsync())?.ToString();

            if (targetFrameworkAlias == null)
            {
                return TargetFramework.Empty;
            }

            string? targetFrameworkMoniker = (await configurationGeneralProperties.TargetFrameworkMoniker.GetValueAsync())?.ToString();
            string? targetFrameworkIdentifier = (await configurationGeneralProperties.TargetFrameworkIdentifier.GetValueAsync())?.ToString();
            string? targetFrameworkVersion = (await configurationGeneralProperties.TargetFrameworkVersion.GetValueAsync())?.ToString();
            string? targetFrameworkProfile = (await configurationGeneralProperties.TargetFrameworkProfile.GetValueAsync())?.ToString();
            string? targetPlatformIdentifier = (await configurationGeneralProperties.TargetPlatformIdentifier.GetValueAsync())?.ToString();
            string? targetPlatformVersion = (await configurationGeneralProperties.TargetPlatformVersion.GetValueAsync())?.ToString();

            return new TargetFramework(
                targetFrameworkAlias,
                targetFrameworkMoniker,
                targetFrameworkIdentifier,
                targetFrameworkVersion,
                targetFrameworkProfile,
                targetPlatformIdentifier,
                targetPlatformVersion);
        }
    }
}
