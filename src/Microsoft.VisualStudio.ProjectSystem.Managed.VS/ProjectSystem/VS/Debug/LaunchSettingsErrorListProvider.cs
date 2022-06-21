// Licensed to the .NET Foundation under one or more agreements. The .NET Foundation licenses this file to you under the MIT license. See the LICENSE.md file in the project root for more information.

using Microsoft.VisualStudio.ProjectSystem.Debug;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Threading;

namespace Microsoft.VisualStudio.ProjectSystem.VS.Debug;

[Export(typeof(ILaunchSettingsErrorListProvider))]
internal class LaunchSettingsErrorListProvider : ILaunchSettingsErrorListProvider, IDisposable
{
    private readonly Dictionary<UnconfiguredProject, ErrorTask> _errorByProject = new();

    private readonly ErrorListProvider _errorListProvider;
    private readonly IVsUIService<SVsUIShellOpenDocument, IVsUIShellOpenDocument> _uiShellOpenDocument;
    private readonly IVsUIService<VsTextManagerClass, IVsTextManager> _vsTextManager;
    private readonly JoinableTaskContext _joinableTaskContext;

    [ImportingConstructor]
    public LaunchSettingsErrorListProvider(
        [Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider,
        IVsUIService<SVsUIShellOpenDocument, IVsUIShellOpenDocument> uiShellOpenDocument,
        IVsUIService<VsTextManagerClass, IVsTextManager> vsTextManager,
        JoinableTaskContext joinableTaskContext)
    {
        _uiShellOpenDocument = uiShellOpenDocument;
        _vsTextManager = vsTextManager;
        _joinableTaskContext = joinableTaskContext;
        _errorListProvider = new ErrorListProvider(serviceProvider)
        {
            AlwaysVisible = true,
            ProviderName = "Launch Settings",
            ProviderGuid = new Guid("6585E562-4132-400F-BA67-D1E9E0B903BC"),
        };
    }

    public void ClearErrors(UnconfiguredProject project)
    {
        lock (_errorByProject)
        {
            if (_errorByProject.TryGetValue(project, out ErrorTask? error))
            {
                _errorListProvider.Tasks.Remove(error);
                _errorByProject.Remove(project);
            }
        }
    }

    public void SetError(Exception ex, string fileName, UnconfiguredProject project)
    {
        lock (_errorByProject)
        {
            if (_errorByProject.TryGetValue(project, out ErrorTask? error))
            {
                error.Text = "Error parsing launch settings: " + ex.Message;
            }
            else
            {
                error = new ErrorTask
                {
                    Text = ex.Message,
                    CanDelete = false,
                    Checked = false,
                    Document = fileName,
                    Priority = TaskPriority.High,
                };

                if (ex is Newtonsoft.Json.JsonReaderException je)
                {
                    error.Line = je.LineNumber;
                    error.Column = je.LinePosition;
                }

                error.Navigate += OnNavigate;

                _errorByProject.Add(project, error);
                _errorListProvider.Tasks.Add(error);
            }

            void OnNavigate(object sender, EventArgs arguments)
            {
                _joinableTaskContext.VerifyIsOnMainThread();

                Guid logicalView = VSConstants.LOGVIEWID_Code;

                if (ErrorHandler.Failed(_uiShellOpenDocument.Value.OpenDocumentViaProject(
                    fileName,
                    ref logicalView,
                    out OLE.Interop.IServiceProvider sp,
                    out IVsUIHierarchy hier,
                    out uint itemid,
                    out IVsWindowFrame frame))
                    || frame is null)
                {
                    return;
                }

                frame.GetProperty((int)__VSFPROPID.VSFPROPID_DocData, out object docData);

                var buffer = docData as VsTextBuffer;
                if (buffer == null)
                {
                    if (docData is IVsTextBufferProvider bufferProvider)
                    {
                        ErrorHandler.ThrowOnFailure(bufferProvider.GetTextBuffer(out IVsTextLines lines));
                        buffer = lines as VsTextBuffer;
                        if (buffer == null)
                        {
                            return;
                        }
                    }
                }

                _vsTextManager.Value.NavigateToLineAndColumn(buffer, ref logicalView, error.Line - 1, error.Column, error.Line - 1, error.Column);
            }
        }
    }

    public void Dispose()
    {
        _errorListProvider.Dispose();

        lock (_errorByProject)
        {
            _errorByProject.Clear();
        }
    }
}
