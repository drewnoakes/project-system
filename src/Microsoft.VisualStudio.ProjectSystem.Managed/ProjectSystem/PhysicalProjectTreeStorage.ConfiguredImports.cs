// Licensed to the .NET Foundation under one or more agreements. The .NET Foundation licenses this file to you under the MIT license. See the LICENSE.md file in the project root for more information.

namespace Microsoft.VisualStudio.ProjectSystem;

internal partial class PhysicalProjectTreeStorage
{
    [Export]
    [ProjectSystemContract(ProjectSystemContractScope.ConfiguredProject, ProjectSystemContractProvider.Private)]
    internal class ConfiguredImports
    {
        public readonly IFolderManager FolderManager;
        public readonly IProjectItemProvider SourceItemsProvider;

        [ImportingConstructor]
#pragma warning disable CPS011 // ImportMany required for ZeroOrMore cardinality
        public ConfiguredImports(IFolderManager folderManager, [Import(ExportContractNames.ProjectItemProviders.SourceFiles)] IProjectItemProvider sourceItemsProvider)
#pragma warning restore CPS011 // ImportMany required for ZeroOrMore cardinality
        {
            FolderManager = folderManager;
            SourceItemsProvider = sourceItemsProvider;
        }
    }
}
