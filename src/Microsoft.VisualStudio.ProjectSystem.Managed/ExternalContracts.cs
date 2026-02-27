// Licensed to the .NET Foundation under one or more agreements. The .NET Foundation licenses this file to you under the MIT license. See the LICENSE.md file in the project root for more information.

using Microsoft.VisualStudio.Composition;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.Properties;
using Microsoft.VisualStudio.Threading;

[assembly: ProjectSystemContract(ProjectSystemContractScope.Global, ProjectSystemContractProvider.Private, Cardinality = ImportCardinality.ExactlyOne, ContractType = typeof(JoinableTaskContext))]
[assembly: ProjectSystemContract(ProjectSystemContractScope.Global, ProjectSystemContractProvider.Private, Cardinality = ImportCardinality.ExactlyOne, ContractType = typeof(ICompositionService))]
[assembly: ProjectSystemContract(ProjectSystemContractScope.Global, ProjectSystemContractProvider.Private, Cardinality = ImportCardinality.ExactlyOne, ContractType = typeof(ExportProvider))]

[assembly: ProjectSystemContract(ProjectSystemContractScope.ConfiguredProject, ProjectSystemContractProvider.Extension, ContractType = typeof(IProjectPropertiesProvider), ContractName = "ProjectFileOrAssemblyInfo")]
[assembly: ProjectSystemContract(ProjectSystemContractScope.ConfiguredProject, ProjectSystemContractProvider.Extension, ContractType = typeof(IProjectPropertiesProvider), ContractName = "ProjectFileWithInterception")]
[assembly: ProjectSystemContract(ProjectSystemContractScope.ConfiguredProject, ProjectSystemContractProvider.Extension, ContractType = typeof(IProjectPropertiesProvider), ContractName = "ProjectFileWithInterceptionViaSnapshot")]
[assembly: ProjectSystemContract(ProjectSystemContractScope.ConfiguredProject, ProjectSystemContractProvider.Extension, ContractType = typeof(IProjectPropertiesProvider), ContractName = "UserFileWithInterception")]
[assembly: ProjectSystemContract(ProjectSystemContractScope.ConfiguredProject, ProjectSystemContractProvider.Extension, ContractType = typeof(IProjectPropertiesProvider), ContractName = "UserFileWithXamlDefaultsWithInterception")]
[assembly: ProjectSystemContract(ProjectSystemContractScope.ConfiguredProject, ProjectSystemContractProvider.Extension, ContractType = typeof(IProjectPropertiesProvider), ContractName = "LaunchProfile")]
[assembly: ProjectSystemContract(ProjectSystemContractScope.ConfiguredProject, ProjectSystemContractProvider.Extension, ContractType = typeof(IProjectInstancePropertiesProvider), ContractName = "ProjectFileWithInterception")]
[assembly: ProjectSystemContract(ProjectSystemContractScope.ConfiguredProject, ProjectSystemContractProvider.Extension, ContractType = typeof(IProjectInstancePropertiesProvider), ContractName = "ProjectFileWithInterceptionViaSnapshot")]
[assembly: ProjectSystemContract(ProjectSystemContractScope.ConfiguredProject, ProjectSystemContractProvider.Extension, ContractType = typeof(IProjectInstancePropertiesProvider), ContractName = "UserFileWithInterception")]
[assembly: ProjectSystemContract(ProjectSystemContractScope.ConfiguredProject, ProjectSystemContractProvider.Extension, ContractType = typeof(IProjectInstancePropertiesProvider), ContractName = "UserFileWithXamlDefaultsWithInterception")]

// NOTE these are being added to CPS too, so we should be able to remove these once consuming a newer CPS
[assembly: ProjectSystemContract(ProjectSystemContractScope.UnconfiguredProject, ProjectSystemContractProvider.Extension, ContractType = typeof(IProjectTreeProvider), ContractName = ExportContractNames.ProjectTreeProviders.PhysicalViewRootGraft)]
[assembly: ProjectSystemContract(ProjectSystemContractScope.UnconfiguredProject, ProjectSystemContractProvider.Host, ContractType = typeof(IProjectTreeProvider), ContractName = ExportContractNames.ProjectTreeProviders.PhysicalProjectTreeService)]
[assembly: ProjectSystemContract(ProjectSystemContractScope.UnconfiguredProject, ProjectSystemContractProvider.Host, ContractType = typeof(IProjectTreeProvider), ContractName = ExportContractNames.ProjectTreeProviders.PhysicalViewTree)]
