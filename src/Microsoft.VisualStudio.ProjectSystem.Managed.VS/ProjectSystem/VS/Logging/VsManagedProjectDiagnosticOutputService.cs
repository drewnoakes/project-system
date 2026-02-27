// Licensed to the .NET Foundation under one or more agreements. The .NET Foundation licenses this file to you under the MIT license. See the LICENSE.md file in the project root for more information.

namespace Microsoft.VisualStudio.ProjectSystem.VS.Logging;

#pragma warning disable RS0030 // Do not used banned APIs

/// <summary>
///   An implementation of the <see cref="IManagedProjectDiagnosticOutputService"/> that
///   delegates to the CPS <see cref="IProjectDiagnosticOutputService"/>.
/// </summary>
/// <remarks>
///   Note <see cref="IProjectDiagnosticOutputService"/> has been banned in order to
///   encourage the use of the more widely available <see cref="IManagedProjectDiagnosticOutputService"/>,
///   not for any technical reason.
/// </remarks>
[Export(typeof(IManagedProjectDiagnosticOutputService))]
[AppliesTo(ProjectCapability.DotNet)]
[method: ImportingConstructor]
internal class VsManagedProjectDiagnosticOutputService(IProjectDiagnosticOutputService projectDiagnosticOutputService)
    : IManagedProjectDiagnosticOutputService
{
    public bool IsEnabled => projectDiagnosticOutputService.IsEnabled;

    public void WriteLine(string outputMessage) => projectDiagnosticOutputService.WriteLine(outputMessage);
}
