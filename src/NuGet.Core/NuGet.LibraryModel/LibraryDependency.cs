// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using NuGet.Common;
using NuGet.Shared;
using NuGet.Versioning;

namespace NuGet.LibraryModel
{
    public class LibraryDependency : IEquatable<LibraryDependency>
    {
        public required LibraryRange LibraryRange { get; init; }

        public LibraryIncludeFlags IncludeType { get; init; } = LibraryIncludeFlags.All;

        public LibraryIncludeFlags SuppressParent { get; init; } = LibraryIncludeFlagUtils.DefaultSuppressParent;

        public ImmutableArray<NuGetLogCode> NoWarn { get; init; } = [];

        public string Name => LibraryRange.Name;

        /// <summary>
        /// True if the PackageReference is added by the SDK and not the user.
        /// </summary>
        public bool AutoReferenced { get; init; }

        /// <summary>
        /// True if the dependency has the version set through CentralPackageVersionManagement file.
        /// </summary>
        public bool VersionCentrallyManaged { get; init; }

        /// <summary>
        /// Information regarding if the dependency is direct or transitive.
        /// </summary>
        public LibraryDependencyReferenceType ReferenceType { get; init; } = LibraryDependencyReferenceType.Direct;

        public bool GeneratePathProperty { get; init; }

        public string? Aliases { get; init; }

        /// <summary>
        /// Gets or sets a value indicating a version override for any centrally defined version.
        /// </summary>
        public VersionRange? VersionOverride { get; init; }

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

        [SetsRequiredMembers]
        internal LibraryDependency(LibraryDependency other)
        {
            LibraryRange = other.LibraryRange;
            IncludeType = other.IncludeType;
            SuppressParent = other.SuppressParent;
            NoWarn = other.NoWarn;
            AutoReferenced = other.AutoReferenced;
            GeneratePathProperty = other.GeneratePathProperty;
            VersionCentrallyManaged = other.VersionCentrallyManaged;
            ReferenceType = other.ReferenceType;
            Aliases = other.Aliases;
            VersionOverride = other.VersionOverride;
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

        /// <summary>
        /// Merge the CentralVersion information to the package reference information.
        /// </summary>
        public static ImmutableArray<LibraryDependency> ApplyCentralVersionInformation(ImmutableArray<LibraryDependency> packageReferences, IReadOnlyDictionary<string, CentralPackageVersion> centralPackageVersions)
        {
            if (packageReferences.IsDefault)
            {
                throw new ArgumentNullException(nameof(packageReferences));
            }
            if (centralPackageVersions == null)
            {
                throw new ArgumentNullException(nameof(centralPackageVersions));
            }
            if (centralPackageVersions.Count == 0)
            {
                return packageReferences;
            }

            LibraryDependency[] result = new LibraryDependency[packageReferences.Length];
            for (int i = 0; i < packageReferences.Length; i++)
            {
                LibraryDependency d = packageReferences[i];
                if (!d.AutoReferenced && d.LibraryRange.VersionRange == null)
                {
                    if (d.VersionOverride != null)
                    {
                        var newLibraryRange = d.LibraryRange.WithVersionRange(d.VersionOverride);
                        d = d.WithLibraryRange(newLibraryRange);
                    }
                    else
                    {
                        if (centralPackageVersions.TryGetValue(d.Name, out CentralPackageVersion? centralPackageVersion))
                        {
                            var newLibraryRange = d.LibraryRange.WithVersionRange(centralPackageVersion.VersionRange);
                            d = d.WithLibraryRange(newLibraryRange);
                        }

                        d = d.WithVersionCentrallyManaged(true);
                    }
                }

                result[i] = d;
            }

            return ImmutableCollectionsMarshal.AsImmutableArray(result);
        }

        public LibraryDependency WithIncludeType(LibraryIncludeFlags includeType)
        {
            if (IncludeType == includeType)
            {
                return this;
            }

            return new LibraryDependency(this)
            {
                IncludeType = includeType
            };
        }

        public LibraryDependency WithSuppressParent(LibraryIncludeFlags suppressParent)
        {
            if (SuppressParent == suppressParent)
            {
                return this;
            }

            return new LibraryDependency(this)
            {
                SuppressParent = suppressParent
            };
        }

        public LibraryDependency WithVersionCentrallyManaged(bool versionCentrallyManaged)
        {
            if (VersionCentrallyManaged == versionCentrallyManaged)
            {
                return this;
            }

            return new LibraryDependency(this)
            {
                VersionCentrallyManaged = versionCentrallyManaged
            };
        }

        public LibraryDependency WithReferenceType(LibraryDependencyReferenceType referenceType)
        {
            if (ReferenceType == referenceType)
            {
                return this;
            }

            return new LibraryDependency(this)
            {
                ReferenceType = referenceType
            };
        }

        public LibraryDependency WithLibraryRange(LibraryRange libraryRange)
        {
            if (LibraryRange == libraryRange)
            {
                return this;
            }

            return new LibraryDependency(this)
            {
                LibraryRange = libraryRange
            };
        }
    }
}
