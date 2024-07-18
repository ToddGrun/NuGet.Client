// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using NuGet.Common;
using NuGet.Shared;
using NuGet.Versioning;

namespace NuGet.LibraryModel
{
    public class LibraryDependency : IEquatable<LibraryDependency>
    {
        public required LibraryRange LibraryRange { get; init; }

        public LibraryIncludeFlags IncludeType { get; set; } = LibraryIncludeFlags.All;

        public LibraryIncludeFlags SuppressParent { get; set; } = LibraryIncludeFlagUtils.DefaultSuppressParent;

        public ImmutableArray<NuGetLogCode> NoWarn { get; init; } = [];

        public string Name => LibraryRange.Name;

        /// <summary>
        /// True if the PackageReference is added by the SDK and not the user.
        /// </summary>
        public bool AutoReferenced { get; init; }

        /// <summary>
        /// True if the dependency has the version set through CentralPackageVersionManagement file.
        /// </summary>
        public bool VersionCentrallyManaged { get; set; }

        /// <summary>
        /// Information regarding if the dependency is direct or transitive.
        /// </summary>
        public LibraryDependencyReferenceType ReferenceType { get; set; } = LibraryDependencyReferenceType.Direct;

        public bool GeneratePathProperty { get; init; }

        public string? Aliases { get; init; }

        /// <summary>
        /// Gets or sets a value indicating a version override for any centrally defined version.
        /// </summary>
        public VersionRange? VersionOverride { get; set; }

        /// <summary>Initializes a new instance of the LibraryDependency class.</summary>
        /// <remarks>Required properties must be set when using this constructor.</remarks>
        public LibraryDependency()
        {
        }

        /// <summary>Initializes a new instance of the LibraryDependency class.</summary>
        /// <param name="libraryRange">The <see cref="NuGet.LibraryModel.LibraryRange"/> to use with the new instance.</param>
        /// <exception cref="ArgumentNullException"><paramref name="libraryRange"/> is <see langword="null"/></exception>
        [SetsRequiredMembers]
        public LibraryDependency(LibraryRange libraryRange) : this()
        {
            LibraryRange = libraryRange ?? throw new ArgumentNullException(nameof(libraryRange));
        }

        [SetsRequiredMembers]
        internal LibraryDependency(
            LibraryRange libraryRange,
            LibraryIncludeFlags includeType,
            LibraryIncludeFlags suppressParent,
            ImmutableArray<NuGetLogCode> noWarn,
            bool autoReferenced,
            bool generatePathProperty,
            bool versionCentrallyManaged,
            LibraryDependencyReferenceType libraryDependencyReferenceType,
            string? aliases,
            VersionRange? versionOverride)
        {
            LibraryRange = libraryRange;
            IncludeType = includeType;
            SuppressParent = suppressParent;
            NoWarn = noWarn;
            AutoReferenced = autoReferenced;
            GeneratePathProperty = generatePathProperty;
            VersionCentrallyManaged = versionCentrallyManaged;
            ReferenceType = libraryDependencyReferenceType;
            Aliases = aliases;
            VersionOverride = versionOverride;
        }

        public override string ToString()
        {
            // Explicitly call .ToString() to ensure string.Concat(string, string, string) overload is called.
            return LibraryRange.ToString() + " " + LibraryIncludeFlagUtils.GetFlagString(IncludeType);
        }

        public override int GetHashCode()
        {
            var hashCode = new HashCodeCombiner();

            hashCode.AddObject(LibraryRange);
            hashCode.AddStruct(IncludeType);
            hashCode.AddStruct(SuppressParent);
            hashCode.AddObject(AutoReferenced);

            foreach (var item in NoWarn)
            {
                hashCode.AddStruct(item);
            }

            hashCode.AddObject(GeneratePathProperty);
            hashCode.AddObject(VersionCentrallyManaged);
            hashCode.AddObject(Aliases);
            hashCode.AddStruct(ReferenceType);

            return hashCode.CombinedHash;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as LibraryDependency);
        }

        public bool Equals(LibraryDependency? other)
        {
            if (other == null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return AutoReferenced == other.AutoReferenced &&
                   EqualityUtility.EqualsWithNullCheck(LibraryRange, other.LibraryRange) &&
                   IncludeType == other.IncludeType &&
                   SuppressParent == other.SuppressParent &&
                   NoWarn.SequenceEqualWithNullCheck(other.NoWarn) &&
                   GeneratePathProperty == other.GeneratePathProperty &&
                   VersionCentrallyManaged == other.VersionCentrallyManaged &&
                   Aliases == other.Aliases &&
                   EqualityUtility.EqualsWithNullCheck(VersionOverride, other.VersionOverride) &&
                   ReferenceType == other.ReferenceType;
        }

        public LibraryDependency Clone()
        {
            var clonedLibraryRange = new LibraryRange(LibraryRange.Name, LibraryRange.VersionRange, LibraryRange.TypeConstraint);

            return new LibraryDependency(clonedLibraryRange, IncludeType, SuppressParent, NoWarn, AutoReferenced, GeneratePathProperty, VersionCentrallyManaged, ReferenceType, Aliases, VersionOverride);
        }

        /// <summary>
        /// Merge the CentralVersion information to the package reference information.
        /// </summary>
        public static void ApplyCentralVersionInformation(IList<LibraryDependency> packageReferences, IReadOnlyDictionary<string, CentralPackageVersion> centralPackageVersions)
        {
            if (packageReferences == null)
            {
                throw new ArgumentNullException(nameof(packageReferences));
            }
            if (centralPackageVersions == null)
            {
                throw new ArgumentNullException(nameof(centralPackageVersions));
            }
            if (centralPackageVersions.Count > 0)
            {
                foreach (LibraryDependency d in packageReferences.Where(d => !d.AutoReferenced && d.LibraryRange.VersionRange == null))
                {
                    if (d.VersionOverride != null)
                    {
                        d.LibraryRange.VersionRange = d.VersionOverride;

                        continue;
                    }

                    if (centralPackageVersions.TryGetValue(d.Name, out CentralPackageVersion? centralPackageVersion))
                    {
                        d.LibraryRange.VersionRange = centralPackageVersion.VersionRange;
                    }

                    d.VersionCentrallyManaged = true;
                }
            }
        }
    }
}
