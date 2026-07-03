# Vector Serialization

Serialization helpers persist QRS and related hex utility values with binary readers and writers.

## Binary Reading

- Read `VectorQRS` values.
- Read `VectorQRSInt` values.
- Validate serialized sixfold angle values before returning them.

## Binary Writing

- Write `VectorQRS` values.
- Write `VectorQRSInt` values.
- Write sixfold angle values in their compact enum representation.

## Validation

- Null readers and writers are rejected.
- Invalid enum payloads fail during reading.
