# Stress Tests

Heavy stress tests live under:

```text
tests/Akeldov.Math.Spatial2D.Tests/Stress
```

Stress fixtures are marked with both `Category("Stress")` and `Explicit`, so normal test runs stay fast.

Run them separately before releases or deep algorithm changes.
