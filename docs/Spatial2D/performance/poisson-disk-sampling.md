# Poisson Disk Sampling

Poisson disk sampling cost depends on field size, minimal distance, and `maxAttempts`.

Higher `maxAttempts` can produce denser sets, but it also increases candidate testing.

Variable distance fields add the cost of sampling the distance field for candidate points.
