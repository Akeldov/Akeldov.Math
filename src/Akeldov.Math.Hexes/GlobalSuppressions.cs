// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("ApiDesign", "RS0016:Add public types and members to the declared API", Justification = "Already declared in PublicAPI.Shipped.txt. Float literal rounding and different numeric formatting in .NET Framework and .NET Roslyn hosts cause a textual mismatch.", Scope = "member", Target = "~F:Akeldov.Math.Hexes.Geometry.Constants.Apothem2Radius")]
[assembly: SuppressMessage("ApiDesign", "RS0016:Add public types and members to the declared API", Justification = "Already declared in PublicAPI.Shipped.txt. Float literal rounding and different numeric formatting in .NET Framework and .NET Roslyn hosts cause a textual mismatch.", Scope = "member", Target = "~F:Akeldov.Math.Hexes.Geometry.Constants.Deg60InRadians")]

// RS0017 is suppressed selectively by FloatConstantApiSuppressor in tools/Akeldov.Math.Hexes.Analyzers.
// It matches the two API entries and verifies that the public constants retain their shipped float values.
