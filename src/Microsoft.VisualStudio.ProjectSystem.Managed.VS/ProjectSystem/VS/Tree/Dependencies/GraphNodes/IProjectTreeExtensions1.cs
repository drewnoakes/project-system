namespace Microsoft.VisualStudio.ProjectSystem.VS.Tree.Dependencies
{
    internal static class IProjectTreeExtensions1
    {
        /// <summary>
        /// Returns the first child node that contains the specified flags, or <see langword="null"/>.
        /// </summary>
        internal static IProjectTree GetChildWithFlags(this IProjectTree self, ProjectTreeFlags flags)
        {
            foreach (IProjectTree child in self.Children)
            {
                if (child.Flags.Contains(flags))
                {
                    return child;
                }
            }

            return null;
        }
    }
}
