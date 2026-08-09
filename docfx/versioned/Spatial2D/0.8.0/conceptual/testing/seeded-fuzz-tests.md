# Seeded Fuzz Tests

Seeded property and fuzz tests check broader geometry invariants with reproducible inputs.

Use fixed seeds in `[TestCase]` and include the seed and iteration or scenario in failure messages.

Current examples include curve intersection fuzzing, rectangle property fuzzing, and Voronoi partition property fuzzing.
