using System.Collections.Immutable;
using System.Linq;
using Xunit;

namespace NuGet.Common.Test
{
    public class MSBuildStringUtilityTests
    {
        [Fact]
        public void GetDistinctNuGetLogCodesOrDefault_SameLogCodes()
        {
            // Arrange
            ImmutableArray<NuGetLogCode> logCodes1 = [NuGetLogCode.NU1000, NuGetLogCode.NU1001];
            ImmutableArray<NuGetLogCode> logCodes2 = [NuGetLogCode.NU1001, NuGetLogCode.NU1000];

            ImmutableArray<ImmutableArray<NuGetLogCode>> logCodesList = [logCodes1, logCodes2];

            // Act
            var result = MSBuildStringUtility.GetDistinctNuGetLogCodesOrDefault(logCodesList);

            // Assert
            Assert.Equal(2, result.Count());
            Assert.True(result.All(logCodes2.Contains));
        }

        [Fact]
        public void GetDistinctNuGetLogCodesOrDefault_EmptyLogCodes()
        {
            // Arrange
            ImmutableArray<ImmutableArray<NuGetLogCode>> logCodesList = [];

            // Act
            var result = MSBuildStringUtility.GetDistinctNuGetLogCodesOrDefault(logCodesList);

            // Assert
            Assert.Equal(0, result.Count());
        }

        [Fact]
        public void GetDistinctNuGetLogCodesOrDefault_DiffLogCodes()
        {
            // Arrange
            ImmutableArray<NuGetLogCode> logCodes1 = [NuGetLogCode.NU1000];
            ImmutableArray<NuGetLogCode> logCodes2 = [NuGetLogCode.NU1001, NuGetLogCode.NU1000];

            ImmutableArray<ImmutableArray<NuGetLogCode>> logCodesList = [logCodes1, logCodes2];

            // Act
            var result = MSBuildStringUtility.GetDistinctNuGetLogCodesOrDefault(logCodesList);

            // Assert
            Assert.Equal(0, result.Count());
        }

        [Fact]
        public void GetDistinctNuGetLogCodesOrDefault_OneDefaultCode()
        {
            // Arrange
            ImmutableArray<NuGetLogCode> logCodes1 = [NuGetLogCode.NU1001, NuGetLogCode.NU1000];

            ImmutableArray<ImmutableArray<NuGetLogCode>> logCodesList = [default, logCodes1];

            // Act
            var result = MSBuildStringUtility.GetDistinctNuGetLogCodesOrDefault(logCodesList);

            // Assert
            Assert.Equal(0, result.Count());
        }

        [Fact]
        public void GetDistinctNuGetLogCodesOrDefault_AllDefaultCodes()
        {
            // Arrange
            ImmutableArray<ImmutableArray<NuGetLogCode>> logCodesList = [default, default];

            // Act
            var result = MSBuildStringUtility.GetDistinctNuGetLogCodesOrDefault(logCodesList);

            // Assert
            Assert.Equal(0, result.Count());
        }

        [Fact]
        public void GetDistinctNuGetLogCodesOrDefault_DefaultCodesAfterFirst()
        {
            // Arrange
            ImmutableArray<NuGetLogCode> logCodes1 = [NuGetLogCode.NU1001, NuGetLogCode.NU1000];
            ImmutableArray<ImmutableArray<NuGetLogCode>> logCodesList = [logCodes1, default];

            // Act
            var result = MSBuildStringUtility.GetDistinctNuGetLogCodesOrDefault(logCodesList);

            // Assert
            Assert.Equal(0, result.Count());
        }

        [Theory]
        [InlineData("true", true)]
        [InlineData("false", false)]
        [InlineData("", null)]
        [InlineData(null, null)]
        public void GetBooleanOrNullTests(string value, bool? expected)
        {
            // Act
            bool? result = MSBuildStringUtility.GetBooleanOrNull(value);

            // Assert
            Assert.Equal(expected, result);
        }
    }
}
