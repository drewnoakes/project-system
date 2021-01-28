// Licensed to the .NET Foundation under one or more agreements. The .NET Foundation licenses this file to you under the MIT license. See the LICENSE.md file in the project root for more information.

using System;

namespace Microsoft.VisualStudio.ProjectSystem
{
    internal sealed class TargetFramework : IEquatable<TargetFramework?>
    {
        public static readonly TargetFramework Empty = new(string.Empty);

        /// <summary>
        /// Any represents all TFMs, no need to be localized, used only in internal data.
        /// </summary>
        public static readonly TargetFramework Any = new("any");

        public TargetFramework(
            string alias,
            string? targetFrameworkMoniker = null,
            string? targetFrameworkIdentifier = null,
            string? targetFrameworkVersion = null,
            string? targetFrameworkProfile = null,
            string? targetPlatformIdentifier = null,
            string? targetPlatformVersion = null)
        {
            Requires.NotNull(alias, nameof(alias));

            TargetFrameworkAlias = alias;
            TargetFrameworkMoniker = targetFrameworkMoniker;
            TargetFrameworkIdentifier = targetFrameworkIdentifier;
            TargetFrameworkVersion = targetFrameworkVersion;
            TargetFrameworkProfile = targetFrameworkProfile;
            TargetPlatformIdentifier = targetPlatformIdentifier;
            TargetPlatformVersion = targetPlatformVersion;
        }

        /// <summary>
        /// Gets the Target Framework alias. This is a string that can be one of the well known
        /// values set by the SDK (for example, "netcoreapp2.1") or a custom string set in the project file
        /// such that evaluation generates a custom mapping of values of the well known properties like
        /// "TargetFrameworkMoniker", "TargetFrameworkIdentifier", etc.
        /// </summary>
        public string TargetFrameworkAlias { get; }

        public string? TargetFrameworkMoniker { get; }
        
        public string? TargetFrameworkIdentifier { get; }
        
        public string? TargetFrameworkVersion { get; }
        
        public string? TargetFrameworkProfile { get; }
        
        public string? TargetPlatformIdentifier { get; }

        public string? TargetPlatformVersion { get; }

        public bool Equals(TargetFramework? other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return StringComparers.FrameworkIdentifiers.Equals(TargetFrameworkAlias, other.TargetFrameworkAlias) &&
                   StringComparers.FrameworkIdentifiers.Equals(TargetFrameworkMoniker, other.TargetFrameworkMoniker) &&
                   StringComparers.FrameworkIdentifiers.Equals(TargetFrameworkIdentifier, other.TargetFrameworkIdentifier) &&
                   StringComparers.FrameworkIdentifiers.Equals(TargetFrameworkVersion, other.TargetFrameworkVersion) &&
                   StringComparers.FrameworkIdentifiers.Equals(TargetFrameworkProfile, other.TargetFrameworkProfile) &&
                   StringComparers.FrameworkIdentifiers.Equals(TargetPlatformIdentifier, other.TargetPlatformIdentifier) &&
                   StringComparers.FrameworkIdentifiers.Equals(TargetPlatformVersion, other.TargetPlatformVersion);
        }

        public override bool Equals(object? obj)
        {
            return ReferenceEquals(this, obj) || obj is TargetFramework other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = StringComparer.OrdinalIgnoreCase.GetHashCode(TargetFrameworkAlias);
                hashCode = (hashCode * 397) ^ (TargetFrameworkMoniker != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(TargetFrameworkMoniker) : 0);
                hashCode = (hashCode * 397) ^ (TargetFrameworkIdentifier != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(TargetFrameworkIdentifier) : 0);
                hashCode = (hashCode * 397) ^ (TargetFrameworkVersion != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(TargetFrameworkVersion) : 0);
                hashCode = (hashCode * 397) ^ (TargetFrameworkProfile != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(TargetFrameworkProfile) : 0);
                hashCode = (hashCode * 397) ^ (TargetPlatformIdentifier != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(TargetPlatformIdentifier) : 0);
                hashCode = (hashCode * 397) ^ (TargetPlatformVersion != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(TargetPlatformVersion) : 0);
                return hashCode;
            }
        }

        public static bool operator ==(TargetFramework? left, TargetFramework? right) => Equals(left, right);

        public static bool operator !=(TargetFramework? left, TargetFramework? right) => !Equals(left, right);

        public override string ToString()
        {
            return TargetFrameworkAlias;
        }
    }
}
