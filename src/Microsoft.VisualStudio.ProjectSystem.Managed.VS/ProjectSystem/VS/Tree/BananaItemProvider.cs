// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using Microsoft.Internal.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;
using Task = System.Threading.Tasks.Task;

namespace Microsoft.VisualStudio.ProjectSystem.VS.Tree
{
    [Export(typeof(IAttachedCollectionSourceProvider))]
    [Name("BananaItemProvider")]
    [VisualStudio.Utilities.Order(Before = HierarchyItemsProviderNames.Contains)]
    internal sealed class BananaItemProvider : AttachedCollectionSourceProvider<IVsHierarchyItem>
    {
        protected override IAttachedCollectionSource? CreateCollectionSource(IVsHierarchyItem item, string relationshipName)
        {
            if (relationshipName == KnownRelationships.Contains && 
                item?.HierarchyIdentity?.NestedHierarchy != null)
            {
                IVsHierarchy hierarchy = item.HierarchyIdentity.NestedHierarchy;
                uint itemId = item.HierarchyIdentity.NestedItemID;

                ImmutableArray<string> projectTreeCapabilities = GetProjectTreeCapabilities(hierarchy, itemId);

                // TODO do this without allocation
                if (projectTreeCapabilities.Any(c => c == "BananaFlag"))
                {
                    return CreateCollectionSourceCore(item.Parent, item);
                }
            }

            return null;
        }

        private static ImmutableArray<string> GetProjectTreeCapabilities(IVsHierarchy hierarchy, uint itemId)
        {
            if (hierarchy.GetProperty(itemId, (int)__VSHPROPID7.VSHPROPID_ProjectTreeCapabilities, out object capabilitiesObj) == HResult.OK)
            {
                string capabilitiesString = (string)capabilitiesObj;
                return ImmutableArray.CreateRange(new LazyStringSplit(capabilitiesString, ' '));
            }
            else
            {
                return ImmutableArray<string>.Empty;
            }
        }

        // Avoid inlining to avoid type load unless necessary
        [MethodImpl(MethodImplOptions.NoInlining)]
        private IAttachedCollectionSource? CreateCollectionSourceCore(IVsHierarchyItem parentItem, IVsHierarchyItem item)
        {
//            var hierarchyMapper = TryGetProjectMap();
//            if (hierarchyMapper != null &&
//                hierarchyMapper.TryGetProjectId(parentItem, targetFrameworkMoniker: null, projectId: out var projectId))
//            {
//                var workspace = TryGetWorkspace();
//                return new AnalyzersFolderItemSource(workspace, projectId, item, _commandHandler);
//            }

            return new BananaItemSource(item);
        }

/*
        private Workspace? TryGetWorkspace()
        {
            if (_workspace == null)
            {
                var provider = _componentModel.DefaultExportProvider.GetExportedValueOrDefault<ISolutionExplorerWorkspaceProvider>();
                if (provider != null)
                {
                    _workspace = provider.GetWorkspace();
                }
            }

            return _workspace;
        }

        private IHierarchyItemToProjectIdMap? TryGetProjectMap()
        {
            var workspace = TryGetWorkspace();
            if (workspace == null)
            {
                return null;
            }

            if (_projectMap == null)
            {
                _projectMap = workspace.Services.GetService<IHierarchyItemToProjectIdMap>();
            }

            return _projectMap;
        }
*/
    }

    internal sealed class BananaItemSource : IAttachedCollectionSource
    {
        private readonly IVsHierarchyItem _projectHierarchyItem;
        private readonly ObservableCollection<BananaItem> _folderItems;

        public BananaItemSource(IVsHierarchyItem projectHierarchyItem)
        {
            _projectHierarchyItem = projectHierarchyItem;

            _folderItems = new ObservableCollection<BananaItem>
            {
                new BananaItem(_projectHierarchyItem)
            };
        }

        public bool HasItems => true;

        public IEnumerable Items => _folderItems;

        public object SourceItem => _projectHierarchyItem;
    }

    internal sealed class BananaItem : BaseItem
    {
        public BananaItem(
            IVsHierarchyItem parentItem
//            IContextMenuController contextMenuController
            )
            : base("Baby Bananas!")
        {
            ParentItem = parentItem;
//            ContextMenuController = contextMenuController;
        }

        public override ImageMoniker IconMoniker => KnownMonikers.CodeInformation;

        public override ImageMoniker ExpandedIconMoniker => KnownMonikers.CodeInformation;

        public override ImageMoniker OverlayIconMoniker => KnownMonikers.OverlayLock;

        public override ImageMoniker StateIconMoniker => KnownMonikers.CheckedIn;

