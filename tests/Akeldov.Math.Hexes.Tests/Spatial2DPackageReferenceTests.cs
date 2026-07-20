#if SPATIAL2D_PACKAGE_REFERENCE
using System.Reflection;
using Akeldov.Math.Spatial2D;

namespace Akeldov.Math.Hexes.Tests;

public class Spatial2DPackageReferenceTests
{
    [Test]
    public void LoadedSpatial2DAssembly_HasRequestedPackageVersion()
    {
        Assembly testAssembly = typeof(Spatial2DPackageReferenceTests).Assembly;
        string expectedVersion = testAssembly
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .Single(attribute => attribute.Key == "ExpectedSpatial2DPackageVersion")
            .Value!;
        string? actualVersion = typeof(PointXY).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion;

        Assert.That(actualVersion, Is.Not.Null);
        Assert.That(
            actualVersion!.Split('+')[0],
            Is.EqualTo(expectedVersion.Split('+')[0]));
    }
}
#endif
