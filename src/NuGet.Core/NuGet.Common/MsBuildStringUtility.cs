// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace NuGet.Common
{
    public static class MSBuildStringUtility
    {
        /// <summary>
        /// Split on ; and trim. Null or empty inputs will return an
        /// empty array.
        /// </summary>
        public static string[] Split(string s)
        {
            return Split(s, ';');
        }

        /// <summary>
        /// Split on ; and trim. Null or empty inputs will return an
        /// empty array.
        /// </summary>
        public static string[] Split(string s, params char[] chars)
        {
            if (!string.IsNullOrEmpty(s))
            {
                // Split on ; and trim all entries
                // After trimming remove any entries that are now empty due to trim.
                return s.Split(chars)
                    .Select(entry => entry.Trim())
                    .Where(entry => entry.Length != 0)
                    .ToArray();
            }
            return Array.Empty<string>();
        }

        /// <summary>
        /// Trims the provided string and converts empty strings to null.
        /// </summary>
        public static string? TrimAndGetNullForEmpty(string? s)
        {
            if (s == null)
            {
                return null;
            }

            s = s.Trim();

            return s.Length == 0 ? null : s;
        }

        /// <summary>
        /// Trims the provided strings and excludes empty or null strings.
        /// </summary>
        public static string[] TrimAndExcludeNullOrEmpty(string?[]? strings)
        {
            if (strings == null)
            {
                return Array.Empty<string>();
            }

            return strings
                .Select(s => TrimAndGetNullForEmpty(s))
                .Where(s => s != null)
                .Cast<string>()
                .ToArray();
        }

        /// <summary>
        /// True if the property is set to true
        /// </summary>
        public static bool IsTrue(string? value)
        {
            return bool.TrueString.Equals(TrimAndGetNullForEmpty(value), StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// True if the property is set to true or empty.
        /// </summary>
        public static bool IsTrueOrEmpty(string? value)
        {
            return TrimAndGetNullForEmpty(value) == null || IsTrue(value);
        }

        /// <summary>
        /// Parses the specified string as a comma or semicolon delimited list of NuGet log codes and ignores unknown codes.
        /// </summary>
        /// <param name="s">A comma or semicolon delimited list of NuGet log codes.</param>
        /// <returns>An <see cref="IList{T}" /> containing the <see cref="NuGetLogCode" /> values that were successfully parsed from the specified string.</returns>
        public static ImmutableArray<NuGetLogCode> GetNuGetLogCodes(string s)
        {
            // The Split() method already checks for an empty string and returns Array.Empty<string>().
            string[] split = MSBuildStringUtility.Split(s, ';', ',');

            if (split.Length == 0)
            {
                return [];
            }

            NuGetLogCode[]? logCodes = null;
            int index = 0;

            for (int i = 0; i < split.Length; i++)
            {
                if (split[i].StartsWith("NU", StringComparison.OrdinalIgnoreCase) &&
                    Enum.TryParse(value: split[i], ignoreCase: true, out NuGetLogCode logCode))
                {
                    if (logCodes == null)
                    {
                        logCodes = ArrayPool<NuGetLogCode>.Shared.Rent(split.Length);
                    }
                    else if (logCodes.Length == index)
                    {
                        var oldItems = logCodes;

                        logCodes = ArrayPool<NuGetLogCode>.Shared.Rent(logCodes.Length * 2);
                        oldItems.CopyTo(logCodes, index: 0);

                        ArrayPool<NuGetLogCode>.Shared.Return(oldItems);
                    }

                    logCodes[index++] = logCode;
                }
            }

            if (logCodes == null)
            {
                return [];
            }

            var retVal = logCodes.AsSpan(0, index).ToImmutableArray();
            ArrayPool<NuGetLogCode>.Shared.Return(logCodes);

            return retVal;
        }

        /// <summary>
        /// Convert the provided string to a boolean, or return null if the value can't be parsed as a boolean.
        /// </summary>
        public static bool? GetBooleanOrNull(string? value)
        {
            if (bool.TryParse(value, out var result))
            {
                return result;
            }

            return null;
        }

        /// <summary>
        /// Convert the provided string to MSBuild style.
        /// </summary>
        public static string? Convert(string? value)
        {
            if (value == null)
            {
                return null;
            }

            return value.Replace(',', ';');
        }

        /// <summary>
        /// Return empty list of NuGetLogCode if all lists of NuGetLogCode are not the same.
        /// </summary>
        public static ImmutableArray<NuGetLogCode> GetDistinctNuGetLogCodesOrDefault(ImmutableArray<ImmutableArray<NuGetLogCode>> nugetLogCodeLists)
        {
            if (nugetLogCodeLists.Length == 0)
            {
                return [];
            }

            ImmutableArray<NuGetLogCode> result = [];
            var first = true;

            foreach (ImmutableArray<NuGetLogCode> logCodeList in nugetLogCodeLists)
            {
                // If this is first item, assign it to result
                if (first)
                {
                    result = logCodeList;
                    first = false;
                }
                // Compare the rest items to the first one.
                else if (result == null || logCodeList == null || result.Length != logCodeList.Length || !result.All(logCodeList.Contains))
                {
                    return [];
                }
            }

            return result;
        }
    }
}
