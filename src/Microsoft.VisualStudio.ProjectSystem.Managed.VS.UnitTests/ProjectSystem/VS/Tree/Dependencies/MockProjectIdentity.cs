// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.VisualStudio.ProjectSystem.VS.Tree.Dependencies
{
    internal sealed class MockProjectIdentity : IProjectIdentity
    {
        public int Id { get; }
        public string CurrentProjectPath { get; }

        public MockProjectIdentity(int id)
        {
            Id = id;
            CurrentProjectPath = $"MockProject{id}";
        }

        public bool Equals(IProjectIdentity other)
        {
            return !(other is null) && other.Id == Id;
        }
    }
}
