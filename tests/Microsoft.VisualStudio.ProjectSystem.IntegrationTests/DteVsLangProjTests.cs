// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.VisualStudio.ProjectSystem.VS
{
    [TestClass]
    public sealed class DteVsLangProjTests : ProjectLayoutTestBase
    {
        [TestMethod]
        public void DteComparison_ProjectProperties()
        {
            var legacy = new LegacyProject(OutputType.Library, "v4.7.2");

            var project = new Project("net472");

            var solution = new Solution
            {
                project,
                legacy
            };

            CreateSolution(solution);

            using (Scope.Enter("Verify DTE project properties"))
            {
                var legacyItem = VisualStudio.Dte.Solution.Item(legacy.RelativeProjectFilePath);
                var projectItem = VisualStudio.Dte.Solution.Item(project.RelativeProjectFilePath);

                var sb = new StringBuilder();

                var propertyNames = legacyItem.Properties.OfType<EnvDTE.Property>().Select(p => p.Name)
                    .Union(projectItem.Properties.OfType<EnvDTE.Property>().Select(p => p.Name))
                    .OrderBy(name => name, StringComparer.OrdinalIgnoreCase);

                foreach (string name in propertyNames)
                {
                    var legacyValue = TryGetValue(legacyItem, name);
                    var projectValue = TryGetValue(projectItem, name);

                    if (Equals(legacyValue, projectValue))
                    {
                        sb.AppendLine($"✔ {name} = {FormatValue(legacyValue)}");
                    }
                    else if (legacyValue is null)
                    {
                        sb.AppendLine($"⚠ {name}");
                        sb.AppendLine($"    legacy  = {FormatValue(legacyValue)}");
                        sb.AppendLine($"    project = {FormatValue(projectValue)}");
                    }
                    else
                    {
                        sb.AppendLine($"❌ {name}");
                        sb.AppendLine($"    legacy  = {FormatValue(legacyValue)}");
                        sb.AppendLine($"    project = {FormatValue(projectValue)}");
                    }
                }

                Console.WriteLine(sb);
            }

            static string FormatValue(object? o)
            {
                if (o is null)
                {
                    return "null";
                }

                if (o is string)
                {
                    return $"\"{o}\"";
                }

                if (o is true)
                {
                    return "true";
                }

                if (o is false)
                {
                    return "false";
                }

                var type = o switch
                {
                    int _ => "int",
                    uint _ => "uint",
                    _ => o.GetType().Name
                };

                return $"{o} ({type})";
            }

            static object? TryGetValue(EnvDTE.Project property, string name)
            {
                try
                {
                    var item = property.Properties.Item(name);
                    return item?.Value;
                }
                catch
                {
                    return null;
                }
            }
        }
    }
}
