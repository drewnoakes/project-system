﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.ProjectSystem.Properties;

namespace Microsoft.VisualStudio.ProjectSystem.VS.Properties
{
    /// <summary>
    /// Converts the MSBuild <c>OutputType</c> value expressed as <see cref="VSLangProj.prjOutputType"/>.
    /// </summary>
    [ExportInterceptingPropertyValueProvider("OutputType", ExportInterceptingPropertyValueProviderFile.ProjectFile)]
    internal sealed class OutputTypeValueProvider : OutputTypeValueProviderBase
    {
        private static readonly ImmutableDictionary<string, string> s_getOutputTypeMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            {"WinExe",          "0" },
            {"Exe",             "1" },
            {"Library",         "2" },
            {"WinMDObj",        "2" },
            {"AppContainerExe", "1" },
        }.ToImmutableDictionary();

        private static readonly ImmutableDictionary<string, string> s_setOutputTypeMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            {"0", "WinExe" },
            {"1", "Exe" },
            {"2", "Library" },
        }.ToImmutableDictionary();

        [ImportingConstructor]
        public OutputTypeValueProvider(ProjectProperties properties)
            : base(properties)
        {
        }

        protected override ImmutableDictionary<string, string> GetMap => s_getOutputTypeMap;
        protected override ImmutableDictionary<string, string> SetMap => s_setOutputTypeMap;
        protected override string DefaultGetValue => "0";
    }
}
