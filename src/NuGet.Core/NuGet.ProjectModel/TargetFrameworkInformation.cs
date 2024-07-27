// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Immutable;
using NuGet.Common;
using NuGet.Frameworks;
using NuGet.LibraryModel;
using NuGet.Shared;

namespace NuGet.ProjectModel
{
    public class TargetFrameworkInformation : IEquatable<TargetFrameworkInformation>
    {
        public static FrozenDictionary<string, CentralPackageVersion> ToCentralPackageVersions(IEnumerable<KeyValuePair<string, CentralPackageVersion>> versions)
            => versions != null ? versions.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase) : FrozenDictionary<string, CentralPackageVersion>.Empty;

        public string TargetAlias { get; init; }

        public NuGetFramework FrameworkName { get; init; }

        public ImmutableArray<LibraryDependency> Dependencies { get; init; }

        /// <summary>
        /// A fallback PCL framework to use when no compatible items
        /// were found for <see cref="FrameworkName"/>.
        /// </summary>
        public ImmutableArray<NuGetFramework> Imports { get; init; }

        /// <summary>
        /// If True AssetTargetFallback behavior will be used for Imports.
        /// </summary>
        public bool AssetTargetFallback { get; init; }

        /// <summary>
        /// Display warnings when the Imports framework is used.
        /// </summary>
        public bool Warn { get; init; }

        /// <summary>
        /// List of dependencies that are not part of the graph resolution.
        /// </summary>
        public ImmutableArray<DownloadDependency> DownloadDependencies { get; init; }

        /// <summary>
        /// Package versions defined in the Central package versions management file. 
        /// </summary>
        public FrozenDictionary<string, CentralPackageVersion> CentralPackageVersions { get; init; }

        /// <summary>
        /// A set of unique FrameworkReferences
        /// </summary>
        public ImmutableHashSet<FrameworkDependency> FrameworkReferences { get; init; }

        /// <summary>
        /// The project provided runtime.json
        /// </summary>
        public string RuntimeIdentifierGraphPath { get; init; }

        public TargetFrameworkInformation()
        {
            TargetAlias = string.Empty;
            Dependencies = [];
            Imports = [];
            DownloadDependencies = [];
            CentralPackageVersions = ToCentralPackageVersions(versions: null);
            FrameworkReferences = ImmutableHashSet<FrameworkDependency>.Empty;
        }

        internal TargetFrameworkInformation(TargetFrameworkInformation cloneFrom)
        {
            TargetAlias = cloneFrom.TargetAlias;
            FrameworkName = cloneFrom.FrameworkName;
            Dependencies = cloneFrom.Dependencies;
            Imports = cloneFrom.Imports;
            AssetTargetFallback = cloneFrom.AssetTargetFallback;
            Warn = cloneFrom.Warn;
            DownloadDependencies = cloneFrom.DownloadDependencies;
            CentralPackageVersions = cloneFrom.CentralPackageVersions;
            FrameworkReferences = cloneFrom.FrameworkReferences;
            RuntimeIdentifierGraphPath = cloneFrom.RuntimeIdentifierGraphPath;
        }

        public override string ToString()
        {
            return FrameworkName.GetShortFolderName();
        }

