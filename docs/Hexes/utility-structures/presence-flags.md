# Presence Flags

Presence flags describe which positions exist in partial grouped values.

## `TripletPresenceFlags`

- Marks `Main`.
- Marks `Left`.
- Marks `Right`.
- Supports an `All` value for complete triplets.

## `SeptupletPresenceFlags`

- Marks `Main`.
- Marks six adjacent positions.
- Supports an `All` value for complete septuplets.

## Boundary Handling

- Partial grids use flags when neighboring cells are outside the field.
- Partial maps use flags when adjacency is logically known but not present in bounds.
