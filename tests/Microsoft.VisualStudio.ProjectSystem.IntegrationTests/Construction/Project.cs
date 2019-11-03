// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Test.Apex.VisualStudio.Solution;

namespace Microsoft.VisualStudio.ProjectSystem.VS
{
    public interface IProject
    {
        string ProjectName { get; }
        string RelativeProjectFilePath { get; }
        Guid ProjectGuid { get; }
        object ProjectTypeGuid { get; }
        ProjectTestExtension? Extension { get; set; }

        void Save(string solutionRoot);
    }

    public enum OutputType
    {
        Library,
        Exe
    }

    public sealed class LegacyProject : IProject
    {
        private static readonly Guid s_legacyProjectTypeGuid = Guid.Parse("FAE04EC0-301F-11D3-BF4B-00C04F79EFBC");

        public string ProjectName { get; } = "LegacyProject_" + Guid.NewGuid().ToString("N").Substring(0, 12);
        public string ProjectFileName => $"{ProjectName}.csproj";
        public string RelativeProjectFilePath => $"{ProjectName}\\{ProjectName}.csproj";
        public Guid ProjectGuid { get; } = Guid.NewGuid();
        public object ProjectTypeGuid => s_legacyProjectTypeGuid;

        public ProjectTestExtension? Extension { get; set; }

        private readonly OutputType _outputType;
        private readonly string _targetFrameworkMoniker;

        public LegacyProject(OutputType outputType, string targetFrameworkMoniker)
        {
            _outputType = outputType;
            _targetFrameworkMoniker = targetFrameworkMoniker;
        }

        public void Save(string solutionRoot)
        {
            var xml = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<Project ToolsVersion=""15.0"" xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
  <Import Project=""$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props"" Condition=""Exists('$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props')"" />
  <PropertyGroup>
    <Configuration Condition="" '$(Configuration)' == '' "">Debug</Configuration>
    <Platform Condition="" '$(Platform)' == '' "">AnyCPU</Platform>
    <ProjectGuid>{ProjectGuid:D}</ProjectGuid>
    <OutputType>{_outputType}</OutputType>
    <AppDesignerFolder>Properties</AppDesignerFolder>
    <RootNamespace>{ProjectName}</RootNamespace>
    <AssemblyName>{ProjectName}</AssemblyName>
    <TargetFrameworkVersion>{_targetFrameworkMoniker}</TargetFrameworkVersion>
    <FileAlignment>512</FileAlignment>
    <Deterministic>true</Deterministic>
  </PropertyGroup>
  <PropertyGroup Condition="" '$(Configuration)|$(Platform)' == 'Debug|AnyCPU' "">
    <DebugSymbols>true</DebugSymbols>
    <DebugType>full</DebugType>
    <Optimize>false</Optimize>
    <OutputPath>bin\Debug\</OutputPath>
    <DefineConstants>DEBUG;TRACE</DefineConstants>
    <ErrorReport>prompt</ErrorReport>
    <WarningLevel>4</WarningLevel>
  </PropertyGroup>
  <PropertyGroup Condition="" '$(Configuration)|$(Platform)' == 'Release|AnyCPU' "">
    <DebugType>pdbonly</DebugType>
    <Optimize>true</Optimize>
    <OutputPath>bin\Release\</OutputPath>
    <DefineConstants>TRACE</DefineConstants>
    <ErrorReport>prompt</ErrorReport>
    <WarningLevel>4</WarningLevel>
  </PropertyGroup>
  <ItemGroup>
    <Reference Include=""System""/>
    <Reference Include=""System.Core""/>
    <Reference Include=""System.Xml.Linq""/>
    <Reference Include=""System.Data.DataSetExtensions""/>
    <Reference Include=""Microsoft.CSharp""/>
    <Reference Include=""System.Data""/>
    <Reference Include=""System.Net.Http""/>
    <Reference Include=""System.Xml""/>
  </ItemGroup>
<!--
  <ItemGroup>
    <Compile Include=""Class1.cs"" />
    <Compile Include=""Properties\AssemblyInfo.cs"" />
  </ItemGroup>
-->
  <Import Project=""$(MSBuildToolsPath)\Microsoft.CSharp.targets"" />
 </Project>
";
            var projectRoot = Path.Combine(solutionRoot, ProjectName);

            Directory.CreateDirectory(projectRoot);

            File.WriteAllText(Path.Combine(solutionRoot, RelativeProjectFilePath), xml);
        }
    }

