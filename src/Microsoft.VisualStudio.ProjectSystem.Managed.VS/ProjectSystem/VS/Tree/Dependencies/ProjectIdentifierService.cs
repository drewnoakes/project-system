// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.ComponentModel.Composition;
using System.Threading.Tasks;

namespace Microsoft.VisualStudio.ProjectSystem.VS.Tree.Dependencies
{
    [Export(typeof(IProjectIdentityService))]
    [AppliesTo(ProjectCapability.DependenciesTree)]
    internal sealed class ProjectIdentityService : IProjectIdentityService
    {
        private readonly object _lock = new object();

        private ImmutableDictionary<string, int> _idByPath = ImmutableDictionary.Create<string, int>(StringComparers.Paths);
        private ImmutableDictionary<int, IProjectIdentity> _identityById = ImmutableDictionary.Create<int, IProjectIdentity>();
        private ImmutableDictionary<int, string> _pathById = ImmutableDictionary<int, string>.Empty;

        private int _nextId = 1;

        public int Register(IProjectIdentity projectId, UnconfiguredProject project)
        {
            lock (_lock)
            {
                string path = project.FullPath;

                if (!_idByPath.TryGetValue(path, out int id))
                {
                    id = _nextId++;
                }

                if (_identityById.ContainsKey(id))
                {
                    throw new ArgumentException("Already registered.", nameof(project));
                }

                _identityById = _identityById.Add(id, projectId);
                _idByPath = _idByPath.SetItem(path, id);
                _pathById = _pathById.SetItem(id, path);

                project.ProjectRenamed += OnUnconfiguredProjectRenamedAsync;

                return id;
            }
        }

        public void Unregister(IProjectIdentity projectId, UnconfiguredProject project)
        {
            lock (_lock)
            {
                _identityById = _identityById.Remove(projectId.Id);
                project.ProjectRenamed -= OnUnconfiguredProjectRenamedAsync;
            }
        }

        private Task OnUnconfiguredProjectRenamedAsync(object sender, ProjectRenamedEventArgs e)
        {
            lock (_lock)
            {
                string oldPath = e.OldFullPath;
                string newPath = e.NewFullPath;

                if (!_idByPath.TryGetValue(oldPath, out int id))
                {
                    System.Diagnostics.Debug.Fail("Renamed project's old path not known");
                }
                else
                {
                    _idByPath = _idByPath.SetItem(newPath, id);
                    _pathById = _pathById.SetItem(id, newPath);
                }
            }

            return Task.CompletedTask;
        }

        public bool TryGetProjectPath(int id, out string projectPath)
        {
            return _pathById.TryGetValue(id, out projectPath);
        }

        public bool TryGetProjectId(string projectPath, out IProjectIdentity projectId)
        {
            if (_idByPath.TryGetValue(projectPath, out int id) &&
                _identityById.TryGetValue(id, out projectId))
            {
                return true;
            }

            projectId = default;
            return false;
        }
    }

    internal interface IProjectIdentityService
    {
        int Register(IProjectIdentity projectId, UnconfiguredProject commonServicesProject);

        void Unregister(IProjectIdentity projectId, UnconfiguredProject commonServicesProject);

        bool TryGetProjectPath(int id, out string projectPath);

        bool TryGetProjectId(string projectPath, out IProjectIdentity projectId);
    }

    [Export(typeof(IProjectIdentity))]
    [AppliesTo(ProjectCapability.DependenciesTree)]
    internal sealed class ProjectIdentity : OnceInitializedOnceDisposed, IProjectIdentity
    {
        private readonly IUnconfiguredProjectCommonServices _commonServices;
        private readonly IProjectIdentityService _identityService;

        public int Id { get; }

        [ImportingConstructor]
        public ProjectIdentity(IUnconfiguredProjectCommonServices commonServices, IProjectIdentityService identityService)
        {
            _commonServices = commonServices;
            _identityService = identityService;

            Id = _identityService.Register(this, commonServices.Project);

            EnsureInitialized();
        }

        public string CurrentProjectPath => _identityService.TryGetProjectPath(Id, out string path) ? path : null;

        protected override void Initialize()
        {
        }

        protected override void Dispose(bool disposing)
        {
            _identityService.Unregister(this, _commonServices.Project);
        }

        public override string ToString() => CurrentProjectPath;


        public bool Equals(IProjectIdentity other) => !(other is null) && Id == other.Id;
        public override bool Equals(object obj) => obj is ProjectIdentity other && Id == other.Id;
        public override int GetHashCode() => Id;
    }

    internal interface IProjectIdentity : IEquatable<IProjectIdentity>
    {
        int Id { get; }
        string CurrentProjectPath { get; }
    }
}
