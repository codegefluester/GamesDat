using GamesDat.Core.Attributes;
using GamesDat.Core.Helpers;
using GamesDat.Core.Telemetry.Sources.AssettoCorsa;
using System.Runtime.InteropServices;
using Xunit;

namespace GamesDat.Tests;

public class MetadataAttributeTests
{
    [Fact]
    public void GameIdAttribute_ValidGameId_StoresValue()
    {
        var attr = new GameIdAttribute("ACC");
        Assert.Equal("ACC", attr.GameId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void GameIdAttribute_InvalidGameId_ThrowsArgumentException(string? gameId)
    {
        Assert.Throws<ArgumentException>(() => new GameIdAttribute(gameId!));
    }

    [Fact]
    public void DataVersionAttribute_ValidVersions_StoresValues()
    {
        var attr = new DataVersionAttribute(1, 2, 3);

        Assert.Equal(1, attr.Major);
        Assert.Equal(2, attr.Minor);
        Assert.Equal(3, attr.Patch);
    }

    [Theory]
    [InlineData(-1, 0, 0)]
    [InlineData(256, 0, 0)]
    [InlineData(0, -1, 0)]
    [InlineData(0, 256, 0)]
    [InlineData(0, 0, -1)]
    [InlineData(0, 0, 65536)]
    public void DataVersionAttribute_InvalidVersions_ThrowsArgumentOutOfRangeException(int major, int minor, int patch)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new DataVersionAttribute(major, minor, patch));
    }

    [Fact]
    public void MetadataHelper_GetGameId_ACCPhysics_ReturnsACC()
    {
        var gameId = MetadataHelper.GetGameId<ACCPhysics>();
        Assert.Equal("ACC", gameId);
    }

    [Fact]
    public void MetadataHelper_GetGameId_ACCGraphics_ReturnsACC()
    {
        var gameId = MetadataHelper.GetGameId<ACCGraphics>();
        Assert.Equal("ACC", gameId);
    }

    [Fact]
    public void MetadataHelper_GetGameId_ACCStatic_ReturnsACC()
    {
        var gameId = MetadataHelper.GetGameId<ACCStatic>();
        Assert.Equal("ACC", gameId);
    }

    [Fact]
    public void MetadataHelper_GetGameId_ACCCombinedData_ReturnsACC()
    {
        var gameId = MetadataHelper.GetGameId<ACCCombinedData>();
        Assert.Equal("ACC", gameId);
    }

    [Fact]
    public void MetadataHelper_GetDataVersion_ACCPhysics_ReturnsVersion()
    {
        var (major, minor, patch) = MetadataHelper.GetDataVersion<ACCPhysics>();

        Assert.Equal(1, major);
        Assert.Equal(0, minor);
        Assert.Equal(0, patch);
    }

    [Fact]
    public void MetadataHelper_GetDataVersion_ACCGraphics_ReturnsVersion()
    {
        var (major, minor, patch) = MetadataHelper.GetDataVersion<ACCGraphics>();

        Assert.Equal(1, major);
        Assert.Equal(0, minor);
        Assert.Equal(0, patch);
    }

    [Fact]
    public void MetadataHelper_GetGameId_StructWithoutAttribute_ThrowsInvalidOperationException()
    {
        var ex = Assert.Throws<InvalidOperationException>(() =>
            MetadataHelper.GetGameId<StructWithoutAttributes>());

        Assert.Contains("missing the [GameId] attribute", ex.Message);
    }

    [Fact]
    public void MetadataHelper_GetDataVersion_StructWithoutAttribute_ThrowsInvalidOperationException()
    {
        var ex = Assert.Throws<InvalidOperationException>(() =>
            MetadataHelper.GetDataVersion<StructWithoutAttributes>());

        Assert.Contains("missing the [DataVersion] attribute", ex.Message);
    }

    [Fact]
    public void MetadataHelper_AllACCStructs_HaveBothAttributes()
    {
        // This test ensures all ACC structs are properly annotated
        var accTypes = new[]
        {
            typeof(ACCPhysics),
            typeof(ACCGraphics),
            typeof(ACCStatic),
            typeof(ACCCombinedData)
        };

        foreach (var type in accTypes)
        {
            var gameIdAttr = type.GetCustomAttributes(typeof(GameIdAttribute), false);
            var versionAttr = type.GetCustomAttributes(typeof(DataVersionAttribute), false);

            Assert.Single(gameIdAttr); // Should have exactly one GameIdAttribute
            Assert.Single(versionAttr); // Should have exactly one DataVersionAttribute
        }
    }

    [Fact]
    public void GameIdAttribute_AppliesToStruct_AttributeUsageCorrect()
    {
        var attr = typeof(GameIdAttribute)
            .GetCustomAttributes(typeof(AttributeUsageAttribute), true)
            .Cast<AttributeUsageAttribute>()
            .FirstOrDefault();

        Assert.NotNull(attr);
        Assert.Equal(AttributeTargets.Struct, attr.ValidOn);
    }

    [Fact]
    public void DataVersionAttribute_AppliesToStruct_AttributeUsageCorrect()
    {
        var attr = typeof(DataVersionAttribute)
            .GetCustomAttributes(typeof(AttributeUsageAttribute), true)
            .Cast<AttributeUsageAttribute>()
            .FirstOrDefault();

        Assert.NotNull(attr);
        Assert.Equal(AttributeTargets.Struct, attr.ValidOn);
    }

    // Test helper struct WITHOUT attributes for negative testing
    [StructLayout(LayoutKind.Sequential)]
    private struct StructWithoutAttributes
    {
        public int Value;
    }
}