    /// <summary>
    /// Defines a <c>.csproj</c> file to be created when using <see cref="ProjectLayoutTestBase"/>.
    /// </summary>
    public sealed class Project : IProject, IEnumerable
    {
        private static readonly Guid s_sdkProjectTypeGuid = Guid.Parse("9A19103F-16F7-4668-BE54-9A1E7A4F7556");

        private List<IProject>? _referencedProjects;
        private List<PackageReference>? _packageReferences;
        private List<AssemblyReference>? _assemblyReferences;
        private List<IFile>? _files;

        public XElement XElement { get; } = new XElement("Project");

        public string Sdk { get; }
        public string TargetFrameworks { get; }

        public string ProjectName { get; } = "Project_" + Guid.NewGuid().ToString("N").Substring(0, 12);
        public string ProjectFileName => $"{ProjectName}.csproj";
        public string RelativeProjectFilePath => $"{ProjectName}\\{ProjectName}.csproj";

        public Guid ProjectGuid { get; } = Guid.NewGuid();
        public object ProjectTypeGuid => s_sdkProjectTypeGuid;

        public ProjectTestExtension? Extension { get; set; }

        public Project(string targetFrameworks, string sdk = "Microsoft.NET.Sdk")
        {
            TargetFrameworks = targetFrameworks;
            Sdk = sdk;
        }

        public void Save(string solutionRoot)
        {
            XElement.Add(new XAttribute("Sdk", Sdk));

            XElement.Add(new XElement(
                "PropertyGroup",
                new XElement("TargetFrameworks", TargetFrameworks)));

            if (_referencedProjects != null)
            {
                XElement.Add(new XElement(
                    "ItemGroup",
                    _referencedProjects.Select(p => new XElement(
                        "ProjectReference",
                        new XAttribute("Include", $"..\\{p.RelativeProjectFilePath}")))));
            }

            if (_assemblyReferences != null)
            {
                XElement.Add(new XElement(
                    "ItemGroup",
                    _assemblyReferences.Select(p => new XElement(
                        "Reference",
                        new XAttribute("Include", p.Name)))));
            }

            if (_packageReferences != null)
            {
                XElement.Add(new XElement(
                    "ItemGroup",
                    _packageReferences.Select(p => new XElement(
                        "PackageReference",
                        new XAttribute("Include", p.PackageId),
                        new XAttribute("Version", p.Version)))));
            }

            var projectRoot = Path.Combine(solutionRoot, ProjectName);

            Directory.CreateDirectory(projectRoot);

            XElement.Save(Path.Combine(solutionRoot, RelativeProjectFilePath));

            if (_files != null)
            {
                foreach (var file in _files)
                {
                    file.Save(projectRoot);
                }
            }
        }

        /// <summary>
        /// Adds a P2P (project-to-project) reference from this project to <paramref name="referee"/>.
        /// </summary>
        /// <param name="referee">The project to reference.</param>
        public void Add(IProject referee)
        {
            _referencedProjects ??= new List<IProject>();
            _referencedProjects.Add(referee);
        }

        public void Add(PackageReference packageReference)
        {
            _packageReferences ??= new List<PackageReference>();
            _packageReferences.Add(packageReference);
        }

        public void Add(AssemblyReference assemblyReference)
        {
            _assemblyReferences ??= new List<AssemblyReference>();
            _assemblyReferences.Add(assemblyReference);
        }

        public void Add(IFile file)
        {
            _files ??= new List<IFile>();
            _files.Add(file);
        }

        /// <summary>
        /// We only implement <see cref="IEnumerable"/> to support collection initialiser syntax.
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator() => throw new NotSupportedException();
    }

    public readonly struct PackageReference
    {
        public string PackageId { get; }
        public string Version { get; }

        public PackageReference(string packageId, string version)
        {
            PackageId = packageId;
            Version = version;
        }
    }

    public readonly struct AssemblyReference
    {
        public string Name { get; }

        public AssemblyReference(string name)
        {
            Name = name;
        }
    }

    public interface IFile
    {
        void Save(string projectRoot);
    }

    public sealed class CSharpClass : IFile
    {
        public string Name { get; }

        public CSharpClass(string name)
        {
            Name = name;
        }

        public void Save(string projectRoot)
        {
            var content = $@"class {Name} {{ }}";

            File.WriteAllText(Path.Combine(projectRoot, $"{Name}.cs"), content);
        }
    }
}
