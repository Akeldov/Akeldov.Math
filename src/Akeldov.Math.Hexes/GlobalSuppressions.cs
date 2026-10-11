// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("ApiDesign", "RS0016:Add public types and members to the declared API", Justification = "Already declared in PublicAPI.Shipped.txt. Float literal rounding and different numeric formatting in .NET Framework and .NET Roslyn hosts cause a textual mismatch.", Scope = "member", Target = "~F:Akeldov.Math.Hexes.Geometry.Constants.Apothem2Radius")]
[assembly: SuppressMessage("ApiDesign", "RS0016:Add public types and members to the declared API", Justification = "Already declared in PublicAPI.Shipped.txt. Float literal rounding and different numeric formatting in .NET Framework and .NET Roslyn hosts cause a textual mismatch.", Scope = "member", Target = "~F:Akeldov.Math.Hexes.Geometry.Constants.Deg60InRadians")]

// RS0017 is reported on API-file entries, so this suppression applies to the whole assembly.
[assembly: SuppressMessage("ApiDesign", "RS0017:Remove deleted types and members from the declared API", Justification = "Apothem2Radius and Deg60InRadians still exist with unchanged float values. Different numeric formatting in .NET Framework and .NET Roslyn hosts causes their shipped API entries to appear deleted.")]
