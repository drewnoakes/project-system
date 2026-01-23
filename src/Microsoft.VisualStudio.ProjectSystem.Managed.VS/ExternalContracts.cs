// Licensed to the .NET Foundation under one or more agreements. The .NET Foundation licenses this file to you under the MIT license. See the LICENSE.md file in the project root for more information.

using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.LanguageServices.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using NuGet.SolutionRestoreManager;
using NuGet.VisualStudio;
using VSLangProj;

#pragma warning disable RS0030 // Do not used banned APIs

[assembly: ProjectSystemContract(ProjectSystemContractScope.Global, ProjectSystemContractProvider.Private, Cardinality = ImportCardinality.ExactlyOne, ContractType = typeof(SVsDesignTimeAssemblyResolution))]
[assembly: ProjectSystemContract(ProjectSystemContractScope.Global, ProjectSystemContractProvider.Private, Cardinality = ImportCardinality.ExactlyOne, ContractType = typeof(SVsServiceProvider))]
[assembly: ProjectSystemContract(ProjectSystemContractScope.Global, ProjectSystemContractProvider.Private, Cardinality = ImportCardinality.ExactlyOne, ContractType = typeof(SAsyncServiceProvider))]
[assembly: ProjectSystemContract(ProjectSystemContractScope.Global, ProjectSystemContractProvider.Private, Cardinality = ImportCardinality.ExactlyOne, ContractType = typeof(VisualStudioWorkspace))]
[assembly: ProjectSystemContract(ProjectSystemContractScope.Global, ProjectSystemContractProvider.Private, Cardinality = ImportCardinality.ExactlyOne, ContractType = typeof(ICodeModelFactory))]
[assembly: ProjectSystemContract(ProjectSystemContractScope.Global, ProjectSystemContractProvider.Private, Cardinality = ImportCardinality.ExactlyOne, ContractType = typeof(IVsSolutionRestoreService3))]
[assembly: ProjectSystemContract(ProjectSystemContractScope.Global, ProjectSystemContractProvider.Private, Cardinality = ImportCardinality.ExactlyOne, ContractType = typeof(IVsFrameworkCompatibility))]
[assembly: ProjectSystemContract(ProjectSystemContractScope.Global, ProjectSystemContractProvider.Private, Cardinality = ImportCardinality.ExactlyOne, ContractType = typeof(IVsSolutionRestoreService4))]
[assembly: ProjectSystemContract(ProjectSystemContractScope.Global, ProjectSystemContractProvider.Private, Cardinality = ImportCardinality.ExactlyOne, ContractType = typeof(IWorkspaceProjectContextFactory))]

[assembly: ProjectSystemContract(ProjectSystemContractScope.UnconfiguredProject, ProjectSystemContractProvider.Private, ContractType = typeof(BuildManager))]
[assembly: ProjectSystemContract(ProjectSystemContractScope.UnconfiguredProject, ProjectSystemContractProvider.Extension, ContractType = typeof(Imports))]
[assembly: ProjectSystemContract(ProjectSystemContractScope.UnconfiguredProject, ProjectSystemContractProvider.Extension, ContractType = typeof(ImportsEvents))]
[assembly: ProjectSystemContract(ProjectSystemContractScope.UnconfiguredProject, ProjectSystemContractProvider.Extension, ContractType = typeof(VSProjectEvents))]

[assembly: ProjectSystemContract(ProjectSystemContractScope.UnconfiguredProject, ProjectSystemContractProvider.Extension, ContractType = typeof(VSProject), ContractName = ExportContractNames.VsTypes.VSProject)]