        public IVsHierarchyItem ParentItem { get; }

//        public override IContextMenuController? ContextMenuController { get; }

        public override object GetBrowseObject() => new BrowseObject(this);

        internal class BrowseObject : BrowseObjectBase
        {
            public BrowseObject(BananaItem bananaItem) => Item = bananaItem;

            public override string GetClassName() => "Class Name";

            public override string GetComponentName() => "Component Name";

            //[BrowseObjectDisplayName("Color")]
            [System.ComponentModel.DisplayName("Color")]
            [Description("What hue does your banana have?")]
            public string Color { get; } = "Yellow";

            //[BrowseObjectDisplayName("Length")]
            [System.ComponentModel.DisplayName("Length")]
            [Description("How long is your banana?")]
            public double Length { get; set; } = 1.23;

            [Browsable(false)]
            public BananaItem Item { get; }
        }
    }

    [Export(typeof(IAttachedCollectionSourceProvider))]
    [Name("BananaBabyItemProvider")]
    [VisualStudio.Utilities.Order]
    internal sealed class BananaBabyItemProvider : AttachedCollectionSourceProvider<BananaItem>
    {
        protected override IAttachedCollectionSource? CreateCollectionSource(BananaItem bananaItem, string relationshipName)
        {
            if (relationshipName == KnownRelationships.Contains)
            {
                return new BananaBabyItemSource(bananaItem);
            }

            return null;
        }
    }

    internal sealed class BananaBabyItemSource : IAttachedCollectionSource
    {
        private readonly BananaItem _bananaItem;
        private readonly ObservableCollection<BananaBabyItem> _folderItems;

