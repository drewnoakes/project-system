// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;

using Microsoft.VisualStudio.GraphModel;
using Microsoft.VisualStudio.GraphModel.Schemas;
using Microsoft.VisualStudio.ProjectSystem.VS.Tree.Dependencies.GraphNodes;

namespace Microsoft.VisualStudio.ProjectSystem.VS.Tree.Dependencies
{
    internal static class GraphNodeIdExtensions
    {
        public static IProjectIdentity GetProjectId(this GraphNodeId id)
        {
            return id.GetNestedValueByName<IProjectIdentity>(DependenciesGraphSchema.ProjectIdName);
        }

        public static string GetDependencyModelId(this GraphNodeId id)
        {
            return id.GetNestedValueByName<string>(DependenciesGraphSchema.DependencyModelIdName);
        }

        public static string GetAssemblyPath(this GraphNodeId id)
        {
            Uri uri = id.GetNestedValueByName<Uri>(CodeGraphNodeIdName.Assembly);
            return (uri.IsAbsoluteUri ? uri.LocalPath : uri.ToString()).Trim('/');
        }

/*
        public static string GetDependencyId(this GraphNode id)
        {
            return id.GetValue<string>(DependenciesGraphSchema.DependencyIdProperty);
        }
*/

//        internal static string GetValue(this GraphNodeId id, GraphNodeIdName idPartName)
//        {
//            if (idPartName == CodeGraphNodeIdName.Assembly || idPartName == CodeGraphNodeIdName.File)
//            {
//                try
//                {
//                    Uri value = id.GetNestedValueByName<Uri>(idPartName);
//
//                    // for idPartName == CodeGraphNodeIdName.File it can be null, avoid unnecessary exception
//                    if (value == null)
//                    {
//                        return null;
//                    }
//
//                    // Assembly and File are represented by a Uri, extract LocalPath string from Uri
//                    return (value.IsAbsoluteUri ? value.LocalPath : value.ToString()).Trim('/');
//                }
//                catch
//                {
//                    // for some node ids Uri might throw format exception, thus try to get string at least
//                    return id.GetNestedValueByName<string>(idPartName);
//                }
//            }
//            else
//            {
//                return id.GetNestedValueByName<string>(idPartName);
//            }
//        }
    }
}
