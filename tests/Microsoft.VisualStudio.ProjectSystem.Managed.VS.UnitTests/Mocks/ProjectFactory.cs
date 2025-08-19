// Licensed to the .NET Foundation under one or more agreements. The .NET Foundation licenses this file to you under the MIT license. See the LICENSE.md file in the project root for more information.

namespace EnvDTE;

internal static class ProjectFactory
{
    public static Project ImplementObject(Func<object> action)
    {
        var mock = new Mock<Project>();
        mock.SetupGet(p => p.Object)
            .Returns(action);

        return mock.Object;
    }
}