        public BananaBabyItemSource(BananaItem bananaItem)
        {
            _bananaItem = bananaItem;
            _folderItems = new ObservableCollection<BananaBabyItem>();

#pragma warning disable RS0030 // Do not used banned APIs
            _ = Task.Run(
                async () =>
                {
                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                    int i = 7;
                    var rand = new Random();
                    try
                    {
                        while (true)
                        {
                            double sample = rand.NextDouble();

                            if ((sample > 0.66 && _folderItems.Count < 10) || _folderItems.Count == 0)
                            {
                                _folderItems.Add(new BananaBabyItem(_bananaItem, $"Baby Banana {i++}", rand));
                            }
                            else if (sample > 0.33 && _folderItems.Count > 2)
                            {
                                _folderItems.RemoveAt(rand.Next(_folderItems.Count));
                            }
                            else
                            {
                                _folderItems[rand.Next(_folderItems.Count)].Randomize(rand);
                            }

                            await Task.Delay(10);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                });
#pragma warning restore RS0030 // Do not used banned APIs
        }

        public bool HasItems => true;

        public IEnumerable Items => _folderItems;

        public object SourceItem => _bananaItem;
    }

    internal sealed class BananaBabyItem : BaseItem
    {
        private static readonly ImageMoniker[] s_iconMonikers =
        {
            KnownMonikers.CodeInformation, KnownMonikers.Bug, KnownMonikers.ErrorBarChart,
            KnownMonikers.CloudWarning, KnownMonikers.DocumentGroup, KnownMonikers.USB,
            KnownMonikers.CheckedIn, KnownMonikers.Database
        };

        private static readonly ImageMoniker[] s_stateMonikers =
        {
            KnownMonikers.State, KnownMonikers.StateIndicator, KnownMonikers.StatusAlert,
            KnownMonikers.StatusError, KnownMonikers.StatusWarning, KnownMonikers.StatusHelp,
            KnownMonikers.StatusOK, KnownMonikers.StatusHelp, KnownMonikers.StatusRunning,
            KnownMonikers.StatusReady, KnownMonikers.StatusRunning, KnownMonikers.StatusStopped
        };

        private static readonly ImageMoniker[] s_overlayMonikers =
        {
            KnownMonikers.OverlayLock, KnownMonikers.OverlayAlert, KnownMonikers.OverlayError,
            KnownMonikers.OverlayExcluded, KnownMonikers.OverlayFriend, KnownMonikers.OverlayLoginDisabled,
            KnownMonikers.OverlayNo, KnownMonikers.OverlayOffline, KnownMonikers.OverlayOnline,
            KnownMonikers.OverlayPolicy, KnownMonikers.OverlayProperty
        };

        private static readonly FontStyle[] s_fontStyles =
        {
            FontStyles.Normal, FontStyles.Italic, FontStyles.Oblique
        };

        private static readonly FontWeight[] s_fontWeights =
        {
            FontWeights.Normal, FontWeights.Bold, FontWeights.Light
        };

        public BananaBabyItem(
            BananaItem parentItem,
            string text,
            Random random
//            IContextMenuController contextMenuController
            )
            : base(text)
        {
            ParentItem = parentItem;
            Randomize(random);
//            ContextMenuController = contextMenuController;
        }

        private ImageMoniker _icon;
        private ImageMoniker _stateIcon;
        private ImageMoniker _overlayIcon;
        private FontStyle _fontStyle;
        private FontWeight _fontWeight;

        public override ImageMoniker IconMoniker => _icon;

        public override ImageMoniker ExpandedIconMoniker => KnownMonikers.CodeInformation;

        public override ImageMoniker OverlayIconMoniker => _overlayIcon;

        public override ImageMoniker StateIconMoniker => _stateIcon;

        public override FontStyle FontStyle => _fontStyle;

        public override FontWeight FontWeight => _fontWeight;

        public BananaItem ParentItem { get; }

//        public override IContextMenuController? ContextMenuController { get; }

        public override object GetBrowseObject() => new BrowseObject(this);

        public override event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            // TODO cache event args by name
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Randomize(Random random)
        {
            _icon = s_iconMonikers[random.Next(s_iconMonikers.Length)];
            _stateIcon = s_stateMonikers[random.Next(s_stateMonikers.Length)];
            _overlayIcon = s_overlayMonikers[random.Next(s_overlayMonikers.Length)];
            _fontStyle = s_fontStyles[random.Next(s_fontStyles.Length)];
            _fontWeight = s_fontWeights[random.Next(s_fontWeights.Length)];
            
            OnPropertyChanged(nameof(IconMoniker));
            OnPropertyChanged(nameof(OverlayIconMoniker));
            OnPropertyChanged(nameof(StateIconMoniker));
            OnPropertyChanged(nameof(FontStyle));
            OnPropertyChanged(nameof(FontWeight));
        }

        internal class BrowseObject : BrowseObjectBase
        {
            public BrowseObject(BananaBabyItem bananaItem) => Item = bananaItem;

            public override string GetClassName() => "Baby Class Name";

            public override string GetComponentName() => "Baby Component Name";

            //[BrowseObjectDisplayName(nameof(Resources.AnalyzersNodeName))]
            [System.ComponentModel.DisplayName("Color")]
            [Description("What hue does your banana have?")]
            public string Color { get; } = "Yellow";

            //[BrowseObjectDisplayName("Length")]
            [System.ComponentModel.DisplayName("Length")]
            [Description("How long is your banana?")]
            public double Length { get; set; } = 1.23;

            [Browsable(false)]
            public BananaBabyItem Item { get; }
        }
    }

    /// <summary>
    /// Abstract base class for a custom node in Solution Explorer. This utilizes the core
    /// SolutionExplorer extensibility similar to 
    /// Microsoft.VisualStudio.Shell.TreeNavigation.HierarchyProvider.dll and
    /// Microsoft.VisualStudio.Shell.TreeNavigation.GraphProvider.dll.
    /// </summary>
    internal abstract class BaseItem :
        BrowseObjectBase,
        INotifyPropertyChanged,
        IPrioritizedComparable,
        IInteractionPatternProvider,
        ITreeDisplayItem,
        IInvocationPattern,
        IContextMenuPattern,
//        ISupportExpansionEvents,
//        ISupportExpansionState,
        IDragDropSourcePattern,
//        IDragDropTargetPattern,
        IBrowsablePattern,
        ISupportDisposalNotification
//        IRenamePattern
    {
        private static readonly HashSet<Type> s_supportedPatterns = new HashSet<Type>
        {
            typeof(ITreeDisplayItem),
            typeof(IInvocationPattern),
            typeof(IContextMenuPattern),
            typeof(ISupportExpansionEvents),
            typeof(ISupportExpansionState),
            typeof(IDragDropSourcePattern),
            typeof(IDragDropTargetPattern),
            typeof(IBrowsablePattern),
            typeof(ISupportDisposalNotification),
            typeof(IRenamePattern)
        };

        public virtual event PropertyChangedEventHandler PropertyChanged { add { } remove { } }

        protected BaseItem(string name) => Text = name;

//        public IEnumerable<string> Children => Enumerable.Empty<string>();
//        public IEnumerable<string> Children => new[] { "Bob", "Alice" };
//        public virtual bool IsExpandable => true;

        public virtual FontStyle FontStyle => FontStyles.Normal;

        public virtual FontWeight FontWeight => FontWeights.Normal;

        public virtual ImageSource? Icon => null;

        public virtual ImageMoniker IconMoniker => default;

        public virtual ImageSource? ExpandedIcon => null;

        public virtual ImageMoniker ExpandedIconMoniker => default;

        public bool AllowIconTheming => true;

        public bool AllowExpandedIconTheming => true;

        public bool IsCut => false;

        public ImageSource? OverlayIcon => null;

        public virtual ImageMoniker OverlayIconMoniker => default;

        public ImageSource? StateIcon => null;

        public virtual ImageMoniker StateIconMoniker => default;

        public virtual string? StateToolTipText => null;

        public override string ToString() => Text;

        public string Text { get; }

        public object? ToolTipContent => null;

        public string ToolTipText => Text;

        public TPattern? GetPattern<TPattern>() where TPattern : class
        {
            if (!IsDisposed)
            {
                if (s_supportedPatterns.Contains(typeof(TPattern)))
                {
                    return this as TPattern;
                }
                else if (typeof(TPattern) != typeof(IVsHierarchyItem))
                {
                    Trace.WriteLine($"Unknown pattern {typeof(TPattern)}");
                }
            }
            else
            {
                // If this item has been deleted, it no longer supports any patterns
                // other than ISupportDisposalNotification.
                // It's valid to use GetPattern on a deleted item, but there are no
                // longer any pattern contracts it fulfills other than the contract
                // that reports the item as a dead ITransientObject.
                if (typeof(TPattern) == typeof(ISupportDisposalNotification))
                {
                    return this as TPattern;
                }
            }

            return null;
        }

        public bool CanPreview => false;

        public virtual IInvocationController? InvocationController => null;

        public virtual IContextMenuController? ContextMenuController => null;

        public IDragDropSourceController? DragDropSourceController => null;

        public virtual object GetBrowseObject() => this;

        public bool IsDisposed => false;

        public int Priority => 0;

        public int CompareTo(object obj) => 1;
    }

    /// <summary>
    /// The attribute used for adding localized display names to properties
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    internal sealed class BrowseObjectDisplayNameAttribute : System.ComponentModel.DisplayNameAttribute
    {
        private readonly string _key;

        public BrowseObjectDisplayNameAttribute(string key)
        {
            _key = key;
        }

        public override string DisplayName
        {
            get
            {
                string name = base.DisplayName;

                if (name.Length == 0)
                {
                    name = DisplayNameValue = Resources.ResourceManager.GetString(_key, System.Globalization.CultureInfo.CurrentUICulture);
                }

                return name;
            }
        }
    }

    /// <summary>
    /// This is a slightly modified copy of Microsoft.VisualStudio.Shell.LocalizableProperties.
    /// http://index/#Microsoft.VisualStudio.Shell.12.0/LocalizableProperties.cs.html
    /// Unfortunately we can't reuse that class because the GetComponentName method on
    /// it is not virtual, so we can't provide a name string for the VS Property Grid's
    /// combo box (which shows ComponentName in bold and ClassName in regular to the
    /// right from it)
    /// </summary>
    [ComVisible(true)]
    internal abstract class BrowseObjectBase : ICustomTypeDescriptor
    {
        [Browsable(false)]
        public string ExtenderCATID => "";

        public AttributeCollection GetAttributes() => TypeDescriptor.GetAttributes(this, true);

        public EventDescriptor GetDefaultEvent() => TypeDescriptor.GetDefaultEvent(this, true);

        public PropertyDescriptor GetDefaultProperty() => TypeDescriptor.GetDefaultProperty(this, true);

        public object GetEditor(Type editorBaseType) => TypeDescriptor.GetEditor(this, editorBaseType, true);

        public EventDescriptorCollection GetEvents() => TypeDescriptor.GetEvents(this, true);

        public EventDescriptorCollection GetEvents(Attribute[] attributes) => TypeDescriptor.GetEvents(this, attributes, true);

        public object GetPropertyOwner(PropertyDescriptor pd) => this;

        public PropertyDescriptorCollection GetProperties() => GetProperties(null);

        public PropertyDescriptorCollection GetProperties(Attribute[]? attributes)
        {
            PropertyDescriptorCollection props = TypeDescriptor.GetProperties(this, attributes, true);

            var newList = new PropertyDescriptor[props.Count];

            for (int i = 0; i < props.Count; i++)
            {
                newList[i] = CreateDesignPropertyDescriptor(props[i]);
            }

            return new PropertyDescriptorCollection(newList);
        }

        public virtual DesignPropertyDescriptor CreateDesignPropertyDescriptor(PropertyDescriptor p) => new DesignPropertyDescriptor(p);

        public virtual string GetComponentName() => TypeDescriptor.GetComponentName(this, true);

        public virtual TypeConverter GetConverter() => TypeDescriptor.GetConverter(this, true);

        public virtual string GetClassName() => GetType().FullName;
    }
}