        public override int GetHashCode()
        {
            var hashCode = new HashCodeCombiner();

            hashCode.AddObject(FrameworkName);
            hashCode.AddObject(AssetTargetFallback);
            hashCode.AddUnorderedSequence(Dependencies);
            hashCode.AddSequence((IReadOnlyList<NuGetFramework>)Imports);
            hashCode.AddObject(Warn);
            hashCode.AddUnorderedSequence(DownloadDependencies);
            hashCode.AddUnorderedSequence(FrameworkReferences);
            if (RuntimeIdentifierGraphPath != null)
            {
                hashCode.AddObject(PathUtility.GetStringComparerBasedOnOS().GetHashCode(RuntimeIdentifierGraphPath));
            }
            hashCode.AddUnorderedSequence(CentralPackageVersions.Values);
            hashCode.AddStringIgnoreCase(TargetAlias);
            return hashCode.CombinedHash;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as TargetFrameworkInformation);
        }

        public bool Equals(TargetFrameworkInformation other)
        {
            if (other == null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return EqualityUtility.EqualsWithNullCheck(FrameworkName, other.FrameworkName) &&
                   EqualityUtility.OrderedEquals(Dependencies, other.Dependencies, dependency => dependency.Name, StringComparer.OrdinalIgnoreCase) &&
                   Imports.SequenceEqualWithNullCheck(other.Imports) &&
                   Warn == other.Warn &&
                   AssetTargetFallback == other.AssetTargetFallback &&
                   EqualityUtility.OrderedEquals(DownloadDependencies, other.DownloadDependencies, e => e.Name, StringComparer.OrdinalIgnoreCase) &&
                   EqualityUtility.OrderedEquals(FrameworkReferences, other.FrameworkReferences, e => e.Name, ComparisonUtility.FrameworkReferenceNameComparer) &&
                   EqualityUtility.OrderedEquals(CentralPackageVersions.Values, other.CentralPackageVersions.Values, e => e.Name, StringComparer.OrdinalIgnoreCase) &&
                   PathUtility.GetStringComparerBasedOnOS().Equals(RuntimeIdentifierGraphPath, other.RuntimeIdentifierGraphPath) &&
                   StringComparer.OrdinalIgnoreCase.Equals(TargetAlias, other.TargetAlias);
        }

        /// <summary>
        /// Returns copy of this with specified versions defined in the Central package versions management file. 
        /// </summary>
        public TargetFrameworkInformation WithCentralPackageVersions(FrozenDictionary<string, CentralPackageVersion> versions)
        {
            versions ??= ToCentralPackageVersions(null);

            if (CentralPackageVersions == versions)
            {
                return this;
            }

            return new TargetFrameworkInformation(this)
            {
                CentralPackageVersions = versions
            };
        }

        public TargetFrameworkInformation WithUpdatedCentralPackageVersions(IEnumerable<KeyValuePair<string, CentralPackageVersion>> versions)
        {
            if (versions == null)
            {
                return this;
            }

            versions ??= ToCentralPackageVersions(null);

            if (CentralPackageVersions == versions)
            {
                return this;
            }

            return new TargetFrameworkInformation(this)
            {
                CentralPackageVersions = versions
            };
        }

        /// <summary>
        /// Returns copy of this with specified framework references. 
        /// </summary>
        public TargetFrameworkInformation WithFrameworkReferences(ImmutableHashSet<FrameworkDependency> references)
        {
            references ??= ImmutableHashSet<FrameworkDependency>.Empty;

            if (FrameworkReferences == references)
            {
                return this;
            }

            return new TargetFrameworkInformation(this)
            {
                FrameworkReferences = references
            };
        }

        /// <summary>
        /// Returns copy of this with specified download dependencies 
        /// </summary>
        public TargetFrameworkInformation WithDownloadDependencies(ImmutableArray<DownloadDependency> dependencies)
        {
            if (DownloadDependencies.Equals(dependencies))
            {
                return this;
            }

            return new TargetFrameworkInformation(this)
            {
                DownloadDependencies = dependencies
            };
        }

        /// <summary>
        /// Returns copy of this with specified asset target fallback
        /// </summary>
        public TargetFrameworkInformation WithAssetTargetFallback(bool assetTargetFallback)
        {
            if (AssetTargetFallback == assetTargetFallback)
            {
                return this;
            }

            return new TargetFrameworkInformation(this)
            {
                AssetTargetFallback = assetTargetFallback
            };
        }

        /// <summary>
        /// Returns copy of this with specified warn
        /// </summary>
        public TargetFrameworkInformation WithWarn(bool warn)
        {
            if (Warn == warn)
            {
                return this;
            }

            return new TargetFrameworkInformation(this)
            {
                Warn = warn
            };
        }

        /// <summary>
        /// Returns copy of this with specified runtime identifier graph path
        /// </summary>
        public TargetFrameworkInformation WithRuntimeIdentifierGraphPath(string runtimeIdentifierGraphPath)
        {
            if (RuntimeIdentifierGraphPath == runtimeIdentifierGraphPath)
            {
                return this;
            }

            return new TargetFrameworkInformation(this)
            {
                RuntimeIdentifierGraphPath = runtimeIdentifierGraphPath
            };
        }

        /// <summary>
        /// Returns copy of this with specified target alias
        /// </summary>
        public TargetFrameworkInformation WithTargetAlias(string targetAlias)
        {
            if (TargetAlias == targetAlias)
            {
                return this;
            }

            return new TargetFrameworkInformation(this)
            {
                TargetAlias = targetAlias
            };
        }

        /// <summary>
        /// Returns copy of this with specified framework name
        /// </summary>
        public TargetFrameworkInformation WithFrameworkName(NuGetFramework frameworkName)
        {
            // Use reference equality here as the NuGetFramework equality comparison allows for instances
            // of different types to be equal
            if (Object.ReferenceEquals(FrameworkName, frameworkName))
            {
                return this;
            }

            return new TargetFrameworkInformation(this)
            {
                FrameworkName = frameworkName
            };
        }

        /// <summary>
        /// Returns copy of this with specified imports
        /// </summary>
        public TargetFrameworkInformation WithImports(ImmutableArray<NuGetFramework> imports)
        {
            if (Imports.Equals(imports))
            {
                return this;
            }

            return new TargetFrameworkInformation(this)
            {
                Imports = imports
            };
        }

        /// <summary>
        /// Returns copy of this with specified dependencies 
        /// </summary>
        public TargetFrameworkInformation WithDependencies(ImmutableArray<LibraryDependency> dependencies)
        {
            if (Dependencies.Equals(dependencies))
            {
                return this;
            }

            return new TargetFrameworkInformation(this)
            {
                Dependencies = dependencies
            };
        }
    }
}
